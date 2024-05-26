using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using TimeTable.Core.DbContext;
using TimeTable.Core.Dto;

namespace TimeTable.Controller;

[Route("api/[controller]")]
[ApiController]
public class TimeTableController : ControllerBase
{
    private readonly TimeTableContext _dbContext;

    public TimeTableController(TimeTableContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet(nameof(GetAllSemesters))]
    public async Task<IEnumerable<SemesterModel>> GetAllSemesters()
    {
        var semesters = await _dbContext.Semesters.ToListAsync();
        return semesters.Select(a => new SemesterModel
        {
            Name = a.Name,
            Id = a.SemesterId,
            StartDate = a.StartDate,
            EndDate = a.EndDate
        });
    }

    [HttpGet(nameof(GetLatestTimetable))]
    public async Task<IActionResult> GetLatestTimetable(int? roomId, int? teacherId, int? subjectId, DateTime? start,
        DateTime? end)
    {
        // Get the current semester
        var currentSemester = await _dbContext.Semesters.FirstOrDefaultAsync(s => s.IsCurrent);
        if (currentSemester == null)
            return NotFound("No current semester available.");

        // Query for the latest timetable within the current semester
        var query = _dbContext.TimeTables
            .Where(t => t.SemesterId == currentSemester.SemesterId)
            .SelectMany(t => t.Slots)
            .Include(s => s.Room)
            .Include(s => s.Teacher)
            .Include(s => s.Subject)
            .AsQueryable();

        // Filtering by IDs
        if (roomId.HasValue)
            query = query.Where(s => s.RoomId == roomId.Value);
        if (teacherId.HasValue)
            query = query.Where(s => s.TeacherId == teacherId.Value);
        if (subjectId.HasValue)
            query = query.Where(s => s.SubjectId == subjectId.Value);
        if (start.HasValue)
            query = query.Where(s => s.StartTime >= start.Value);
        if (end.HasValue)
            query = query.Where(s => s.EndTime <= end.Value);

        var slots = await query.ToListAsync();

        // Map to DTO
        var result = slots.Select(slot => new SlotModel
        {
            StartTime = slot.StartTime,
            Duration = slot.Duration,
            Teacher = slot.Teacher.Name,
            Room = slot.Room.Name,
            Subject = slot.Subject.Name
        });

        return Ok(result);
    }

    [HttpGet("ExportSlotsToExcel/{timetableId:int}")]
    public async Task<IActionResult> ExportSlotsToExcel(int timetableId)
    {
        var timetable = await _dbContext.TimeTables
            .Include(t => t.Slots)
            .ThenInclude(s => s.Subject)
            .Include(t => t.Slots)
            .ThenInclude(s => s.Teacher)
            .Include(t => t.Slots)
            .ThenInclude(s => s.Room)
            .FirstOrDefaultAsync(t => t.TimetableId == timetableId);

        if (timetable == null)
        {
            return NotFound();
        }

        // Set the license context
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Slots");

        worksheet.Cells[1, 1].Value = "Start";
        worksheet.Cells[1, 2].Value = "End";
        worksheet.Cells[1, 3].Value = "Room";
        worksheet.Cells[1, 4].Value = "Subject";
        worksheet.Cells[1, 5].Value = "Teacher";

        var row = 2;
        foreach (var slot in timetable.Slots)
        {
            worksheet.Cells[row, 1].Value = slot.StartTime;
            worksheet.Cells[row, 2].Value = slot.EndTime;
            worksheet.Cells[row, 3].Value = slot.Room.Name;
            worksheet.Cells[row, 4].Value = slot.Subject.Name;
            worksheet.Cells[row, 5].Value = slot.Teacher.Name;

            // Format the datetime columns
            worksheet.Cells[row, 1].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";
            worksheet.Cells[row, 2].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";

            row++;
        }

        var stream = new MemoryStream();
        package.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"Timetable_{timetableId}_Slots.xlsx";
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet(nameof(Generate))]
    public async Task<ActionResult<IEnumerable<SlotModel>?>> Generate(int semesterId)
    {
        var semester = await _dbContext.Semesters.FirstOrDefaultAsync(a => a.SemesterId == semesterId);
        if (semester == null) return BadRequest("Invalid semester ID.");

        var subjects = await _dbContext.Subjects
            .Include(a => a.Setting)
            .Include(a => a.Department)
            .Include(a => a.Slots)
            .Include(a => a.TeacherSubjects)
            .ThenInclude(a => a.Teacher)
            .AsSplitQuery()
            .ToListAsync();
        var rooms = await _dbContext.Rooms
            .Include(a => a.Slots)
            .ToListAsync();
        var teachers = await _dbContext.Teachers.Include(a => a.AvailabilityRules).ToListAsync();
        var students = await _dbContext.Students.ToListAsync();
        var departments = await _dbContext.Departments.ToListAsync();
        var colleges = await _dbContext.Colleges.ToListAsync();

        // Validate constraints before running the algorithm
        var validator = new TimetableValidator();
        var errors = validator.ValidateConstraints(subjects, rooms, teachers, departments, colleges, semester);
        if (errors.Any())
        {
            return BadRequest(new { Errors = errors });
        }

        var ga = new TimetableGeneticAlgorithm
        {
            Semester = semester
        };

        const int populationSize = 10;
        const int numGenerations = 100;
        const double mutationRate = 0.1;

        var timetable = ga.GeneticAlgorithm(subjects, rooms, teachers, students, populationSize, numGenerations,
            mutationRate, _dbContext);

        return Ok(timetable?.Slots.Select(a => new SlotModel
        {
            Teacher = a.Teacher.Name,
            Duration = a.Duration,
            StartTime = a.StartTime,
            Room = a.Room.Name,
            Subject = a.Subject.Name
        }));
    }
}
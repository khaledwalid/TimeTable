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

    [HttpGet("CheckCurrentSemesterTimetable")]
    public async Task<IActionResult> CheckCurrentSemesterTimetable()
    {
        // Get the current semester
        var currentSemester = await _dbContext.Semesters.FirstOrDefaultAsync(s => s.IsCurrent);
        if (currentSemester == null)
            return NotFound("No current semester available.");

        // Check if any timetable exists for the current semester
        var hasTimetable = await _dbContext.TimeTables
            .AnyAsync(t => t.SemesterId == currentSemester.SemesterId);

        return Ok(new { HasTimetable = hasTimetable });
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

    [HttpPost(nameof(GetLatestTimetable))]
    public async Task<IActionResult> GetLatestTimetable(TimeTableRequestModel model)
    {
        // Get the current semester
        var currentSemester = await _dbContext.Semesters.FirstOrDefaultAsync(s => s.IsCurrent);
        if (currentSemester == null)
            return NotFound("No current semester available.");

        // Query for the latest timetable within the current semester
        var latestTimetable = await _dbContext.TimeTables
            .Where(t => t.SemesterId == currentSemester.SemesterId)
            .OrderByDescending(t => t.Date) // Assuming 'Date' indicates when the timetable was generated
            .FirstOrDefaultAsync();

        if (latestTimetable == null)
            return NotFound("No timetable found for the current semester.");

        var query = _dbContext.Slots
            .Where(s => s.TimeTableId == latestTimetable.TimetableId)
            .Include(s => s.Room)
            .Include(s => s.Teacher)
            .Include(s => s.Subject)
            .AsQueryable();

        // Filtering by IDs
        if (model.RoomId.HasValue)
            query = query.Where(s => s.RoomId == model.RoomId.Value);
        if (model.TeacherId.HasValue)
            query = query.Where(s => s.TeacherId == model.TeacherId.Value);
        if (model.SubjectId.HasValue)
            query = query.Where(s => s.SubjectId == model.SubjectId.Value);
        if (model.Start.HasValue)
            query = query.Where(s => s.StartTime >= model.Start.Value);
        if (model.End.HasValue)
            query = query.Where(s => s.EndTime <= model.End.Value);

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
            .ThenInclude(s => s.Subject).ThenInclude(subject => subject.Setting)
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
        worksheet.Cells[1, 6].Value = "Type";

        var row = 2;
        foreach (var slot in timetable.Slots)
        {
            worksheet.Cells[row, 1].Value = slot.StartTime;
            worksheet.Cells[row, 2].Value = slot.EndTime;
            worksheet.Cells[row, 3].Value = slot.Room.Name;
            worksheet.Cells[row, 4].Value = slot.Subject.Name;
            worksheet.Cells[row, 5].Value = slot.Teacher.Name;
            worksheet.Cells[row, 6].Value = slot.Subject.Setting.Type;

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
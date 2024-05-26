using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using TimeTable.Core;
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
            Id = a.SemesterId
        });
    }

    [HttpGet(nameof(GetLatestTimetable))]
    public async Task<IEnumerable<SlotModel>?> GetLatestTimetable()
    {
        var latestTimetable = await _dbContext.TimeTables
            .OrderByDescending(t => t.Date)
            .Include(t => t.Slots)
            .ThenInclude(s => s.Subject)
            .ThenInclude(sub => sub.TeacherSubjects)
            .ThenInclude(ts => ts.Teacher)
            .Include(t => t.Slots)
            .ThenInclude(s => s.Room).Include(timeTable => timeTable.Slots).ThenInclude(slot => slot.Teacher)
            .FirstOrDefaultAsync();

        return latestTimetable?.Slots.Select(a => new SlotModel
        {
            Teacher = a.Teacher.Name,
            Duration = a.Duration,
            StartTime = a.StartTime,
            Room = a.Room.Name,
            Subject = a.Subject.Name
        });
    }

[HttpGet("ExportSlotsToExcel/{timetableId}")]
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
    public async Task<IEnumerable<SlotModel>?> Generate(int semesterId)
    {
        var semester = await _dbContext.Semesters.FirstOrDefaultAsync(a => a.SemesterId == semesterId);
        if (semester == null) return null;
        var ga = new TimetableGeneticAlgorithm
        {
            Semester = semester
        };
        // Generate initial population
        const int populationSize = 10;
        const int numGenerations = 100;
        const double mutationRate = 0.1;
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
        var timetable = ga.GeneticAlgorithm(subjects, rooms, teachers, students, populationSize, numGenerations,
            mutationRate, _dbContext);
        return timetable?.Slots.Select(a => new SlotModel
        {
            Teacher = a.Teacher.Name,
            Duration = a.Duration,
            StartTime = a.StartTime,
            Room = a.Room.Name,
            Subject = a.Subject.Name
        });
    }
}
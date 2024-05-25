using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeTable.Core;
using TimeTable.Core.DbContext;
using TimeTable.Core.Dto;

namespace TimeTable.Controller;

[Route("api/[controller]")]
[ApiController]
public class TimeTableController
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
            Day = a.Day,
            Subject = a.Subject.Name
        });
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
            Day = a.Day,
            Subject = a.Subject.Name
        });
    }
}
using Newtonsoft.Json;
using TimeTable.Core.Models;

namespace TimeTable.Core.DbContext;

using Microsoft.EntityFrameworkCore;

public class TimeTableContext : DbContext
{
    public DbSet<College> Colleges { get; set; } = null!;
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Teacher> Teachers { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<Subject> Subjects { get; set; } = null!;
    public DbSet<Setting> Settings { get; set; } = null!;
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Slot> Slots { get; set; } = null!;
    public DbSet<Semester> Semesters { get; set; } = null!;
    public DbSet<Models.TimeTable> TimeTables { get; set; } = null!;

    public TimeTableContext(DbContextOptions<TimeTableContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Models.TimeTable>()
            .HasOne(t => t.Semester)
            .WithMany(s => s.TimeTables)
            .HasForeignKey(t => t.SemesterId);
        // Define relationships if necessary
        modelBuilder.Entity<Teacher>()
            .HasMany(t => t.AvailabilityRules)
            .WithOne(ar => ar.Teacher)
            .HasForeignKey(ar => ar.TeacherId);
        modelBuilder.Entity<Teacher>()
            .HasOne(t => t.Department)
            .WithMany(d => d.Teachers)
            .HasForeignKey(t => t.DepartmentId);
        modelBuilder.Entity<Subject>()
            .HasMany(s => s.TeacherSubjects)
            .WithOne(ts => ts.Subject)
            .HasForeignKey(ts => ts.SubjectId);

        modelBuilder.Entity<TeacherSubject>()
            .HasKey(ts => new { ts.TeacherId, ts.SubjectId });

        // Additional seeding for other entities if necessary

        // Seed data for College
        modelBuilder.Entity<College>().HasData(
            new College { CollegeId = 1, Name = "College of Engineering" },
            new College { CollegeId = 2, Name = "College of Science" }
        );

        // Seed data for Department
        modelBuilder.Entity<Department>().HasData(
            new Department { DepartmentId = 1, Name = "Computer Science", CollegeId = 1 },
            new Department { DepartmentId = 2, Name = "Electrical Engineering", CollegeId = 1 },
            new Department { DepartmentId = 3, Name = "Biology", CollegeId = 2 }
        );

        // Seed data for Teacher and AvailabilityRules
        modelBuilder.Entity<Teacher>().HasData(
            new Teacher { TeacherId = 1, Name = "John Doe", DepartmentId = 1 },
            new Teacher { TeacherId = 2, Name = "Jane Smith", DepartmentId = 2 }
        );

        modelBuilder.Entity<AvailabilityRule>().HasData(
            new AvailabilityRule
            {
                AvailabilityRuleId = 1, TeacherId = 1, Day = DayOfWeek.Monday,
                StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(10, 0, 0)
            },
            new AvailabilityRule
            {
                AvailabilityRuleId = 2, TeacherId = 2, Day = DayOfWeek.Monday,
                StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(10, 0, 0)
            }
        );

        // Seed data for Room
        modelBuilder.Entity<Room>().HasData(
            new Room { RoomId = 1, Name = "Room 101", Type = "Lecture Hall", Capacity = 100, Zone = "Shared" },
            new Room { RoomId = 2, Name = "Room 102", Type = "Classroom", Capacity = 30, Zone = "Male" },
            new Room { RoomId = 3, Name = "Room 103", Type = "Classroom", Capacity = 30, Zone = "Female" }
        );

        var twoDay = new List<TimeSpan>
        {
            new(8, 0, 0),
            new(09, 30, 0),
            new(11, 0, 0),
            new(12, 30, 0),
            new(14, 0, 0),
            new(15, 30, 00),
            new(17, 0, 0)
        };
        var oneDay = new List<TimeSpan>
        {
            new(8, 0, 0),
            new(11, 0, 0),
            new(14, 0, 0),
            new(17, 0, 0)
        };

        modelBuilder.Entity<Setting>().HasData(
            new Setting
            {
                // 2 day
                SettingId = 1,
                Duration = new TimeSpan(1, 15, 0),
                SpecificStartTimesJson = JsonConvert.SerializeObject(new Dictionary<int, List<TimeSpan>>
                {
                    { 1, twoDay },
                    { 2, twoDay },
                    { 3, twoDay },
                    { 4, twoDay },
                })
            },
            new Setting
            {
                // one day
                SettingId = 2,
                Duration = new TimeSpan(2, 15, 0),
                SpecificStartTimesJson = JsonConvert.SerializeObject(new Dictionary<int, List<TimeSpan>>
                {
                    { 1, oneDay },
                    { 2, oneDay },
                    { 3, oneDay },
                    { 4, oneDay },
                    { 5, oneDay },
                })
            }
        );

        // Seed data for Subject
        modelBuilder.Entity<Subject>().HasData(
            new Subject
            {
                SubjectId = 1, Name = "Introduction to Programming", RequiredRoomType = "Lecture Hall",
                RequiredCapacity = 50,
                DepartmentId = 1, SettingId = 1
            },
            new Subject
            {
                SubjectId = 2, Name = "C++", RequiredRoomType = "Lecture Hall",
                RequiredCapacity = 50,
                DepartmentId = 1, SettingId = 1
            },
            new Subject
            {
                SubjectId = 3, Name = "R+", RequiredRoomType = "Lecture Hall",
                RequiredCapacity = 50,
                DepartmentId = 1, SettingId = 1
            },
            new Subject
            {
                SubjectId = 4, Name = "Android", RequiredRoomType = "Lecture Hall",
                RequiredCapacity = 50,
                DepartmentId = 1, SettingId = 1
            },
            new Subject
            {
                SubjectId = 5, Name = "Circuit Analysis", RequiredRoomType = "Classroom", RequiredCapacity = 20,
                DepartmentId = 2, SettingId = 2
            }
        );

        // Seed data for Student
        modelBuilder.Entity<Student>().HasData(
            new Student { StudentId = 1, Name = "Alice Johnson", Gender = "Female" },
            new Student { StudentId = 2, Name = "Bob Smith", Gender = "Male" }
        );

        // Seed data for Semester
        modelBuilder.Entity<Semester>().HasData(
            new Semester
            {
                SemesterId = 1, Name = "Spring 2024", StartDate = new DateTime(2024, 5, 20),
                EndDate = new DateTime(2024, 8, 20)
            },
            new Semester
            {
                SemesterId = 2, Name = "Fall 2024", StartDate = new DateTime(2024, 9, 20),
                EndDate = new DateTime(2024, 12, 20)
            }
        );

        // Seed data for TeacherSubject relationship
        modelBuilder.Entity<TeacherSubject>().HasData(
            new TeacherSubject { TeacherId = 1, SubjectId = 1 },
            new TeacherSubject { TeacherId = 2, SubjectId = 2 },
            new TeacherSubject { TeacherId = 2, SubjectId = 3 },
            new TeacherSubject { TeacherId = 1, SubjectId = 4 },
            new TeacherSubject { TeacherId = 2, SubjectId = 5 }
        );

        // Additional seeding for other entities
    }
}
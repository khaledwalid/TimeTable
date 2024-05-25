using System.ComponentModel.DataAnnotations;

namespace TimeTable.Core.Models;

public class TeacherSubject
{
    [Key] public int TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;

    [Key] public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;
}
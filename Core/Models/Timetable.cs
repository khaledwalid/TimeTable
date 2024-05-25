using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTable.Core.Models;

public class TimeTable
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int TimetableId { get; set; }

    [Required] public string Name { get; set; } = null!;

    [Required] public DateTime Date { get; set; }

    // Navigation property
    public ICollection<Slot> Slots { get; set; } = null!;

    // Foreign key property for Semester
    [Required] public int SemesterId { get; set; }

    // Navigation property for Semester
    [ForeignKey("SemesterId")] public Semester Semester { get; set; } = null!;
}
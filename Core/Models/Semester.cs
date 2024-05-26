using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTable.Core.Models;

public class Semester
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int SemesterId { get; set; }

    [Required] public string Name { get; set; } = null!;
    // New property to indicate current semester
    public bool IsCurrent { get; set; }
    [Required] public DateTime StartDate { get; set; }

    [Required] public DateTime EndDate { get; set; }
    public ICollection<TimeTable> TimeTables { get; set; } = null!;


    // Navigation property
    public ICollection<Slot> Slots { get; set; } = null!;
}
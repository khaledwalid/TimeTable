using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTable.Core.Models;

public class Student
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int StudentId { get; set; }

    [Required] public string Name { get; set; } = null!;

    [Required]
    public string Gender { get; set; } = null!; // Male or Female
    
    // Navigation property for slots
    public ICollection<Slot> Slots { get; set; } = null!;
}
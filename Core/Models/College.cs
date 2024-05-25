using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTable.Core.Models;

public class College
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int CollegeId { get; set; }

    [Required] public string Name { get; set; } = null!;

    // Navigation property
    public ICollection<Department> Departments { get; set; } = null!;
}
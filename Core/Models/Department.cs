using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTable.Core.Models
{
    public class Department
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        // Foreign key property
        public int CollegeId { get; set; }

        // Navigation properties
        public College College { get; set; } = null!;
        public ICollection<Subject> Subjects { get; set; } = null!;
        public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
    }
}
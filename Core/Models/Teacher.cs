using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTable.Core.Models
{
    public class Teacher
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int TeacherId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public ICollection<AvailabilityRule> AvailabilityRules { get; set; } = new List<AvailabilityRule>();

        // Foreign key property for Department
        [Required]
        public int DepartmentId { get; set; }

        // Navigation property for Department
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; } = null!;
    }
}
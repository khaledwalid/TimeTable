using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTable.Core.Models
{
    public class Subject
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int SubjectId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string RequiredRoomType { get; set; } = null!;

        [Required]
        public int RequiredCapacity { get; set; }

        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department Department { get; set; } = null!;

        public int SettingId { get; set; }

        [ForeignKey("SettingId")]
        public Setting Setting { get; set; } = null!;

        // Navigation properties
        public ICollection<TeacherSubject> TeacherSubjects { get; set; }
        public ICollection<Slot> Slots { get; set; }

        public Subject()
        {
            TeacherSubjects = new List<TeacherSubject>();
            Slots = new List<Slot>();
        }
    }
}
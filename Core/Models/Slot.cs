using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTable.Core.Models
{
    public class Slot
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int SlotId { get; set; }

        [Required]
        public int Day { get; set; }

        [Required]
        public DateTime StartTime { get; set; }
        public DateTime EndTime => StartTime.Add(Duration);

        [Required]
        public TimeSpan Duration { get; set; }

        // Foreign key properties
        [Required]
        public int TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public Teacher Teacher { get; set; }

        [Required]
        public int RoomId { get; set; }
        [ForeignKey("RoomId")]
        public Room Room { get; set; }

        [Required]
        public int SubjectId { get; set; }
        [ForeignKey("SubjectId")]
        public Subject Subject { get; set; }

        [Required]
        public int TimeTableId { get; set; }
        [ForeignKey("TimeTableId")]
        public TimeTable TimeTable { get; set; }
    }
}
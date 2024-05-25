using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTable.Core.Models;

public class AvailabilityRule
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int AvailabilityRuleId { get; set; }

    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;

    public DayOfWeek Day { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }
}
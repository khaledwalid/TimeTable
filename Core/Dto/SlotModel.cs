using System.ComponentModel.DataAnnotations;

namespace TimeTable.Core.Dto;

public class SlotModel
{
    [Required] public int Day { get; set; }
    [Required] public DateTime StartTime { get; set; }
    public DateTime EndTime => StartTime.Add(Duration);
    [Required] public TimeSpan Duration { get; set; }
    public string Teacher { get; set; }
    public string Room { get; set; }
    public string Subject { get; set; }
}
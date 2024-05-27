using System.ComponentModel.DataAnnotations;

public class SlotModel
{
    [Required] public DateTime StartTime { get; set; }
    public DateTime EndTime => StartTime.Add(Duration);
    [Required] public TimeSpan Duration { get; set; }
    public string Teacher { get; set; }
    public string Room { get; set; }
    public string Subject { get; set; }
    public string Department { get; set; }
    public string College { get; set; }
}
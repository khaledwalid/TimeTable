using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace TimeTable.Core.Models;

public class Setting
{
    public int SettingId { get; set; }

    public TimeSpan Duration { get; set; }

    // Specific start times for the subject
    [Required] public string SpecificStartTimesJson { get; set; } = null!;

    [NotMapped]
    public Dictionary<int, List<TimeSpan>> SpecificStartTimes
    {
        get => JsonConvert.DeserializeObject<Dictionary<int, List<TimeSpan>>>(SpecificStartTimesJson);
        set => SpecificStartTimesJson = JsonConvert.SerializeObject(value);
    }
}
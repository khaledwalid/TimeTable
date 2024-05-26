using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace TimeTable.Core.Models
{
    public class Setting
    {
        public int SettingId { get; set; }

        [Required]
        public string Type { get; set; } = null!; // "2-day" or "1-day"

        public TimeSpan Duration { get; set; }

        [Required]
        public string SpecificStartTimesJson { get; set; } = null!;

        [NotMapped]
        public List<TimeSpan> SpecificStartTimes
        {
            get => JsonConvert.DeserializeObject<List<TimeSpan>>(SpecificStartTimesJson);
            set => SpecificStartTimesJson = JsonConvert.SerializeObject(value);
        }
    }
}
using TimeTable.Core.Models;

namespace TimeTable.Core.Dto;

public class Schedule
{
    public List<Slot> Slots { get; init; } = null!;
    public int Fitness { get; set; }
}
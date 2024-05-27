namespace TimeTable.Core.Dto;

public class TimeTableRequestModel
{
    public int? RoomId { get; set; }
    public int? TeacherId { get; set; }
    public int? SubjectId { get; set; }
    public int? CollegeId { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
}
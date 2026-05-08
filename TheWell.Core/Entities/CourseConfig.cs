namespace TheWell.Core.Entities;

public class CourseConfig
{
    public Guid ConfigID { get; set; } = Guid.NewGuid();
    public DateOnly CourseStartDate { get; set; }
    public DateOnly CourseEndDate { get; set; }   // CourseStartDate + 60 days
    public Guid SetByAdminID { get; set; }
    public DateTime SetAt { get; set; } = DateTime.UtcNow;
}

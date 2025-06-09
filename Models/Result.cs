using EduSync.Models;

public class Result
{
    public Guid ResultId { get; set; }
    public Guid AssessmentId { get; set; }
    public Guid UserId { get; set; }
    public int Score { get; set; }
    public DateTime AttemptDate { get; set; }

    // Add this property
    public bool Published { get; set; }

    // Navigation properties
    public Assessment Assessment { get; set; }
    public User User { get; set; }
}

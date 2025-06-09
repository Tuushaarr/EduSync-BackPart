public class ResultDto
{
    public Guid ResultId { get; set; }
    public Guid AssessmentId { get; set; }
    public string AssessmentTitle { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public int Score { get; set; }
    public DateTime AttemptDate { get; set; }

    // Add this property
    public bool Published { get; set; }
}

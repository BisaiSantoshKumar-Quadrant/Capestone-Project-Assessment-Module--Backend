[ApiController]
[Route("api/ai/user-summary")]
public class AIController : ControllerBase
{
    private readonly YourDbContext _db;
    private readonly AzureOpenAIService _ai;

    public AIController(YourDbContext db, AzureOpenAIService ai)
    {
        _db = db;
        _ai = ai;
    }

    [HttpGet("{username}")]
    public async Task<IActionResult> GetUserSummary(string username)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return NotFound("User not found.");

        var exams = await _db.ExamResults.Where(e => e.UserId == user.Id).ToListAsync();

        var inputData = new
        {
            Name = user.FullName,
            Email = user.Email,
            Exams = exams.Select(e => new {
                e.Subject,
                e.Score,
                e.TotalMarks,
                e.DateTaken
            })
        };

        var prompt = $"Summarize this user data for the manager:\n{JsonSerializer.Serialize(inputData)}";

        var summary = await _ai.GetUserSummaryAsync(prompt);
        return Ok(new { summary });
    }
}

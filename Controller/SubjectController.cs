using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeTable.Core.DbContext;

namespace TimeTable.Controller;

[Route("api/[controller]")]
[ApiController]
public class SubjectController : ControllerBase
{
    private readonly TimeTableContext _dbContext;

    public SubjectController(TimeTableContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSubjects()
    {
        var subjects = await _dbContext.Subjects.ToListAsync();
        return Ok(subjects);
    }
}
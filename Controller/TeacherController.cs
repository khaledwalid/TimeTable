using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeTable.Core.DbContext;

namespace TimeTable.Controller;

[Route("api/[controller]")]
[ApiController]
public class TeacherController : ControllerBase
{
    private readonly TimeTableContext _dbContext;

    public TeacherController(TimeTableContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTeachers()
    {
        var teachers = await _dbContext.Teachers.ToListAsync();
        return Ok(teachers);
    }
}
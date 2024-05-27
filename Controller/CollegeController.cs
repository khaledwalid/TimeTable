using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeTable.Core.DbContext;

namespace TimeTable.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CollegeController : ControllerBase
    {
        private readonly TimeTableContext _dbContext;

        public CollegeController(TimeTableContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllColleges()
        {
            var colleges = await _dbContext.Colleges.ToListAsync();
            return Ok(colleges);
        }
    }
}

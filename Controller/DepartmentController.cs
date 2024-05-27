using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeTable.Core.DbContext;

namespace TimeTable.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly TimeTableContext _dbContext;

        public DepartmentController(TimeTableContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDepartments()
        {
            var departments = await _dbContext.Departments
                .ToListAsync();
            return Ok(departments);
        }
    }
}
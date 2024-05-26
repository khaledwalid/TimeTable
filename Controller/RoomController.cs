using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeTable.Core.DbContext;

namespace TimeTable.Controller;

[Route("api/[controller]")]
[ApiController]
public class RoomController : ControllerBase
{
    private readonly TimeTableContext _dbContext;

    public RoomController(TimeTableContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRooms()
    {
        var rooms = await _dbContext.Rooms.ToListAsync();
        return Ok(rooms);
    }
}
using Hubalinno.CRM.Shared.Dtos;
using Hubalinno.CRM.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hubalinno.CRM.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UserSummaryDto>>> GetAll()
    {
        var users = await db.Users
            .OrderBy(u => u.FullName)
            .Select(u => new UserSummaryDto { Id = u.Id, Email = u.Email ?? "", FullName = u.FullName })
            .ToListAsync();

        return users;
    }
}

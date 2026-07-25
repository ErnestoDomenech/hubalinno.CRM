using Hubalinno.CRM.Shared.Dtos;
using Hubalinno.CRM.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hubalinno.CRM.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/products")]
public class ProductsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll()
    {
        var products = await db.Products.OrderBy(p => p.BusinessLine).ThenBy(p => p.Name).ToListAsync();

        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Key = p.Key,
            Name = p.Name,
            BusinessLine = p.BusinessLine,
        }).ToList();
    }
}

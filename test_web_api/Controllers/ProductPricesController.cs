using Microsoft.AspNetCore.Mvc;
using test_web_api.Data;
using test_web_api.Models;

namespace test_web_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductPricesController:ControllerBase
    {
        private readonly MyDBContext _context;
        public ProductPricesController(MyDBContext context)
        {
            _context = context;
        }

        // POST: api/productprices
        [HttpPost]
        public IActionResult AddPrice([FromBody] ProductPrice price)
        {
            if (!_context.Products.Any(p => p.Id == price.ProductId))
            {
                return BadRequest("Invalid ProductId");
            }

            _context.ProductPrices.Add(price);
            _context.SaveChanges();

            return Ok(price);
        }

    }
}

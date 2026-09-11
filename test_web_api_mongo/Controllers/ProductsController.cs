using Microsoft.AspNetCore.Mvc;
using test_web_api_mongo.Dtos;
using test_web_api_mongo.Services;

namespace test_web_api_mongo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // GET: api/products
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            _logger.LogInformation("GET products at {Time}", DateTime.UtcNow);
            _logger.LogTrace("This is a trace message");
            _logger.LogDebug("This is a debug message");
            _logger.LogInformation("This is an information message");
            _logger.LogWarning("This is a warning message");
            _logger.LogError("This is an error message");
            _logger.LogCritical("This is a critical message");
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(string id)
        {
            _logger.LogInformation("GET product by id at {Time}", DateTime.UtcNow);
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        // POST: api/products
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDto productDto)
        {
            try
            {
                var product = await _productService.CreateAsync(productDto);
                return Ok(product);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(string id, [FromBody] ProductUpdateDto productDto)
        {
            var product = await _productService.UpdateAsync(id, productDto);
            return product ==null? NotFound(): Ok(product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var response =await _productService.DeleteAsync(id);
            return response ? NoContent() : NotFound();
        }
       
    }

}

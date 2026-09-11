using test_web_api_mongo.Models;

namespace test_web_api_mongo.Dtos
{
    public class ProductDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<ProductPriceDto> Prices { get; set; } = new();
    }
    public class ProductCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public List<ProductPriceDto> Prices { get; set; } = new();
    }

    public class ProductUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public List<ProductPriceDto> Prices { get; set; } = new();
    }

    public class ProductPriceDto
    {
        public string Currency { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }


}

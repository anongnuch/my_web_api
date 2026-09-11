using test_web_api.Models;

namespace test_web_api.Dtos
{
    //public class ProductDto
    //{
    //    public string? Name { get; set; }
    //    public DateTime? CreatedDate { get; set; }

    //    public List<ProductPriceDto> ProductPrices { get; set; }
    //}

    //public class ProductPriceDto
    //{
    //    public int? ProductId { get; set; }
    //    public decimal? Price { get; set; }
    //    public DateTime? EffectiveDate { get; set; }
    //}

    public class ProductCreateDto
    {
        public string Name { get; set; }
        public List<ProductPriceDto> ProductPrices { get; set; }
    }

    public class ProductPriceDto
    {
        public int? Id { get; set; }
        public decimal Price { get; set; }
        public DateTime EffectiveDate { get; set; }
    }

    public class ProductUpdateDto
    {
        public string Name { get; set; }
        public List<ProductPriceDto> ProductPrices { get; set; }
    }


}

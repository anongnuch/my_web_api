using System;
using System.Collections.Generic;

namespace test_web_api.Models
{
    public partial class Product
    {
        public Product()
        {
            ProductPrices = new HashSet<ProductPrice>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime? CreatedDate { get; set; }

        public virtual ICollection<ProductPrice> ProductPrices { get; set; }
    }
}

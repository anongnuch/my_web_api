using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace test_web_api.Models
{
    public partial class ProductPrice
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public decimal? Price { get; set; }
        public DateTime? EffectiveDate { get; set; }

        [JsonIgnore]
        public virtual Product? Product { get; set; }
    }
}

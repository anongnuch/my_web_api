using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace test_web_api_mongo.Models
{
    public partial class ProductPrice
    {
        public string Currency { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}

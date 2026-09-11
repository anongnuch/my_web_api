using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace test_web_api_mongo.Models
{
    public partial class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public List<ProductPrice> Prices { get; set; } = new();
    }
    public partial class ProductPrice
    {
        public string Currency { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}

namespace stock_api.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Symbol { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Side { get; set; } = ""; // BUY / SELL
        public DateTime CreatedAt { get; set; }
    }
}

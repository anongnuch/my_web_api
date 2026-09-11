using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using stock_api.Models;

namespace stock_api.Tests
{
    /// <summary>
    /// Covers the [RequireAuthorization] endpoints: POST /api/order and GET /api/order/{userId}.
    /// Tests in this class share one CustomWebApplicationFactory/InMemory database instance
    /// (xunit runs tests within a class sequentially by default), so distinct userIds are used
    /// per test to avoid cross-test interference.
    /// </summary>
    public class OrderTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public OrderTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private HttpClient CreateAuthenticatedClient()
        {
            var client = _factory.CreateClient();
            var token = TestTokenHelper.CreateValidToken();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        [Fact]
        public async Task PostOrder_WithValidTokenAndBody_ReturnsOkWithSavedOrder()
        {
            var client = CreateAuthenticatedClient();
            var order = new OrderModel
            {
                UserId = 1001,
                Symbol = "AAPL",
                Quantity = 10,
                Price = 150.25m,
                Side = "BUY"
            };

            var response = await client.PostAsJsonAsync("/api/order", order);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var saved = await response.Content.ReadFromJsonAsync<OrderModel>();

            Assert.NotNull(saved);
            Assert.Equal(order.UserId, saved!.UserId);
            Assert.Equal(order.Symbol, saved.Symbol);
            Assert.Equal(order.Quantity, saved.Quantity);
            Assert.Equal(order.Price, saved.Price);
            Assert.Equal(order.Side, saved.Side);
            // CreatedAt is overwritten server-side, so it should be populated (not default).
            Assert.NotEqual(default, saved.CreatedAt);
        }

        [Fact]
        public async Task PostOrder_ThenGetOrdersForUser_ReturnsTheCreatedOrder()
        {
            var client = CreateAuthenticatedClient();
            const int userId = 1002;
            var order = new OrderModel
            {
                UserId = userId,
                Symbol = "MSFT",
                Quantity = 5,
                Price = 300.00m,
                Side = "SELL"
            };

            var postResponse = await client.PostAsJsonAsync("/api/order", order);
            Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);

            var getResponse = await client.GetAsync($"/api/order/{userId}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var orders = await getResponse.Content.ReadFromJsonAsync<List<OrderModel>>();

            Assert.NotNull(orders);
            var single = Assert.Single(orders!);
            Assert.Equal(userId, single.UserId);
            Assert.Equal("MSFT", single.Symbol);
        }

        [Fact]
        public async Task GetOrders_ForUserWithNoOrders_ReturnsOkWithEmptyList()
        {
            var client = CreateAuthenticatedClient();
            const int userIdWithNoOrders = 9999;

            var response = await client.GetAsync($"/api/order/{userIdWithNoOrders}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var orders = await response.Content.ReadFromJsonAsync<List<OrderModel>>();

            Assert.NotNull(orders);
            Assert.Empty(orders!);
        }

        [Fact]
        public async Task PostOrder_WithoutAuthorizationHeader_ReturnsUnauthorized()
        {
            var order = new OrderModel { UserId = 1003, Symbol = "GOOG", Quantity = 1, Price = 1m, Side = "BUY" };

            var response = await _client.PostAsJsonAsync("/api/order", order);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PostOrder_WithInvalidToken_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", "this.is.not-a-valid-jwt");
            var order = new OrderModel { UserId = 1004, Symbol = "TSLA", Quantity = 1, Price = 1m, Side = "BUY" };

            var response = await client.PostAsJsonAsync("/api/order", order);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PostOrder_WithTokenSignedByWrongKey_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var badToken = TestTokenHelper.CreateTokenSignedWithWrongKey();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", badToken);
            var order = new OrderModel { UserId = 1005, Symbol = "NFLX", Quantity = 1, Price = 1m, Side = "BUY" };

            var response = await client.PostAsJsonAsync("/api/order", order);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PostOrder_WithExpiredToken_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var expiredToken = TestTokenHelper.CreateExpiredToken();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);
            var order = new OrderModel { UserId = 1006, Symbol = "AMZN", Quantity = 1, Price = 1m, Side = "BUY" };

            var response = await client.PostAsJsonAsync("/api/order", order);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetOrdersForUser_WithoutAuthorizationHeader_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("/api/order/1");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetOrdersForUser_WithInvalidToken_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", "not-a-real-token");

            var response = await client.GetAsync("/api/order/1");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetOrdersForUser_WithNonNumericUserId_ReturnsBadRequest()
        {
            // A valid token is used here so the failed int route-parameter bind (not the
            // [RequireAuthorization] check) is what's actually being exercised.
            var client = CreateAuthenticatedClient();

            var response = await client.GetAsync("/api/order/not-a-number");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostOrder_WithMalformedJsonBody_ReturnsBadRequest()
        {
            var client = CreateAuthenticatedClient();
            var content = new StringContent("{ this is not valid json", System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/order", content);

            // Invalid JSON fails model binding before the handler's try/catch runs, so the
            // framework itself produces a 400, not the handler's own catch-block BadRequest().
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}

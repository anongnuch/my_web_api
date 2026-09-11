using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using stock_api.Models;

namespace stock_api.Tests
{
    /// <summary>
    /// GET /api/order_unauthorized/{userId} deliberately has no RequireAuthorization() call
    /// in Program.cs. This is documented, intentional (if questionable) existing behavior -
    /// these tests confirm it currently works with no Authorization header at all, and are
    /// not meant to "fix" or harden the endpoint.
    /// </summary>
    public class OrderUnauthorizedEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public OrderUnauthorizedEndpointTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetOrders_WithNoAuthorizationHeader_ReturnsOkWithEmptyList()
        {
            const int userIdWithNoOrders = 8888;

            var response = await _client.GetAsync($"/api/order_unauthorized/{userIdWithNoOrders}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var orders = await response.Content.ReadFromJsonAsync<List<OrderModel>>();

            Assert.NotNull(orders);
            Assert.Empty(orders!);
        }

        [Fact]
        public async Task GetOrders_WithNoAuthorizationHeader_ReturnsOrdersCreatedForThatUser()
        {
            const int userId = 8889;

            var authedClient = _factory.CreateClient();
            authedClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TestTokenHelper.CreateValidToken());
            var order = new OrderModel
            {
                UserId = userId,
                Symbol = "IBM",
                Quantity = 3,
                Price = 42.5m,
                Side = "BUY"
            };
            var postResponse = await authedClient.PostAsJsonAsync("/api/order", order);
            Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);

            // No Authorization header attached at all on this call.
            var response = await _client.GetAsync($"/api/order_unauthorized/{userId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var orders = await response.Content.ReadFromJsonAsync<List<OrderModel>>();

            Assert.NotNull(orders);
            var single = Assert.Single(orders!);
            Assert.Equal(userId, single.UserId);
            Assert.Equal("IBM", single.Symbol);
        }

        [Fact]
        public async Task GetOrders_WithNonNumericUserId_ReturnsBadRequest()
        {
            var response = await _client.GetAsync("/api/order_unauthorized/not-a-number");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}

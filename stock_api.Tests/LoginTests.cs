using System.Net;
using System.Net.Http.Json;

namespace stock_api.Tests
{
    public class LoginTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public LoginTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private record LoginResponse(int userId, string token);

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkWithUserIdAndToken()
        {
            var response = await _client.PostAsJsonAsync("/login", new
            {
                Username = CustomWebApplicationFactory.SeededUsername,
                Password = CustomWebApplicationFactory.SeededPassword
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<LoginResponse>();

            Assert.NotNull(body);
            Assert.Equal(_factory.SeededUserId, body!.userId);
            Assert.False(string.IsNullOrWhiteSpace(body.token));
            // A JWT has three dot-separated segments (header.payload.signature).
            Assert.Equal(3, body.token.Split('.').Length);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsUnauthorized()
        {
            var response = await _client.PostAsJsonAsync("/login", new
            {
                Username = CustomWebApplicationFactory.SeededUsername,
                Password = "not-the-right-password"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithNonexistentUser_ReturnsUnauthorized()
        {
            var response = await _client.PostAsJsonAsync("/login", new
            {
                Username = "no-such-user",
                Password = "whatever"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithEmptyUsernameAndPassword_ReturnsUnauthorized()
        {
            var response = await _client.PostAsJsonAsync("/login", new
            {
                Username = "",
                Password = ""
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithMissingPasswordField_ReturnsUnauthorized()
        {
            // No "Password" property at all - LoginRequest's Password parameter binds to null
            // rather than the request failing to deserialize, since it has no default requiring it.
            var response = await _client.PostAsJsonAsync("/login", new
            {
                Username = CustomWebApplicationFactory.SeededUsername
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithMalformedJsonBody_ReturnsBadRequest()
        {
            var content = new StringContent("{ this is not valid json", System.Text.Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/login", content);

            // Invalid JSON fails model binding before the handler's try/catch runs, so the
            // framework itself produces a 400, not the handler's own catch-block BadRequest().
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}

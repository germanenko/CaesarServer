using System.Text.Json;
using Planer_mailbox_service.Core.Entities.Response;
using Planer_mailbox_service.Core.IService;

namespace Planer_mailbox_service.App.Service
{
    public class GoogleTokenService : IGoogleTokenService
    {
        private readonly string clientId;
        private readonly string clientSecret;

        public GoogleTokenService(string clientId, string clientSecret)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
        }

        public async Task<GoogleTokenResponse> RefreshAccessTokenAsync(string refreshToken)
        {
            var requestUrl = "https://oauth2.googleapis.com/token";
            var client = new HttpClient();
            var requestData = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "refresh_token", refreshToken },
            { "grant_type", "refresh_token" }
        };

            var requestContent = new FormUrlEncodedContent(requestData);
            var response = await client.PostAsync(requestUrl, requestContent);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(responseString);
                return tokenResponse;
            }

            throw new Exception("Failed to refresh token.");
        }
    }
}
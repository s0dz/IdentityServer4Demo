using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace ImageGallery.Client.Services
{
    public class ImageGalleryHttpClient : IImageGalleryHttpClient
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient = new HttpClient();

        public ImageGalleryHttpClient(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        public async Task<HttpClient> GetClient()
        {
            string accessToken;

            var currentContext = _httpContextAccessor.HttpContext;

            // var accessToken = await currentContext.Authentication.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            var expiresAt = await currentContext.Authentication.GetTokenAsync("expires_at");

            if (string.IsNullOrWhiteSpace(expiresAt) ||
                DateTime.Parse(expiresAt).AddSeconds(-60).ToUniversalTime() < DateTime.UtcNow)
            {
                accessToken = await RenewTokens();
            }
            else
            {
                accessToken = await currentContext.Authentication.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            }

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                _httpClient.SetBearerToken(accessToken);
            }

            _httpClient.BaseAddress = new Uri("https://localhost:44364/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return _httpClient;
        }

        private async Task<string> RenewTokens()
        {
            // get the current HttpContext to access the tokens
            var currentContext = _httpContextAccessor.HttpContext;

            // get the metadata
            var discoveryClient = new DiscoveryClient("https://localhost:44379/");
            var metaDataResponse = await discoveryClient.GetAsync();

            // create a new token client to get new tokens
             var tokenClient = new TokenClient(metaDataResponse.TokenEndpoint, "imagegalleryclient", "secret");

            // get the saved refresh token
            var currentRefreshToken = await currentContext.Authentication.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            // refresh the tokens
            var tokenResult = await tokenClient.RequestRefreshTokenAsync(currentRefreshToken);

            // save the tokens
            if (!tokenResult.IsError)
            {
                // get auth info
                var authenticateInfo = await currentContext.Authentication.GetAuthenticateInfoAsync("Cookies");

                // create a new value for expires_at and save it
                var expiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(tokenResult.ExpiresIn);
                authenticateInfo.Properties.UpdateTokenValue("expires_at", expiresAt.ToString("o", CultureInfo.InvariantCulture));

                authenticateInfo.Properties.UpdateTokenValue(OpenIdConnectParameterNames.AccessToken, tokenResult.AccessToken);

                authenticateInfo.Properties.UpdateTokenValue(OpenIdConnectParameterNames.RefreshToken, tokenResult.RefreshToken);

                // sign in with new values
                await currentContext.Authentication.SignInAsync("Cookies", authenticateInfo.Principal, authenticateInfo.Properties);

                // return the new access token
                return tokenResult.AccessToken;
            }
            else
            {
                throw new Exception("Problem encountered while refreshing tokens.", tokenResult.Exception);
            }
        }
    }
}


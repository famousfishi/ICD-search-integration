using IdentityModel.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ICD_integration
{
    public class IcdSearchService
    {
        private static readonly string tokenEndpoint = "https://icdaccessmanagement.who.int/connect/token";
        private static readonly string clientId = "CLIENT_ID";
        private static readonly string clientSecret = "CLIENT_SECRET";
        private static readonly string scope = "icdapi_access";
        private static readonly string apiUrl = "https://id.who.int/icd/entity/search";

        private HttpClient _client;

        public IcdSearchService()
        {
            _client = new HttpClient();
        }

        private async Task<string> GetAccessTokenAsync()
        {
            ClientCredentialsTokenRequest tokenRequest = new ClientCredentialsTokenRequest
            {
                Address = tokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = scope,
                GrantType = "client_credentials",
                ClientCredentialStyle = ClientCredentialStyle.AuthorizationHeader
            };

            //request token from endpoint
            TokenResponse tokenResponse = await _client.RequestClientCredentialsTokenAsync(tokenRequest);

            if (tokenResponse.IsError)
            {
                throw new Exception("Error retrieving access token: " + tokenResponse.Error);
            }

            return tokenResponse.AccessToken!;
        }

        public async Task<string> SearchDiseasesAsync(string query)
        {
            // Get token and set it in the Authorization header
            var accessToken = await GetAccessTokenAsync();
            _client.SetBearerToken(accessToken);

            // Prepare request to the API
            var request = new HttpRequestMessage(HttpMethod.Get, $"{apiUrl}?q={query}&flatResults=false&language=en");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("en"));
            request.Headers.Add("API-Version", "v2");

            // Send the request
            HttpResponseMessage response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error: {response.StatusCode}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public void DisplaySearchResults(string jsonResponse)
        {
            var results = JsonConvert.DeserializeObject<IcdSearchResponse>(jsonResponse);
            if (results?.DestinationEntities != null)
            {
                Console.WriteLine("\nSearch Results:");
                foreach (var entity in results.DestinationEntities)
                {
                    Console.WriteLine($"Code: {entity.TheCode}");
                    Console.WriteLine($"Title: {entity.Title}");
                }
            }
            else
            {
                Console.WriteLine("No results found.");
            }
        }
    }

    public class IcdSearchResponse
    {
        public DestinationEntity[] DestinationEntities { get; set; }
    }

    public class DestinationEntity
    {
        public string TheCode { get; set; }
        public string Title { get; set; }
    }
}
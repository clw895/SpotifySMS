using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;

namespace SpotifySMS
{
    public class SpotifyClient
    {
        protected static HttpClient _httpClient {get;set;}
        internal string ClientId {get; private set;}
        internal string ClientSecret {get;private set;}
        
        public SpotifyClient(string clientId, string clientSecret)
        {
            _httpClient = new HttpClient();
            ClientId = clientId;
            ClientSecret = clientSecret;
        }

        public async Task<JObject> GetSong(string query)
        {
            var bearerToken = await GetBearerToken(ClientId,ClientSecret);
            
            if (string.IsNullOrEmpty(bearerToken))
            {
                return null;
            }

            var builder = new UriBuilder("https://api.spotify.com/v1/search");
            var parameters = HttpUtility.ParseQueryString("");
            parameters["q"] = query;
            parameters["type"]="track";
            builder.Query = parameters.ToString();

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");
            try
            {
                var response = await _httpClient.GetStringAsync(builder.ToString());
                var json = JObject.Parse(response);
            
                var firstTrack = json["tracks"]["items"][0];

                var song = new 
                {
                    title = firstTrack["name"].ToString(),
                    artist = firstTrack["artists"][0]["name"].ToString(),
                    previewUrl = firstTrack["preview_url"].ToString() ?? string.Empty,
                    image = firstTrack["album"]["images"][0]["url"].ToString()
                };

                return JObject.FromObject(song);
            }
            catch
            {
                return null;
            }
            
        }

        private async Task<string> GetBearerToken(string clientId, string clientSecret)
        {
            var bytes = System.Text.ASCIIEncoding.ASCII.GetBytes($"{clientId}:{clientSecret}");
            var encoded = Convert.ToBase64String(bytes);

            var body = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("grant_type","client_credentials")
            });
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {encoded}");

            var response = await _httpClient.PostAsync("https://accounts.spotify.com/api/token",body);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            return json["access_token"].ToString();

        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FtdnaBuddy.Ftdna.CsvParsing;
using FtdnaBuddy.Ftdna.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FtdnaBuddy.Ftdna
{
    public class FtdnaConnector : IFtdnaConnector
    {
        private const string BaseUri = "https://www.familytreedna.com/";
        private const string PageAcceptHeader = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
        private const string SignInPath = "sign-in";
        public const int DefaultPageSize = 1500;
        public const int DefaultRequestDelay = 2000;

        private readonly CookieContainer _cookies = new CookieContainer();
        private readonly HttpClient _client;
        
        public int RequestDelay { get; }
            
        public FtdnaConnector(int requestDelay = DefaultRequestDelay)
        {
            RequestDelay = requestDelay;
            var messageHandler = CreateMessageHandler();
            _client = CreateClient(messageHandler);
        }
        
        public FtdnaConnector(HttpMessageHandler messageHandler, int requestDelay = DefaultRequestDelay)
        {
            RequestDelay = requestDelay;
            _client = CreateClient(messageHandler);
        }

        private HttpMessageHandler CreateMessageHandler()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = true,
                CookieContainer = _cookies
            };
            var throttler = new ThrottlingMessageHandler(RequestDelay, handler)
            {
                Filter = request => 
                {
                    var unthrottledPaths = new[] { "sign-in", "default.aspx" };
                    bool isUnthrottled = unthrottledPaths.Any(request.RequestUri.PathAndQuery.Contains);
                    return !isUnthrottled;
                }    
            };
            return throttler;
        }

        private HttpClient CreateClient(HttpMessageHandler messageHandler, string baseUri = BaseUri)
        {
            var client = new HttpClient(messageHandler) { BaseAddress = new Uri(baseUri) };
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-US"));
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en", .8));
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
			client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36");
			return client;
        }

        public async Task<string> GetVerificationTokenAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, SignInPath);
            request.Headers.Accept.ParseAdd(PageAcceptHeader);

            var result = await _client.SendAsync(request);

            string token = _cookies.GetCookies(new Uri(BaseUri + SignInPath))
                                   .Cast<Cookie>()
                                   .SingleOrDefault(c => c.Name == "__RequestVerificationToken")
                                   ?.Value;
            return token;
        }

        public async Task<LoginResult> LoginAsync(string kitNumber, string password)
        {
            var body = $"{{\"model\": {{\"password\": \"{password}\", \"kitNum\": \"{kitNumber}\", \"rememberMe\": false}}, \"returnUrl\": null}}";

            var request = new HttpRequestMessage(HttpMethod.Post, SignInPath);
            request.Headers.Referrer = new Uri(BaseUri + SignInPath);
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var result = await _client.SendAsync(request);
			var resultString = await result.Content.ReadAsStringAsync();
            if (!ValidateJson(resultString))
                return new LoginResult
                {
                    ErrorMessage = "There was an unexpected error logging in"
                };

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.Converters.Add(new LoginResultConverter());
			return JsonConvert.DeserializeObject<LoginResult>(resultString, jsonSettings);
        }

        private bool ValidateJson(string json)
        {
            try
            {
				JToken.Parse(json);
                return true;
			}
            catch
            {
                return false;
            }
        }

        public async Task<string> GetEkitIdAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "my/default.aspx");
            request.Headers.Accept.ParseAdd(PageAcceptHeader);
            request.Headers.Referrer = new Uri(BaseUri + SignInPath);

            var result = await _client.SendAsync(request);

            using (var resultStream = await result.Content.ReadAsStreamAsync())
            {
                var reader = new StreamReader(resultStream);
                var body = await reader.ReadToEndAsync();

                var match = Regex.Match(body, "<notification\\sekit=\\\"(?<ekit>.*?)\\\"");
                if (!match.Success)
                    return null;

                return match.Groups["ekit"]?.Captures.FirstOrDefault()?.Value;
            }
        }

        public async Task<MatchResults> ListAllMatches(int page = 1, int pageSize = DefaultPageSize)
        {
            var uri = $"my/family-finder-api/matches?filter3rdParty=false&filterId=0&page={page}&pageSize={pageSize}&selectedBucket=0&sortDirection=desc&sortField=relationshipPercentage()&trial=0";
            return await ListMatches(uri);
        }

		public async Task<MatchResults> ListNewMatches(DateTime startDate, int page = 1, int pageSize = DefaultPageSize)
		{
            string formattedDate = startDate.ToString("yyyy-MM-dd T00:00:00.000Z");
            var uri = $"my/family-finder-api/matches?filter3rdParty=false&filterId=10&filterSince={formattedDate}&page={page}&pageSize={pageSize}&selectedBucket=0&sortDirection=desc&sortField=rbDate&trial=0";
			return await ListMatches(uri);
		}

        private async Task<MatchResults> ListMatches(string uri)
        {
			var request = new HttpRequestMessage(HttpMethod.Get, uri);
			request.Headers.Accept.ParseAdd(PageAcceptHeader);
			request.Headers.Referrer = new Uri(BaseUri + "my/default.aspx");

			var result = await _client.SendAsync(request);
			return await GetJsonResponse<MatchResults>(result);
        }

        public async Task<IEnumerable<ChromosomeSegment>> ListChromosomeSegmentsAsync(string ekitId)
        {
            var uri = $"my/family-finder/chromosome-browser-download?ekit={ekitId}";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.ParseAdd(PageAcceptHeader);
            request.Headers.Referrer = new Uri(BaseUri + "my/family-finder/chromosome-browser?c=True");

            var result = await _client.SendAsync(request);

            var parser = new CsvChromosomeSegmentParser();
            using (var resultStream = await result.Content.ReadAsStreamAsync())
            using (var streamReader = new StreamReader(resultStream))
            {
                return parser.Parse(streamReader);
            }
        }

        public async Task<MatchResults> ListInCommonWithAsync(string resultId2, int page = 1, int pageSize = DefaultPageSize)
        {
            var uri = $"my/family-finder-api/matches?filter3rdParty=false&filterId=5&filterResultId={resultId2}&page={page}&pageSize={pageSize}&selectedBucket=0&sortDirection=desc&sortField=relationshipPercentage()&trial=0";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            request.Headers.Referrer = new Uri(BaseUri + "my/familyfinder/");

            var result = await _client.SendAsync(request);
            return await GetJsonResponse<MatchResults>(result);
        }

        public async Task<MatchDetailsResult> GetMatchDetailsAsync(string resultId1, string resultId2)
        {
            var content = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("resultId1", resultId1),
                new KeyValuePair<string, string>("resultId2", resultId2),
                new KeyValuePair<string, string>("trial", "false")
            });

            var request = new HttpRequestMessage(HttpMethod.Post, "my/family-finder/get-user-match-data")
            {
                Content = content
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/javascript"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", .01));
            request.Headers.Referrer = new Uri(BaseUri + "my/familyfinder");
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");

            var result = await _client.SendAsync(request);
            return await GetJsonResponse<MatchDetailsResult>(result);
        }

        async Task<T> GetJsonResponse<T>(HttpResponseMessage response, params JsonConverter[] converters)
        {
            JsonSerializer serializer = new JsonSerializer();
            foreach (var converter in converters)
            {
                serializer.Converters.Add(converter);
            }

            using (var resultStream = await response.Content.ReadAsStreamAsync())
            using (var streamReader = new StreamReader(resultStream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                return serializer.Deserialize<T>(jsonReader);
            }
        }
    }
}
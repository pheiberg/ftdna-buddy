﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;
using Newtonsoft.Json;

namespace AsdaLoader.Ftdna
{
    public class FtdnaConnector
    {
        const string BaseUri = "https://www.familytreedna.com/";
        const string PageAcceptHeader = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
        private const string SignInPath = "sign-in";
        private readonly CookieContainer _cookies = new CookieContainer();

        public async Task<string> GetVerificationTokenAsync()
        {
            using (var client = CreateClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, SignInPath);
                request.Headers.Add("Accept", PageAcceptHeader);
                
                var result = await client.SendAsync(request);

                string token = _cookies.GetCookies(new Uri(BaseUri + SignInPath))
                                       .Cast<Cookie>()
                                       .SingleOrDefault(c => c.Name == "__RequestVerificationToken")
                                       ?.Value;
                return token;
            }
        }

        public async Task<string> LoginAsync(string kitNumber, string password)
        {
            var body = $"{{\"model\": {{\"password\": \"{password}\", \"kitNum\": \"{kitNumber}\", \"rememberMe\": false}}, \"returnUrl\": null}}";

            using (var client = CreateClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, SignInPath);
                request.Headers.Referrer = new Uri(BaseUri + SignInPath);
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                
                var result = await client.SendAsync(request);
                
                using (var resultStream = await result.Content.ReadAsStreamAsync())
                {
                    var reader = new StreamReader(resultStream);
                    return await reader.ReadToEndAsync();
                }
            }
        }

        public async Task<string> GetEkitIdAsync()
        {
            using (var client = CreateClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "my/default.aspx");
                request.Headers.Add("Accept", PageAcceptHeader);
                request.Headers.Referrer = new Uri(BaseUri + SignInPath);
                
                var result = await client.SendAsync(request);
                
                using (var resultStream = await result.Content.ReadAsStreamAsync())
                {
                    var reader = new StreamReader(resultStream);
                    var body = await reader.ReadToEndAsync();

                    var match = Regex.Match(body, "<notification\\sekit=\\\"(?<ekit>.*?)\\\"");
                    if (!match.Success)
                        return null;
                    
                    return match.Groups["ekit"]?.Captures.FirstOrDefault()?.Value; ;
                }
            }
        }

        public async Task<MatchResults> ListMatches(int pageSize, int page)
        {
            JsonSerializer serializer = new JsonSerializer();
            using (var client = CreateClient())
            {
                var uri = $"my/family-finder-api/matches?filter3rdParty=false&filterId=0&page={page}&pageSize={pageSize}&selectedBucket=0&sortDirection=desc&sortField=relationshipPercentage()&trial=0";
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Add("Accept", PageAcceptHeader);
                request.Headers.Referrer = new Uri(BaseUri + "my/default.aspx");
                
                var result = await client.SendAsync(request);
                return await GetJsonResponse<MatchResults>(result);
            }
        }

        public async Task<IEnumerable<ChromosomeSegment>> ListChromosomeSegmentsAsync(string ekitId)
        {
            using (var client = CreateClient())
            {
                var uri = $"my/family-finder/chromosome-browser-download?ekit={ekitId}";
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Add("Accept", PageAcceptHeader);
                request.Headers.Referrer = new Uri(BaseUri + "my/family-finder/chromosome-browser?c=True");

                var result = await client.SendAsync(request);

                using (var resultStream = await result.Content.ReadAsStreamAsync())
                using (var streamReader = new StreamReader(resultStream))
                {
                    var csvReader = new CsvReader(streamReader);
                    csvReader.Configuration.RegisterClassMap<ChromosomeSegmentMap>();
					return csvReader.GetRecords<ChromosomeSegment>().ToArray();
                }
            }
        }

        public async Task<MatchResults> ListInCommonWithAsync(string resultId2, int page = 1, int pageSize = 1500)
        {
			JsonSerializer serializer = new JsonSerializer();
			using (var client = CreateClient())
			{
                var uri = $"my/family-finder-api/matches?filter3rdParty=false&filterId=5&filterResultId={resultId2}&page={page}&pageSize={pageSize}&selectedBucket=0&sortDirection=desc&sortField=relationshipPercentage()&trial=0";
				var request = new HttpRequestMessage(HttpMethod.Get, uri);
				request.Headers.Add("Accept", PageAcceptHeader);
				request.Headers.Referrer = new Uri(BaseUri + "my/familyfinder/");

				var result = await client.SendAsync(request);
                return await GetJsonResponse<MatchResults>(result);
			}  
        }

        public async Task<MatchDetailsResult> GetMatchDetailsAsync(string resultId1, string resultId2)
        {
            JsonSerializer serializer = new JsonSerializer();
            using (var client = CreateClient())
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
                request.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");
                request.Headers.Referrer = new Uri(BaseUri + "my/familyfinder");
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");

                var result = await client.SendAsync(request);
                return await GetJsonResponse<MatchDetailsResult>(result);
			}
        }

        private async Task<T> GetJsonResponse<T>(HttpResponseMessage response)
        {
            JsonSerializer serializer = new JsonSerializer();
			using (var resultStream = await response.Content.ReadAsStreamAsync())
			using (var streamReader = new StreamReader(resultStream))
			using (var jsonReader = new JsonTextReader(streamReader))
			{
				return serializer.Deserialize<T>(jsonReader);
			}
        }

        private HttpClient CreateClient(string baseUri = BaseUri)
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = true,
                CookieContainer = _cookies
            };
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUri)
            };
            client.DefaultRequestHeaders.Add("Host", new Uri(BaseUri).Host);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            return client;
        }
    }
}
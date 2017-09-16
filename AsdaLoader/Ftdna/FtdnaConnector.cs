using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AsdaLoader.Ftdna
{
    public class FtdnaConnector
    {
        const string BaseUri = "https://www.familytreedna.com/";
        const string PageAcceptHeader = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";

        private readonly CookieContainer _cookies = new CookieContainer();

        public async Task<string> GetVerificationTokenAsync()
        {
			using (var client = CreateClient())
			{
                var request = new HttpRequestMessage(HttpMethod.Get, "sign-in");
                request.Headers.Add("Accept", PageAcceptHeader);
				var result = await client.SendAsync(request);
                string token = _cookies.GetCookies(new Uri(BaseUri + "sign-in"))
                                       .Cast<Cookie>()
                                       .SingleOrDefault(c => c.Name == "__RequestVerificationToken")
                                       ?.Value;
				return token;
            }
        }

		public async Task<string> LoginAsync(string verificationToken, string kitNumber, string password)
		{
			var body = $"{{\"model\": {{\"password\": \"{password}\", \"kitNum\": \"{kitNumber}\", \"rememberMe\": false}}, \"returnUrl\": null}}";

			using (var client = CreateClient())
			{
				var request = new HttpRequestMessage(HttpMethod.Post, "sign-in");
				request.Headers.Referrer = new Uri(BaseUri + "sign-in");
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

        public async Task<bool> VerifyLoginAsync()
        {
			using (var client = CreateClient())
			{
				var request = new HttpRequestMessage(HttpMethod.Get, "my/default.aspx");
				request.Headers.Add("Accept", PageAcceptHeader);
                request.Headers.Referrer = new Uri(BaseUri + "sign-in");
				var result = await client.SendAsync(request);
				using (var resultStream = await result.Content.ReadAsStreamAsync())
				{
					var reader = new StreamReader(resultStream);
					var body = await reader.ReadToEndAsync();
                    return body.IndexOf("Invalid credentials", StringComparison.OrdinalIgnoreCase) == -1 && 
                               body.IndexOf("FTDNA signin", StringComparison.OrdinalIgnoreCase) == -1;
				};
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
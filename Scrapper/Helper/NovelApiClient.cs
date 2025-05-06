using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scrapper.Data.Configs;

namespace Scrapper.Helper
{
    public class NovelApiClient
    {
        private readonly NovelSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<NovelApiClient> _logger;
        private readonly Random _random;
        private readonly string[] _userAgents = new string[]
        {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.159 Safari/537.36",
            "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:54.0) Gecko/20100101 Firefox/54.0",
            "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.82 Safari/537.36",
            "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:83.0) Gecko/20100101 Firefox/83.0"
        };

        public NovelApiClient(IOptions<NovelSettings> options,
                              HttpClient httpClient,
                              ILogger<NovelApiClient> logger)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            _logger = logger;
            _random = new Random();
        }
        private void SetRandomUserAgentHeader()
        {
            var randomUserAgent = _userAgents[_random.Next(_userAgents.Length)]; // Select random User-Agent
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", randomUserAgent);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Remove("User-Agent"); // Remove previous User-Agent
                _httpClient.DefaultRequestHeaders.Add("User-Agent", randomUserAgent); // Add new one
            }
        }

        #region
        public async Task<string> Get(string url)
        {
            SetRandomUserAgentHeader(); // Set a random User-Agent header before making the request
            var response = await _httpClient.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetHome()
        {
            SetRandomUserAgentHeader(); // Set a random User-Agent header
            var response = await _httpClient.GetAsync("");
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetLatest()
        {
            SetRandomUserAgentHeader(); // Set a random User-Agent header
            var response = await _httpClient.GetAsync(_settings.Latest);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetHot()
        {
            SetRandomUserAgentHeader(); // Set a random User-Agent header
            var response = await _httpClient.GetAsync(_settings.Hot);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetCompleted()
        {
            SetRandomUserAgentHeader(); // Set a random User-Agent header
            var response = await _httpClient.GetAsync(_settings.Completed);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetPopular()
        {
            SetRandomUserAgentHeader(); // Set a random User-Agent header
            var response = await _httpClient.GetAsync(_settings.Popular);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetChapter(string novelName)
        {
            SetRandomUserAgentHeader(); // Set a random User-Agent header
            var response = await _httpClient.GetAsync($"b/{novelName}");
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> GetResponseContentAsync(string endpoint)
        {
            SetRandomUserAgentHeader(); // Set a random User-Agent header
            var response = await _httpClient.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            _logger.LogError($"Request to {endpoint} failed with status code {response.StatusCode}");
            return null;
        }
        #endregion
        //public async Task<string> Get(string url) => await (await _httpClient.GetAsync(url)).Content.ReadAsStringAsync();

        //public async Task<string> GetHome() => await (await _httpClient.GetAsync("")).Content.ReadAsStringAsync();
        //public async Task<string> GetLatest() => await (await _httpClient.GetAsync(_settings.Latest)).Content.ReadAsStringAsync();
        //public async Task<string> GetHot() => await (await _httpClient.GetAsync(_settings.Hot)).Content.ReadAsStringAsync();
        //public async Task<string> GetCompleted() => await (await _httpClient.GetAsync(_settings.Completed)).Content.ReadAsStringAsync();
        //public async Task<string> GetPopular() => await (await _httpClient.GetAsync(_settings.Popular)).Content.ReadAsStringAsync();
        //public async Task<string> GetChapter(string novelName) => await (await _httpClient.GetAsync($"b/{novelName}")).Content.ReadAsStringAsync();


        //private async Task<string> GetResponseContentAsync(string endpoint)
        //{
        //    var response = await _httpClient.GetAsync(endpoint);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        return await response.Content.ReadAsStringAsync();
        //    }
        //    _logger.LogError($"Request to {endpoint} failed with status code {response.StatusCode}");
        //    return null;
        //}
    }
}

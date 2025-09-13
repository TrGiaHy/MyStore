using Newtonsoft.Json;

namespace BusinessLogic.Services.ApiClientService
{
    public class ApiClientService : IApiClientService
    {
        private readonly HttpClient _httpClient;

        public ApiClientService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7139");
        }

        public async Task<T?> GetAsync<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API error ({response.StatusCode}): {content}");

            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}

using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;

namespace BusinessLogic.Services.ApiClientService
{
    public class ApiClientService : IApiClientService
    {
        private readonly HttpClient _httpClient;

        public ApiClientService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5021");
        }

        public async Task<T?> GetAsync<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API error ({response.StatusCode}): {content}");

            return JsonConvert.DeserializeObject<T>(content);
        }

        public async Task<T?> PostAsync<T>(string url, object data)
        {
            var response = await _httpClient.PostAsJsonAsync(url, data);
            return await HandleResponse<T>(response);
        }

        public async Task<T?> PutAsync<T>(string url, object data)
        {
            var response = await _httpClient.PutAsJsonAsync(url, data);
            return await HandleResponse<T>(response);
        }

        public async Task<bool> DeleteAsync(string url)
        {
            var response = await _httpClient.DeleteAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new Exception($"API error ({response.StatusCode}): {content}");
            }
            return true;
        }
        public async Task<T?> PatchAsync<T>(string url, object data)
        {
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };

            var response = await _httpClient.SendAsync(request);
            return await HandleResponse<T>(response);
        }
        // Hàm xử lý chung response để tránh lặp code
        private async Task<T?> HandleResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API error ({response.StatusCode}): {content}");

            return JsonConvert.DeserializeObject<T>(content);
        }
        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public T Data { get; set; }
            public string ErrorMessage { get; set; }
            public int StatusCode { get; set; }
            public string? Username { get; set; }
        }
        public async Task<ApiResponse<T>> PostJsonAsync<T>(string url, object data)
        {
            var response = await _httpClient.PostAsJsonAsync(url, data);
            var content = await response.Content.ReadAsStringAsync();

            try
            {
                // Parse thẳng về ApiResponse<T>
                var apiResult = JsonConvert.DeserializeObject<ApiResponse<T>>(content);

                if (apiResult != null)
                {
                    apiResult.StatusCode = (int)response.StatusCode;
                    return apiResult;
                }

                return new ApiResponse<T>
                {
                    Success = false,
                    ErrorMessage = "Empty response from API.",
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    ErrorMessage = $"Deserialize error: {ex.Message}",
                    StatusCode = (int)response.StatusCode
                };
            }
        }

        public async Task<ApiResponse<T>> GetJsonAsync<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            try
            {
                var apiResult = JsonConvert.DeserializeObject<ApiResponse<T>>(content);

                if (apiResult != null)
                {
                    apiResult.StatusCode = (int)response.StatusCode;
                    return apiResult;
                }

                return new ApiResponse<T>
                {
                    Success = response.IsSuccessStatusCode,
                    Data = default,
                    ErrorMessage = response.IsSuccessStatusCode ? null : content,
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    ErrorMessage = $"Deserialize error: {ex.Message}",
                    StatusCode = (int)response.StatusCode
                };
            }
        }
        public async Task<ApiResponse<T>> DeleteJsonAsync<T>(string url)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<T>
                    {
                        Success = true,
                        Data = default,
                        StatusCode = (int)response.StatusCode
                    };
                }

                return new ApiResponse<T>
                {
                    Success = false,
                    Data = default,
                    ErrorMessage = content,
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Data = default,
                    ErrorMessage = $"Request error: {ex.Message}",
                    StatusCode = 0
                };
            }
        }

        public async Task<ApiResponse<T>> PutJsonAsync<T>(string url, object data)
        {
            var response = await _httpClient.PutAsJsonAsync(url, data);
            var content = await response.Content.ReadAsStringAsync();

            try
            {
                // Parse thẳng về ApiResponse<T>
                var apiResult = JsonConvert.DeserializeObject<ApiResponse<T>>(content);

                if (apiResult != null)
                {
                    apiResult.StatusCode = (int)response.StatusCode;
                    return apiResult;
                }

                return new ApiResponse<T>
                {
                    Success = false,
                    ErrorMessage = "Empty response from API.",
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    ErrorMessage = $"Deserialize error: {ex.Message}",
                    StatusCode = (int)response.StatusCode
                };
            }
        }

    }
}

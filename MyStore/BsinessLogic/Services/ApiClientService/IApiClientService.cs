using static BusinessLogic.Services.ApiClientService.ApiClientService;

namespace BusinessLogic.Services.ApiClientService
{
    public interface IApiClientService
    {
        Task<T?> GetAsync<T>(string url);
        Task<T?> PostAsync<T>(string url, object data);
        Task<T?> PutAsync<T>(string url, object data);
        Task<bool> DeleteAsync(string url);
        Task<T?> PatchAsync<T>(string url, object data);
        Task<ApiResponse<T>> PostJsonAsync<T>(string url, object data);
        Task<ApiResponse<T>> GetJsonAsync<T>(string url);
        Task<ApiResponse<T>> DeleteJsonAsync<T>(string url);

        Task<ApiResponse<T>> PutJsonAsync<T>(string url, object data);
    }
}

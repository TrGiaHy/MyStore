namespace BusinessLogic.Services.ApiClientService
{
    public interface IApiClientService
    {
        Task<T?> GetAsync<T>(string url);
    }
}

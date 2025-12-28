namespace Application.Interface.Services
{
    public interface ICacheVersionProvider
    {
        Task<string> GetVersionAsync(string scope);
        Task IncrementAsync(string scope);
    }
}

namespace CodegateTest.Services.IServices
{
    public interface IJWTHandler
    {
        Task<string?> GenerateTokenAsync(string userId, string email);
    }
}

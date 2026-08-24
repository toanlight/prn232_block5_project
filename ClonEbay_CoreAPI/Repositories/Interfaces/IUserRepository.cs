using ClonEbay_CoreAPI.Models;

namespace ClonEbay_CoreAPI.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByUsernameAsync(string username);
        Task<User?> GetUserWithAddressesAsync(int userId);
        Task<List<OrderTable>> GetUserOrdersAsync(int userId);
        Task<List<int>> GetUserReviewedProductIdsAsync(int userId);
        Task SaveChangesAsync();
    }
}

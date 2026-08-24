using ClonEbay_CoreAPI.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace ClonEbay_CoreAPI.Repositories.Interfaces
{
    public interface IAddressRepository
    {
        Task<List<Address>> GetByUserIdAsync(int userId);
        Task<Address?> GetByIdForUserAsync(int addressId, int userId);
        Task<bool> AnyForUserAsync(int userId);
        Task<Address?> GetFirstForUserAsync(int userId, int? excludeAddressId = null);
        Task ClearDefaultAsync(int userId, int? exceptAddressId = null);
        Task AddAsync(Address address);
        void Update(Address address);
        void Delete(Address address);
        Task SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}

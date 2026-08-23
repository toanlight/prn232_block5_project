using ClonEbay_CoreAPI.Models;
using ClonEbay_CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ClonEbay_CoreAPI.Repositories.Implementations
{
    public class AddressRepository : IAddressRepository
    {
        private readonly CloneEbayDbContext _context;
        private readonly DbSet<Address> _addresses;

        public AddressRepository(CloneEbayDbContext context)
        {
            _context = context;
            _addresses = context.Addresses;
        }

        public Task<List<Address>> GetByUserIdAsync(int userId)
        {
            return _addresses
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenBy(a => a.Id)
                .ToListAsync();
        }

        public Task<Address?> GetByIdForUserAsync(int addressId, int userId)
        {
            return _addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
        }

        public Task<bool> AnyForUserAsync(int userId)
        {
            return _addresses.AnyAsync(a => a.UserId == userId);
        }

        public Task<Address?> GetFirstForUserAsync(int userId, int? excludeAddressId = null)
        {
            return _addresses
                .Where(a => a.UserId == userId && (!excludeAddressId.HasValue || a.Id != excludeAddressId.Value))
                .OrderBy(a => a.Id)
                .FirstOrDefaultAsync();
        }

        public async Task ClearDefaultAsync(int userId, int? exceptAddressId = null)
        {
            var defaults = await _addresses
                .Where(a => a.UserId == userId && a.IsDefault && (!exceptAddressId.HasValue || a.Id != exceptAddressId.Value))
                .ToListAsync();

            foreach (var address in defaults)
            {
                address.IsDefault = false;
            }
        }

        public Task AddAsync(Address address)
        {
            return _addresses.AddAsync(address).AsTask();
        }

        public void Update(Address address)
        {
            _addresses.Update(address);
        }

        public void Delete(Address address)
        {
            _addresses.Remove(address);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        public Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return _context.Database.BeginTransactionAsync();
        }
    }
}

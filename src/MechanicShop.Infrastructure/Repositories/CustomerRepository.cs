using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MechanicShop.Domain.Entities;
using MechanicShop.Domain.Interfaces;
using MechanicShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Infrastructure.Repositories
{
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(MechanicShopDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<Customer> Items, int TotalCount)> GetPagedCustomersAsync(int pageNumber, int pageSize, string? search)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(c => c.User)
                .Where(c => !c.IsDeleted && !c.User.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(c =>
                    c.User.FirstName.ToLower().Contains(searchLower) ||
                    c.User.LastName.ToLower().Contains(searchLower) ||
                    c.User.Email.ToLower().Contains(searchLower) ||
                    c.PhoneNumber.ToLower().Contains(searchLower));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}

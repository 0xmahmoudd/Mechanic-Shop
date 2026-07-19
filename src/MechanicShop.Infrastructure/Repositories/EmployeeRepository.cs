using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MechanicShop.Domain.Entities;
using MechanicShop.Domain.Interfaces;
using MechanicShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Infrastructure.Repositories
{
    public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(MechanicShopDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<Employee> Items, int TotalCount)> GetPagedEmployeesAsync(int pageNumber, int pageSize, string? search)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(e => e.User)
                .Include(e => e.EmployeeSalaryHistories)
                .Where(e => !e.IsDeleted && !e.User.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(e =>
                    e.User.FirstName.ToLower().Contains(searchLower) ||
                    e.User.LastName.ToLower().Contains(searchLower) ||
                    e.User.Email.ToLower().Contains(searchLower) ||
                    (e.Title != null && e.Title.ToLower().Contains(searchLower))
                );
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(e => e.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Employee?> GetWithDetailsAsync(int id)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(e => e.User)
                .Include(e => e.EmployeeSalaryHistories)
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        }
    }
}

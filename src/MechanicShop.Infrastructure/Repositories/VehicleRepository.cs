using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MechanicShop.Domain.Entities;
using MechanicShop.Domain.Interfaces;
using MechanicShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Infrastructure.Repositories
{
    public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
    {
        public VehicleRepository(MechanicShopDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<Vehicle> Items, int TotalCount)> GetPagedVehiclesAsync(int pageNumber, int pageSize, string? search)
        {
            var query = _dbSet.AsNoTracking().Where(v => !v.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(v =>
                    v.LicensePlate.ToLower().Contains(searchLower) ||
                    (v.Vin != null && v.Vin.ToLower().Contains(searchLower)) ||
                    v.Make.ToLower().Contains(searchLower) ||
                    v.Model.ToLower().Contains(searchLower));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(v => v.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using MechanicShop.Domain.Entities;
using static MechanicShop.Domain.Interfaces.IGenericRepository;

namespace MechanicShop.Domain.Interfaces
{
    public interface IVehicleRepository : IGenericRepository<Vehicle>
    {
        Task<(IEnumerable<Vehicle> Items, int TotalCount)> GetPagedVehiclesAsync(int pageNumber, int pageSize, string? search);
    }
}

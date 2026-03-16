using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MechanicShop.Domain.Entities;
using static MechanicShop.Domain.Interfaces.IGenericRepository;

namespace MechanicShop.Domain.Interfaces
{
    public interface IPartRepository : IGenericRepository<Part>
    {
        Task<(IEnumerable<Part> parts, int totalCount)> GetAllPartsAsync(string? category = null, string? supplier = null, string? search = null);
        Task CreatePriceHistoryAsync(int createdPartId, decimal currentCost);
        Task DeletePriceHistoryAsync(int partId);
    }
}

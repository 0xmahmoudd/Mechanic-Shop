using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MechanicShop.Domain.Entities;
using static MechanicShop.Domain.Interfaces.IGenericRepository;

namespace MechanicShop.Domain.Interfaces
{
    public interface IInvoiceRepository : IGenericRepository<Invoice>
    {
        Task<Invoice?> GetByWorkOrderIdAsync(int workOrderId);
        Task<Invoice> CreateFromWorkOrderAsync(int workOrderId);
        Task<(IEnumerable<Invoice> Items, int TotalCount)> GetPagedInvoicesAsync(int pageNumber, int pageSize, string? search);
    }
}
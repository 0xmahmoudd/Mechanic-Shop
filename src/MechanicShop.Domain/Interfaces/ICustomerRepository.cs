using System.Collections.Generic;
using System.Threading.Tasks;
using MechanicShop.Domain.Entities;
using static MechanicShop.Domain.Interfaces.IGenericRepository;

namespace MechanicShop.Domain.Interfaces
{
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        Task<(IEnumerable<Customer> Items, int TotalCount)> GetPagedCustomersAsync(int pageNumber, int pageSize, string? search);
    }
}

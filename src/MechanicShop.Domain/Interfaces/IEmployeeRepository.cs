using System.Collections.Generic;
using System.Threading.Tasks;
using MechanicShop.Domain.Entities;
using static MechanicShop.Domain.Interfaces.IGenericRepository;

namespace MechanicShop.Domain.Interfaces
{
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        Task<(IEnumerable<Employee> Items, int TotalCount)> GetPagedEmployeesAsync(int pageNumber, int pageSize, string? search);
        Task<Employee?> GetWithDetailsAsync(int id);
    }
}

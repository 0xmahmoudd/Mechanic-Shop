using System.Collections.Generic;
using System.Threading.Tasks;
using MechanicShop.Application.DTO.Employee;

namespace MechanicShop.Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto);
        Task<(IEnumerable<EmployeeDto> Items, int TotalCount)> GetAllEmployeesAsync(int pageNumber, int pageSize, string? search);
        Task<EmployeeDto> GetEmployeeByIdAsync(int employeeId);
    }
}

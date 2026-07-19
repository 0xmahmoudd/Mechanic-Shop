using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MechanicShop.Domain.Entities;
using MechanicShop.Domain.Enums;

namespace MechanicShop.Domain.Interfaces
{
    public interface IWorkOrderRepository : IGenericRepository
    {
        Task<WorkOrder?> GetWithDetailsAsync(int id);
        Task<IEnumerable<WorkOrder>> GetAllWithDetailsAsync();
        Task<(IEnumerable<WorkOrder> Items, int TotalCount)> GetPagedWithDetailsAsync(int pageNumber, int pageSize, string? state, string? search);
        Task AssignEmployeesAsync(int workOrderId, List<int> employeeIds);
        Task AddRepairTasksAsync(int workOrderId, List<int> taskIds);
        Task AddPartsAsync(int workOrderId, List<int> partIds);
        Task<bool> ChangeStateAsync(int workOrderId, WorkOrderState newState);
    }
}
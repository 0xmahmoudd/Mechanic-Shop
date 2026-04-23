using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MechanicShop.Application.DTO.WorkOrder;
using MechanicShop.Domain.Common;
using MechanicShop.Domain.Enums;

namespace MechanicShop.Application.Interfaces
{
    public interface IWorkOrderService
    {
        Task<PagedResult<WorkOrderDto>> GetAllWorkOrdersAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? state = null,
            string? search = null);
        
        Task<WorkOrderDto> GetWorkOrderByIdAsync(int workOrderId);
        
        Task<IEnumerable<WorkOrderDto>> GetWorkOrdersByEmployeeIdAsync(int employeeId);
        
        Task<IEnumerable<WorkOrderDto>> GetWorkOrdersByCustomerIdAsync(int customerId);
        
        Task<IEnumerable<WorkOrderDto>> GetWorkOrdersByVehicleIdAsync(int vehicleId);
        Task<WorkOrderDto> CreateWorkOrderAsync(CreateWorkOrderDto dto);
        Task<WorkOrderDto> AssignEmployeesAsync(int workOrderId, List<int> employeeIds);
        Task<WorkOrderDto> AddRepairTasksAsync(int workOrderId, List<int> taskIds);
        Task<WorkOrderDto> AddPartsAsync(int workOrderId, List<int> partIds);
        Task<WorkOrderDto> UpdateWorkOrderAsync(int workOrderId);
        Task<WorkOrderDto> ChangeStateAsync(int workOrderId, WorkOrderState newState);
        Task<WorkOrderDto> StartWorkOrderAsync(int workOrderId, int employeeId);
        Task<WorkOrderDto> CancelWorkOrderAsync(int workOrderId, int employeeId);
        Task<WorkOrderDto> UpdateHoursAsync(int workOrderId, int employeeId, decimal hoursWorked);
        Task<WorkOrderDto> CompleteWorkOrderAsync(int workOrderId, int? employeeId = null);
        Task<WorkOrderDto> UpdatePartUsageAsync(int workOrderId, int employeeId, List<PartUsageUpdateDto> partUsages);

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MechanicShop.Application.DTO.WorkOrder;
using MechanicShop.Application.Interfaces;
using MechanicShop.Domain.Common;
using MechanicShop.Domain.Entities;
using MechanicShop.Domain.Enums;
using MechanicShop.Domain.Interfaces;

namespace MechanicShop.Application.Services
{
    public class WorkOrderService : IWorkOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkOrderRepository _workOrderRepository;
        private readonly IInvoiceRepository _invoiceRepository;

        public WorkOrderService(IUnitOfWork unitOfWork, IWorkOrderRepository workOrderRepository, IInvoiceRepository invoiceRepository)
        {
            _unitOfWork = unitOfWork;
            _workOrderRepository = workOrderRepository;
            _invoiceRepository = invoiceRepository;
        }

        private static WorkOrderDto MapToDto(WorkOrder workOrder) => new()
        {
            Id = workOrder.Id,
            VehicleId = workOrder.VehicleId,
            StartAt = workOrder.StartAt ?? DateTime.MinValue,
            EndAt = workOrder.EndAt,
            State = workOrder.State.ToString(), // Convert enum to string
            Vehicle = new VehicleInfo
            {
                Id = workOrder.Vehicle.Id,
                Make = workOrder.Vehicle.Make,
                Model = workOrder.Vehicle.Model,
                Year = workOrder.Vehicle.Year,
                LicensePlate = workOrder.Vehicle.LicensePlate ?? string.Empty,
                Customer = new CustomerInfo
                {
                    Id = workOrder.Vehicle.Customer.Id,
                    Name = $"{workOrder.Vehicle.Customer.User.FirstName} {workOrder.Vehicle.Customer.User.LastName}",
                    Email = workOrder.Vehicle.Customer.User.Email,
                    PhoneNumber = workOrder.Vehicle.Customer.PhoneNumber
                }
            },
            Employees = workOrder.WorkOrderEmployees.Select(woe => new EmployeeInfo
            {
                Id = woe.Employee.Id,
                Name = $"{woe.Employee.User.FirstName} {woe.Employee.User.LastName}",
                Email = woe.Employee.User.Email,
                Title = woe.Employee.Title
            }).ToList(),
            CreatedAt = workOrder.CreatedAt,
            UpdatedAt = workOrder.UpdatedAt
        };


        public async Task<WorkOrderDto> CreateWorkOrderAsync(CreateWorkOrderDto dto)
        {
            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            var workOrder = new WorkOrder
            {
                VehicleId = dto.VehicleId,
                StartAt = now,
                State = WorkOrderState.Scheduled,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _unitOfWork.WorkOrders.AddAsync(workOrder);
            await _unitOfWork.SaveChangesAsync();

            // If employee IDs, repair task IDs, or part IDs are provided, add them
            if (dto.EmployeeIds != null && dto.EmployeeIds.Any())
            {
                await _workOrderRepository.AssignEmployeesAsync(workOrder.Id, dto.EmployeeIds);
            }

            if (dto.RepairTaskIds != null && dto.RepairTaskIds.Any())
            {
                await _workOrderRepository.AddRepairTasksAsync(workOrder.Id, dto.RepairTaskIds);
            }

            if (dto.PartIds != null && dto.PartIds.Any())
            {
                await _workOrderRepository.AddPartsAsync(workOrder.Id, dto.PartIds);
            }

            // Fetch the complete work order with details
            var createdWorkOrder = await _workOrderRepository.GetWithDetailsAsync(workOrder.Id);
            return MapToDto(createdWorkOrder!);
        }

        public async Task<WorkOrderDto> AssignEmployeesAsync(int workOrderId, List<int> employeeIds)
        {
            await _workOrderRepository.AssignEmployeesAsync(workOrderId, employeeIds);
            
            var workOrder = await _workOrderRepository.GetWithDetailsAsync(workOrderId);
            if (workOrder == null)
                throw new KeyNotFoundException($"WorkOrder with ID {workOrderId} not found.");

            return MapToDto(workOrder);
        }

        public async Task<WorkOrderDto> AddRepairTasksAsync(int workOrderId, List<int> taskIds)
        {
            await _workOrderRepository.AddRepairTasksAsync(workOrderId, taskIds);
            
            var workOrder = await _workOrderRepository.GetWithDetailsAsync(workOrderId);
            if (workOrder == null)
                throw new KeyNotFoundException($"WorkOrder with ID {workOrderId} not found.");

            return MapToDto(workOrder);
        }

        public async Task<WorkOrderDto> AddPartsAsync(int workOrderId, List<int> partIds)
        {
            await _workOrderRepository.AddPartsAsync(workOrderId, partIds);
            
            var workOrder = await _workOrderRepository.GetWithDetailsAsync(workOrderId);
            if (workOrder == null)
                throw new KeyNotFoundException($"WorkOrder with ID {workOrderId} not found.");

            return MapToDto(workOrder);
        }

        public async Task<WorkOrderDto> ChangeStateAsync(int workOrderId, WorkOrderState newState)
        {
            await _workOrderRepository.ChangeStateAsync(workOrderId, newState);
            
            var workOrder = await _workOrderRepository.GetWithDetailsAsync(workOrderId);
            if (workOrder == null)
                throw new KeyNotFoundException($"WorkOrder with ID {workOrderId} not found.");

            return MapToDto(workOrder);
        }

        public async Task<WorkOrderDto> CompleteWorkOrderAsync(int workOrderId)
        {
            var workOrder = await _workOrderRepository.GetWithDetailsAsync(workOrderId);

            if (workOrder == null)
                throw new KeyNotFoundException($"WorkOrder with ID {workOrderId} not found.");

            if (workOrder.State != WorkOrderState.In_Progress)
                throw new InvalidOperationException($"WorkOrder must be in 'In_Progress' state to be completed. Current state: {workOrder.State}.");

            // Change state to Completed
            await _workOrderRepository.ChangeStateAsync(workOrderId, WorkOrderState.Completed);

            // Auto-generate invoice
            await _invoiceRepository.CreateFromWorkOrderAsync(workOrderId);

            // Fetch the updated work order with all details
            var completedWorkOrder = await _workOrderRepository.GetWithDetailsAsync(workOrderId);
            return MapToDto(completedWorkOrder!);
        }

        public async Task<WorkOrderDto> UpdateWorkOrderAsync(int workOrderId)
        {
            var workOrder = await _unitOfWork.WorkOrders.GetByIdAsync(workOrderId);
            
            if (workOrder == null)
                throw new KeyNotFoundException($"WorkOrder with ID {workOrderId} not found.");

            workOrder.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            await _unitOfWork.WorkOrders.UpdateAsync(workOrder);
            await _unitOfWork.SaveChangesAsync();

            // Re-fetch with full details for DTO mapping
            var updated = await _workOrderRepository.GetWithDetailsAsync(workOrderId);
            return MapToDto(updated!);
        }

        public async Task<PagedResult<WorkOrderDto>> GetAllWorkOrdersAsync(
            int pageNumber = 1, 
            int pageSize = 20, 
            string? state = null, 
            string? search = null)
        {
            var allWorkOrders = await _workOrderRepository.GetAllWithDetailsAsync();
            
            // Apply state filter if provided
            if (!string.IsNullOrWhiteSpace(state))
            {
                if (Enum.TryParse<WorkOrderState>(state, true, out var stateEnum))
                {
                    allWorkOrders = allWorkOrders.Where(wo => wo.State == stateEnum);
                }
            }
            
            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                allWorkOrders = allWorkOrders.Where(wo =>
                    wo.Vehicle.Make.ToLower().Contains(searchLower) ||
                    wo.Vehicle.Model.ToLower().Contains(searchLower) ||
                    (wo.Vehicle.LicensePlate != null && wo.Vehicle.LicensePlate.ToLower().Contains(searchLower)) ||
                    wo.Vehicle.Customer.User.FirstName.ToLower().Contains(searchLower) ||
                    wo.Vehicle.Customer.User.LastName.ToLower().Contains(searchLower) ||
                    wo.Vehicle.Customer.User.Email.ToLower().Contains(searchLower)
                );
            }
            
            var workOrdersList = allWorkOrders.ToList();
            var totalCount = workOrdersList.Count;
            
            // Apply pagination
            var paginatedWorkOrders = workOrdersList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToDto)
                .ToList();
            
            return new PagedResult<WorkOrderDto>
            {
                Items = paginatedWorkOrders,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<WorkOrderDto> GetWorkOrderByIdAsync(int workOrderId)
        {
            var workOrder = await _workOrderRepository.GetWithDetailsAsync(workOrderId);
            
            if (workOrder == null)
                throw new KeyNotFoundException($"WorkOrder with ID {workOrderId} not found.");

            return MapToDto(workOrder);
        }

        public async Task<IEnumerable<WorkOrderDto>> GetWorkOrdersByEmployeeIdAsync(int employeeId)
        {
            var allWorkOrders = await _workOrderRepository.GetAllWithDetailsAsync();
            var workOrders = allWorkOrders.Where(wo => 
                wo.WorkOrderEmployees.Any(woe => woe.EmployeeId == employeeId));
            
            return workOrders.Select(MapToDto).ToList();
        }

        public async Task<IEnumerable<WorkOrderDto>> GetWorkOrdersByCustomerIdAsync(int customerId)
        {
            var allWorkOrders = await _workOrderRepository.GetAllWithDetailsAsync();
            var workOrders = allWorkOrders.Where(wo => 
                wo.Vehicle.CustomerId == customerId);
            
            return workOrders.Select(MapToDto).ToList();
        }

        public async Task<IEnumerable<WorkOrderDto>> GetWorkOrdersByVehicleIdAsync(int vehicleId)
        {
            var allWorkOrders = await _workOrderRepository.GetAllWithDetailsAsync();
            var workOrders = allWorkOrders.Where(wo => wo.VehicleId == vehicleId);
            
            return workOrders.Select(MapToDto).ToList();
        }
    }
}
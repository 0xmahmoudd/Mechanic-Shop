using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MechanicShop.Domain.Entities;
using MechanicShop.Domain.Enums;
using MechanicShop.Domain.Interfaces;
using MechanicShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Infrastructure.Repositories
{
    public class WorkOrderRepository : GenericRepository<WorkOrder>, IWorkOrderRepository
    {
        public WorkOrderRepository(MechanicShopDbContext context) : base(context) { }

        public async Task<WorkOrder?> GetWithDetailsAsync(int id)
        {
            return await _context.WorkOrders
                .AsNoTracking()
                .Include(wo => wo.Vehicle)
                    .ThenInclude(v => v.Customer)
                        .ThenInclude(c => c.User)
                .Include(wo => wo.WorkOrderEmployees)
                    .ThenInclude(woe => woe.Employee)
                        .ThenInclude(e => e.User)
                .Include(wo => wo.WorkOrderRepairTasks)
                    .ThenInclude(wort => wort.RepairTask)
                .Include(wo => wo.WorkOrderParts)
                    .ThenInclude(wop => wop.Part)
                .Include(wo => wo.Invoice)
                .FirstOrDefaultAsync(wo => wo.Id == id && !wo.IsDeleted);
        }

        public async Task<IEnumerable<WorkOrder>> GetAllWithDetailsAsync()
        {
            return await _context.WorkOrders
                .AsNoTracking()
                .Where(wo => !wo.IsDeleted)
                .Include(wo => wo.Vehicle)
                    .ThenInclude(v => v.Customer)
                        .ThenInclude(c => c.User)
                .Include(wo => wo.WorkOrderEmployees)
                    .ThenInclude(woe => woe.Employee)
                        .ThenInclude(e => e.User)
                .Include(wo => wo.WorkOrderRepairTasks)
                    .ThenInclude(wort => wort.RepairTask)
                .Include(wo => wo.WorkOrderParts)
                    .ThenInclude(wop => wop.Part)
                .Include(wo => wo.Invoice)
                .OrderByDescending(wo => wo.CreatedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<WorkOrder> Items, int TotalCount)> GetPagedWithDetailsAsync(int pageNumber, int pageSize, string? state, string? search)
        {
            var query = _context.WorkOrders
                .AsNoTracking()
                .Where(wo => !wo.IsDeleted);

            // Apply state filter if provided
            if (!string.IsNullOrWhiteSpace(state))
            {
                if (Enum.TryParse<WorkOrderState>(state, true, out var stateEnum))
                {
                    query = query.Where(wo => wo.State == stateEnum);
                }
            }

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(wo =>
                    wo.Vehicle.Make.ToLower().Contains(searchLower) ||
                    wo.Vehicle.Model.ToLower().Contains(searchLower) ||
                    (wo.Vehicle.LicensePlate != null && wo.Vehicle.LicensePlate.ToLower().Contains(searchLower)) ||
                    wo.Vehicle.Customer.User.FirstName.ToLower().Contains(searchLower) ||
                    wo.Vehicle.Customer.User.LastName.ToLower().Contains(searchLower) ||
                    wo.Vehicle.Customer.User.Email.ToLower().Contains(searchLower)
                );
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Include(wo => wo.Vehicle)
                    .ThenInclude(v => v.Customer)
                        .ThenInclude(c => c.User)
                .Include(wo => wo.WorkOrderEmployees)
                    .ThenInclude(woe => woe.Employee)
                        .ThenInclude(e => e.User)
                .Include(wo => wo.WorkOrderRepairTasks)
                    .ThenInclude(wort => wort.RepairTask)
                .Include(wo => wo.WorkOrderParts)
                    .ThenInclude(wop => wop.Part)
                .Include(wo => wo.Invoice)
                .OrderByDescending(wo => wo.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task AssignEmployeesAsync(int workOrderId, List<int> employeeIds)
        {
            var workOrder = await _context.WorkOrders
                .Include(wo => wo.WorkOrderEmployees)
                .FirstOrDefaultAsync(wo => wo.Id == workOrderId && !wo.IsDeleted);

            if (workOrder == null)
                throw new KeyNotFoundException($"WorkOrder with ID {workOrderId} not found.");

            // Validate that all employees exist
            var employees = await _context.Employees
                .Where(e => employeeIds.Contains(e.Id) && !e.IsDeleted)
                .ToListAsync();

            if (employees.Count != employeeIds.Count)
                throw new ArgumentException("One or more employee IDs are invalid.");

            // Add only new employees that aren't already assigned
            foreach (var employeeId in employeeIds)
            {
                if (!workOrder.WorkOrderEmployees.Any(woe => woe.EmployeeId == employeeId))
                {
                    workOrder.WorkOrderEmployees.Add(new WorkOrderEmployee
                    {
                        WorkOrderId = workOrderId,
                        EmployeeId = employeeId,
                        HoursWorked = 0
                    });
                }
            }

            workOrder.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            await _context.SaveChangesAsync();
        }

        public async Task AddRepairTasksAsync(int workOrderId, List<int> taskIds)
        {
            var workOrder = await _context.WorkOrders
                .Include(wo => wo.WorkOrderRepairTasks)
                .FirstOrDefaultAsync(wo => wo.Id == workOrderId && !wo.IsDeleted);

            if (workOrder == null)
                throw new KeyNotFoundException($"WorkOrder with ID {workOrderId} not found.");

            // Get repair tasks with their current default labor costs
            var repairTasks = await _context.RepairTasks
                .Where(rt => taskIds.Contains(rt.Id) && !rt.IsDeleted)
                .ToListAsync();

            if (repairTasks.Count != taskIds.Count)
                throw new ArgumentException("One or more repair task IDs are invalid.");

            // Add only new repair tasks that aren't already linked
            foreach (var repairTask in repairTasks)
            {
                if (!workOrder.WorkOrderRepairTasks.Any(wort => wort.RepairTaskId == repairTask.Id))
                {
                    workOrder.WorkOrderRepairTasks.Add(new WorkOrderRepairTask
                    {
                        WorkOrderId = workOrderId,
                        RepairTaskId = repairTask.Id,
                        Quantity = 1,
                        LaborCostAtUse = repairTask.DefaultLaborCost // Capture the cost at the time of addition
                    });
                }
            }

            workOrder.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            await _context.SaveChangesAsync();
        }

        public async Task AddPartsAsync(int workOrderId, List<int> partIds)
        {
            var workOrder = await _context.WorkOrders
                .Include(wo => wo.WorkOrderParts)
                .FirstOrDefaultAsync(wo => wo.Id == workOrderId && !wo.IsDeleted);

            if (workOrder == null)
                throw new KeyNotFoundException($"WorkOrder with ID {workOrderId} not found.");

            // Get parts with their current costs
            var parts = await _context.Parts
                .Where(p => partIds.Contains(p.Id) && !p.IsDeleted)
                .ToListAsync();

            if (parts.Count != partIds.Count)
                throw new ArgumentException("One or more part IDs are invalid.");

            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            // Add parts (can have duplicates with different IDs since WorkOrderPart has its own ID)
            foreach (var part in parts)
            {
                workOrder.WorkOrderParts.Add(new WorkOrderPart
                {
                    WorkOrderId = workOrderId,
                    PartId = part.Id,
                    QuantityUsed = 0, // Initialize to 0
                    UnitPriceAtUse = part.CurrentCost, // Capture the cost at the time of addition
                    CreatedAt = now
                });
            }

            workOrder.UpdatedAt = now;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ChangeStateAsync(int workOrderId, WorkOrderState newState)
        {
            var workOrder = await _context.WorkOrders
                .FirstOrDefaultAsync(wo => wo.Id == workOrderId && !wo.IsDeleted);

            if (workOrder == null)
                throw new KeyNotFoundException($"WorkOrder with ID {workOrderId} not found.");

            // Validate state transition (optional - add business logic here if needed)
            workOrder.State = newState;
            workOrder.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            // If state is completed, set EndAt timestamp
            if (newState == WorkOrderState.Completed && workOrder.EndAt == null)
            {
                workOrder.EndAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
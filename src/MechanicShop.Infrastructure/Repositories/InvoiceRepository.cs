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
    public class InvoiceRepository : GenericRepository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(MechanicShopDbContext context) : base(context) { }

        public async Task<Invoice?> GetByWorkOrderIdAsync(int workOrderId)
        {
            return await _context.Invoices
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.WorkOrderId == workOrderId && !i.IsDeleted);
        }

        public async Task<Invoice> CreateFromWorkOrderAsync(int workOrderId)
        {
            var workOrder = await _context.WorkOrders
                .Include(wo => wo.WorkOrderParts)
                .Include(wo => wo.WorkOrderRepairTasks)
                .Include(wo => wo.WorkOrderEmployees)
                    .ThenInclude(woe => woe.Employee)
                        .ThenInclude(e => e.EmployeeSalaryHistories)
                .FirstOrDefaultAsync(wo => wo.Id == workOrderId && !wo.IsDeleted);

            if (workOrder == null)
                throw new KeyNotFoundException($"WorkOrder with ID {workOrderId} not found.");

            // 1. Parts cost: Sum(QuantityUsed * UnitPriceAtUse)
            var partsCost = workOrder.WorkOrderParts
                .Sum(wop => wop.QuantityUsed * wop.UnitPriceAtUse);

            // 2. Repair tasks cost: Sum(Quantity * LaborCostAtUse)
            var repairTasksCost = workOrder.WorkOrderRepairTasks
                .Sum(wort => wort.Quantity * wort.LaborCostAtUse);

            // 3. Labor cost: Sum(HoursWorked * HourlyRate) using salary history effective at WorkOrder.StartAt
            decimal laborCost = 0;
            if (workOrder.StartAt.HasValue)
            {
                var startDate = DateOnly.FromDateTime(workOrder.StartAt.Value);

                foreach (var woe in workOrder.WorkOrderEmployees)
                {
                    var salaryRecord = woe.Employee.EmployeeSalaryHistories
                        .FirstOrDefault(sh =>
                            sh.EffectiveFrom <= startDate &&
                            (sh.EffectiveTo == null || sh.EffectiveTo > startDate));

                    if (salaryRecord != null && woe.HoursWorked.HasValue)
                    {
                        laborCost += woe.HoursWorked.Value * salaryRecord.HourlyRate;
                    }
                }
            }

            // 4. Calculate totals
            var subtotal = partsCost + repairTasksCost + laborCost;
            var discount = 0m;
            var taxRate = 0.08m; // Fixed 8% tax rate
            var taxAmount = Math.Round((subtotal - discount) * taxRate, 2);
            var totalAmount = subtotal - discount + taxAmount;

            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            var invoice = new Invoice
            {
                WorkOrderId = workOrderId,
                Subtotal = subtotal,
                Discount = discount,
                TaxRate = taxRate,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount,
                PaymentStatus = PaymentStatus.Pending,
                IssuedAt = now,
                CreatedAt = now
            };

            await _context.Invoices.AddAsync(invoice);
            await _context.SaveChangesAsync();

            return invoice;
        }
        public async Task<(IEnumerable<Invoice> Items, int TotalCount)> GetPagedInvoicesAsync(int pageNumber, int pageSize, string? search)
        {
            var query = _dbSet.AsNoTracking().Where(i => !i.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                // We can't easily search WorkOrder.Customer here without Includes, so let's add them.
            }

            // Always include WorkOrder for invoice details if needed
            query = query.Include(i => i.WorkOrder);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(i => i.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}

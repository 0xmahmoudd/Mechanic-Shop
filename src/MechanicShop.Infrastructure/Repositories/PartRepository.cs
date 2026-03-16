using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MechanicShop.Domain.Entities;
using MechanicShop.Domain.Interfaces;
using MechanicShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Infrastructure.Repositories
{
    public class PartRepository : GenericRepository<Part>, IPartRepository
    {
        public PartRepository(MechanicShopDbContext context) : base(context)
        {
        }

        public async Task CreatePriceHistoryAsync(int partId, decimal currentCost)
        {
            var now = DateTime.Now;

            // Close any currently active price history records (where EffectiveTo is NULL)
            var activePriceHistory = await _context.PartPriceHistories
                .Where(pph => pph.PartId == partId && pph.EffectiveTo == null)
                .ToListAsync();

            foreach (var history in activePriceHistory)
            {
                history.EffectiveTo = now;
            }

            // Create new price history record
            var newPriceHistory = new PartPriceHistory
            {
                PartId = partId,
                UnitCost = currentCost,
                EffectiveFrom = now,
                EffectiveTo = null,  // Currently active
                CreatedAt = now
            };

            await _context.PartPriceHistories.AddAsync(newPriceHistory);
        }

        public async Task DeletePriceHistoryAsync(int partId)
        {
            var priceHistories = await _context.PartPriceHistories
                .Where(pph => pph.PartId == partId)
                .ToListAsync();

            _context.PartPriceHistories.RemoveRange(priceHistories);
            await _context.SaveChangesAsync();
        }

        public async Task<(IEnumerable<Part> parts, int totalCount)> GetAllPartsAsync(string? category = null, string? supplier = null, string? search = null)
        {
            var query = _dbSet.AsQueryable();

            // Filter by category if provided
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category != null && p.Category.ToLower() == category.ToLower());
            }

            // Filter by supplier if provided
            if (!string.IsNullOrWhiteSpace(supplier))
            {
                query = query.Where(p => p.Supplier != null && p.Supplier.ToLower() == supplier.ToLower());
            }

            // Search across name and description if search term provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p => 
                    (p.Name != null && p.Name.ToLower().Contains(searchLower)) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchLower)) ||
                    (p.Category != null && p.Category.ToLower().Contains(searchLower)) ||
                    (p.Supplier != null && p.Supplier.ToLower().Contains(searchLower))
                );
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Get the filtered parts
            var parts = await query.ToListAsync();

            return (parts, totalCount);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MechanicShop.Application.DTO.Part;
using MechanicShop.Application.Interfaces;
using MechanicShop.Domain.Common;
using MechanicShop.Domain.Entities;
using MechanicShop.Domain.Interfaces;

namespace MechanicShop.Application.Services
{
    public class PartService : IPartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPartRepository _partRepository;

        public PartService(IUnitOfWork unitOfWork, IPartRepository partRepository)
        {
            _unitOfWork = unitOfWork;
            _partRepository = partRepository;
        }

        private static PartDto MapToDto(Part part) => new()
        {
            Id = part.Id,
            Name = part.Name,
            Description = part.Description,
            CurrentCost = part.CurrentCost,
            StockQuantity = part.StockQuantity,
            Category = part.Category,
            Supplier = part.Supplier,
            CreatedAt = part.CreatedAt,
            UpdatedAt = part.UpdatedAt
        };

        public async Task<PartDto> AdjustStockAsync(int partId, int adjustment)
        {
            var part = await _unitOfWork.Parts.GetByIdAsync(partId);
            if (part == null)
                throw new KeyNotFoundException($"Part with ID {partId} not found.");

            if (part.StockQuantity + adjustment < 0)
                throw new InvalidOperationException("Adjustment would result in negative stock.");

            part.StockQuantity += adjustment;
            part.UpdatedAt = DateTime.Now;
            
            await _unitOfWork.Parts.UpdateAsync(part);
            await _unitOfWork.SaveChangesAsync();
            
            return MapToDto(part);
        }

        public async Task<PartDto> CreatePartAsync(CreatePartDto dto)
        {
            var part = new Part
            {
                Name = dto.Name,
                Description = dto.Description,
                CurrentCost = dto.CurrentCost,
                StockQuantity = dto.StockQuantity,
                Category = dto.Category,
                Supplier = dto.Supplier,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now

            };

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var createdPart = await _unitOfWork.Parts.AddAsync(part);
                // Save first to get the database-generated Id
                await _unitOfWork.SaveChangesAsync();

                // Now createdPart.Id is populated, create initial price history
                await _partRepository.CreatePriceHistoryAsync(createdPart.Id, createdPart.CurrentCost);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                
                return MapToDto(createdPart);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

        }

        public async Task DeletePartAsync(int partId)
        {
            var part = await _unitOfWork.Parts.GetByIdAsync(partId);
            if (part == null)
                throw new KeyNotFoundException($"Part with ID {partId} not found.");

            // Delete associated price history records first to avoid FK violation
            await _partRepository.DeletePriceHistoryAsync(partId);

            // Soft delete
            part.IsDeleted = true;
            part.DeletedAt = DateTime.Now;
            part.UpdatedAt = DateTime.Now;
            await _unitOfWork.Parts.UpdateAsync(part);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PagedResult<PartDto>> GetAllPartsAsync(int pageNumber, int pageSize, string? category = null, string? supplier = null, string? search = null)
        {
            // Get filtered parts from repository
            var (parts, totalCount) = await _partRepository.GetAllPartsAsync(category, supplier, search);
            
            // Apply pagination
            var pagedParts = parts
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToDto)
                .ToList();
            
            // Return paged result
            return PagedResult<PartDto>.Create(pagedParts, totalCount, pageNumber, pageSize);
        }

        public async Task<PartDto> GetPartByIdAsync(int id)
        {
            var part = await _unitOfWork.Parts.GetByIdAsync(id);
            if (part == null)
            {
                throw new KeyNotFoundException($"Part with ID {id} not found.");
            }

            if (part.IsDeleted)
            {
                throw new KeyNotFoundException($"Part with ID {id} is deleted.");
            }
            
            return MapToDto(part);
        }

        public async Task<PartDto> UpdatePartAsync(int partId, UpdatePartDto dto)
        {
            var part = await _unitOfWork.Parts.GetByIdAsync(partId);
            if (part == null)
            {
                throw new KeyNotFoundException($"Part with ID {partId} not found.");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                bool costChanged = part.CurrentCost != dto.CurrentCost;

                // Update
                part.Name = dto.Name;
                part.Description = dto.Description;
                part.CurrentCost = dto.CurrentCost;
                part.StockQuantity = dto.StockQuantity;
                part.Category = dto.Category;
                part.Supplier = dto.Supplier;
                part.UpdatedAt = DateTime.Now;

                await _unitOfWork.Parts.UpdateAsync(part);

                // TODO: Create new price history if cost changed
                if (costChanged) await _partRepository.CreatePriceHistoryAsync(part.Id, part.CurrentCost);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                
                return MapToDto(part);


            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

        }
    }
}
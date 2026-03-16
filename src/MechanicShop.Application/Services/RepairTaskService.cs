using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MechanicShop.Application.DTO.Part;
using MechanicShop.Application.DTO.RepairTask;
using MechanicShop.Application.Interfaces;
using MechanicShop.Domain.Common;
using MechanicShop.Domain.Entities;
using MechanicShop.Domain.Interfaces;

namespace MechanicShop.Application.Services
{
    public class RepairTaskService : IRepairTaskService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepairTaskRepository _repairTaskRepository;

        public RepairTaskService(IUnitOfWork unitOfWork, IRepairTaskRepository repairTaskRepository)
        {
            _unitOfWork = unitOfWork;
            _repairTaskRepository = repairTaskRepository;
        }

        private static RepairTaskDto MapToDto(RepairTask repairTask) => new()
        {
            Id = repairTask.Id,
            Name = repairTask.Name,
            EstimatedDuration = repairTask.EstimatedDuration ?? TimeSpan.Zero,
            DefaultLaborCost = repairTask.DefaultLaborCost,
            CreatedAt = repairTask.CreatedAt,
            UpdatedAt = repairTask.UpdatedAt
        };

        private static PartDto MapPartToDto(Part part) => new()
        {
            Id = part.Id,
            Name = part.Name,
            Description = part.Description,
            CurrentCost = part.CurrentCost,
            StockQuantity = part.StockQuantity,
            Category = part.Category,
            Supplier = part.Supplier,
            CreatedAt = part.CreatedAt,
            UpdatedAt = part.UpdatedAt,
        };

        public async Task<PagedResult<RepairTaskDto>> GetAllTasks(int pageNumber, int pageSize)
        {
            var pagedTasks = await _unitOfWork.RepairTasks.GetPagedAsync(pageNumber, pageSize);
            
            var dtos = pagedTasks.Items.Select(MapToDto).ToList();
            
            return new PagedResult<RepairTaskDto>
            {
                Items = dtos,
                TotalCount = pagedTasks.TotalCount,
                PageNumber = pagedTasks.PageNumber,
                PageSize = pagedTasks.PageSize
            };
        }

        public async Task<RepairTaskDto> CreateTaskAsync(CreateRepairTaskDto createDto)
        {
            var existingTasks = await _repairTaskRepository.SearchByNameAsync(createDto.Name);
            if (existingTasks.Any(t => string.Equals(t.Name, createDto.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException($"A repair task with the name '{createDto.Name}' already exists.");
            }

            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            
            var repairTask = new RepairTask
            {
                Name = createDto.Name,
                EstimatedDuration = createDto.EstimatedDuration,
                DefaultLaborCost = createDto.DefaultLaborCost,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _unitOfWork.RepairTasks.AddAsync(repairTask);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(repairTask);
        }

        public async Task<RepairTaskDto> UpdateTaskAsync(int taskId, UpdateRepairTaskDto updateDto)
        {
            var repairTask = await _unitOfWork.RepairTasks.GetByIdAsync(taskId);
            
            if (repairTask == null)
                throw new KeyNotFoundException($"RepairTask with ID {taskId} not found.");

            repairTask.Name = updateDto.Name;
            repairTask.EstimatedDuration = updateDto.EstimatedDuration;
            repairTask.DefaultLaborCost = updateDto.DefaultLaborCost;
            repairTask.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            await _unitOfWork.RepairTasks.UpdateAsync(repairTask);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(repairTask);
        }

        public async Task DeleteTaskAsync(int taskId)
        {
            var repairTask = await _unitOfWork.RepairTasks.GetByIdAsync(taskId);
            
            if (repairTask == null)
                throw new KeyNotFoundException($"RepairTask with ID {taskId} not found.");

            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            repairTask.IsDeleted = true;
            repairTask.DeletedAt = now;
            repairTask.UpdatedAt = now;

            await _unitOfWork.RepairTasks.UpdateAsync(repairTask);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<PartDto>> GetAssociatedPartsAsync(int taskId)
        {
            var parts = await _repairTaskRepository.GetAssociatedPartsAsync(taskId);
            return parts.Select(MapPartToDto).ToList();
        }

        public async Task LinkPartsAsync(int taskId, List<int> partIds)
        {
            await _repairTaskRepository.LinkPartsAsync(taskId, partIds);
        }

        public async Task UnlinkPartsAsync(int taskId, List<int> partIds)
        {
            await _repairTaskRepository.UnlinkPartsAsync(taskId, partIds);
        }

        public async Task<IEnumerable<RepairTask>> SearchByNameAsync(string searchTerm)
        {
            return await _repairTaskRepository.SearchByNameAsync(searchTerm);
        }
    }
}
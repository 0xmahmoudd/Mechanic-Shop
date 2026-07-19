using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MechanicShop.Application.DTO.Employee;
using MechanicShop.Application.Interfaces;
using MechanicShop.Domain.Entities;
using MechanicShop.Domain.Enums;
using MechanicShop.Domain.Interfaces;

namespace MechanicShop.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;
        public EmployeeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
        {
            var allUsers = await _unitOfWork.Users.GetAllAsync();
            if (allUsers.Any(u => u.Email == dto.Email && !u.IsDeleted))
                throw new ArgumentException("Email already exists.");

            if (allUsers.Any(u => u.Username == dto.Username && !u.IsDeleted))
                throw new ArgumentException("Username already exists.");

            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var user = new User
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Username = dto.Username,
                    Email = dto.Email,
                    PasswordHash = dto.Password, // Following existing pattern
                    Role = UserRole.Employee,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                var employee = new Employee
                {
                    UserId = user.Id,
                    IsActive = true,
                    Title = dto.Title,
                    WorkHoursPerDay = dto.WorkHoursPerDay,
                    EmploymentStartDate = dto.EmploymentStartDate,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await _unitOfWork.Employees.AddAsync(employee);
                await _unitOfWork.SaveChangesAsync();

                var salaryHistory = new EmployeeSalaryHistory
                {
                    EmployeeId = employee.Id,
                    HourlyRate = dto.InitialHourlyRate,
                    EffectiveFrom = DateOnly.FromDateTime(now),
                    CreatedAt = now
                };

                // We need to add salary history through DbContext or generic repo.
                // Assuming it's simpler to just map it through Employee's collection if tracking is set up,
                // but let's add a generic repo for EmployeeSalaryHistory to UnitOfWork if it's not there,
                // or just let EF handle it via the employee object if we update.
                // Wait, UnitOfWork doesn't have EmployeeSalaryHistories. Let's add it or rely on EF graph.
                employee.EmployeeSalaryHistories.Add(salaryHistory);
                await _unitOfWork.Employees.UpdateAsync(employee);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();
                
                return MapToDto(employee, user, salaryHistory);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<(IEnumerable<EmployeeDto> Items, int TotalCount)> GetAllEmployeesAsync(int pageNumber, int pageSize, string? search)
        {
            var (items, totalCount) = await _unitOfWork.Employees.GetPagedEmployeesAsync(pageNumber, pageSize, search);
            var dtos = items.Select(e => MapToDto(e, e.User, e.EmployeeSalaryHistories.FirstOrDefault(sh => sh.EffectiveTo == null))).ToList();
            return (dtos, totalCount);
        }

        public async Task<EmployeeDto> GetEmployeeByIdAsync(int employeeId)
        {
            var employee = await _unitOfWork.Employees.GetWithDetailsAsync(employeeId);
            if (employee == null)
                throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");

            return MapToDto(employee, employee.User, employee.EmployeeSalaryHistories.FirstOrDefault(sh => sh.EffectiveTo == null));
        }

        private static EmployeeDto MapToDto(Employee employee, User user, EmployeeSalaryHistory? currentSalary) => new()
        {
            Id = employee.Id,
            UserId = employee.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Title = employee.Title,
            WorkHoursPerDay = employee.WorkHoursPerDay,
            EmploymentStartDate = employee.EmploymentStartDate,
            IsActive = employee.IsActive,
            CurrentHourlyRate = currentSalary?.HourlyRate ?? 0
        };
    }
}

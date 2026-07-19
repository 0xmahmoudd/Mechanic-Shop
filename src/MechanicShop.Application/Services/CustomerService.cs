using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MechanicShop.Application.DTO.Customer;
using MechanicShop.Application.DTO.Vehicle;
using MechanicShop.Application.Interfaces;
using MechanicShop.Domain.Entities;
using MechanicShop.Domain.Enums;
using MechanicShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CustomerWithUserDto> RegisterCustomerAsync(RegisterCustomerDto dto)
        {
            var allUsers = await _unitOfWork.Users.GetAllAsync();
            if (allUsers.Any(u => u.Email == dto.Email && !u.IsDeleted))
                throw new ArgumentException("Email already exists.");

            if (allUsers.Any(u => u.Username == dto.Username && !u.IsDeleted))
                throw new ArgumentException("Username already exists.");

            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = dto.Password,
                Role = UserRole.Customer,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var customer = new Customer
            {
                UserId = user.Id,
                PhoneNumber = dto.PhoneNumber,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            return MapToCustomerWithUserDto(customer, user);
        }

        public async Task<(IEnumerable<CustomerDto> Items, int TotalCount)> GetAllCustomersAsync(int pageNumber, int pageSize, string? search)
        {
            var (items, totalCount) = await _unitOfWork.Customers.GetPagedCustomersAsync(pageNumber, pageSize, search);
            var dtos = items.Select(c => MapToDto(c, c.User)).ToList();
            return (dtos, totalCount);
        }

        public async Task<CustomerDto> GetCustomerProfileAsync(int customerId)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null || customer.IsDeleted)
                throw new KeyNotFoundException($"Customer with ID {customerId} not found.");

            var user = await _unitOfWork.Users.GetByIdAsync(customer.UserId);
            if (user == null)
                throw new KeyNotFoundException($"User for customer {customerId} not found.");

            return MapToDto(customer, user);
        }

        public async Task<CustomerDto> UpdateCustomerProfileAsync(int customerId, UpdateCustomerProfileDto dto)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null || customer.IsDeleted)
                throw new KeyNotFoundException($"Customer with ID {customerId} not found.");

            customer.PhoneNumber = dto.PhoneNumber;
            customer.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            await _unitOfWork.Customers.UpdateAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            return await GetCustomerProfileAsync(customerId);
        }

        public async Task<IEnumerable<VehicleDto>> GetCustomerVehiclesAsync(int customerId)
        {
            var allVehicles = await _unitOfWork.Vehicles.GetAllAsync();
            var vehicles = allVehicles
                .Where(v => v.CustomerId == customerId && !v.IsDeleted)
                .OrderBy(v => v.CreatedAt);

            return vehicles.Select(MapToVehicleDto).ToList();
        }

        public async Task<VehicleDto> AddVehicleAsync(int customerId, CreateVehicleDto dto)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null || customer.IsDeleted)
                throw new KeyNotFoundException($"Customer with ID {customerId} not found.");

            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            var vehicle = new Vehicle
            {
                CustomerId = customerId,
                Make = dto.Make,
                Model = dto.Model,
                Year = dto.Year,
                LicensePlate = dto.LicensePlate,
                Vin = dto.Vin,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _unitOfWork.Vehicles.AddAsync(vehicle);
            await _unitOfWork.SaveChangesAsync();

            return MapToVehicleDto(vehicle);
        }

        public async Task<VehicleDto> UpdateVehicleAsync(int customerId, int vehicleId, UpdateVehicleDto dto)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(vehicleId);
            if (vehicle == null || vehicle.IsDeleted)
                throw new KeyNotFoundException($"Vehicle with ID {vehicleId} not found.");

            if (vehicle.CustomerId != customerId)
                throw new UnauthorizedAccessException("Vehicle does not belong to this customer.");

            vehicle.Make = dto.Make;
            vehicle.Model = dto.Model;
            vehicle.Year = dto.Year;
            vehicle.LicensePlate = dto.LicensePlate;
            vehicle.Vin = dto.Vin;
            vehicle.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            await _unitOfWork.Vehicles.UpdateAsync(vehicle);
            await _unitOfWork.SaveChangesAsync();

            return MapToVehicleDto(vehicle);
        }

        public async Task DeleteVehicleAsync(int customerId, int vehicleId)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(vehicleId);
            if (vehicle == null || vehicle.IsDeleted)
                throw new KeyNotFoundException($"Vehicle with ID {vehicleId} not found.");

            if (vehicle.CustomerId != customerId)
                throw new UnauthorizedAccessException("Vehicle does not belong to this customer.");

            vehicle.IsDeleted = true;
            vehicle.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            vehicle.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            await _unitOfWork.Vehicles.UpdateAsync(vehicle);
            await _unitOfWork.SaveChangesAsync();
        }

        private static CustomerDto MapToDto(Customer customer, User user) => new()
        {
            Id = customer.Id,
            UserId = customer.UserId,
            PhoneNumber = customer.PhoneNumber,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt,
            User = new UserInfo
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role.ToString()
            }
        };

        private static CustomerWithUserDto MapToCustomerWithUserDto(Customer customer, User user) => new()
        {
            Id = customer.Id,
            UserId = customer.UserId,
            PhoneNumber = customer.PhoneNumber,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt,
            User = new UserDetails
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString()
            }
        };

        private static VehicleDto MapToVehicleDto(Vehicle vehicle) => new()
        {
            Id = vehicle.Id,
            CustomerId = vehicle.CustomerId,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            LicensePlate = vehicle.LicensePlate,
            Vin = vehicle.Vin,
            CreatedAt = vehicle.CreatedAt,
            UpdatedAt = vehicle.UpdatedAt
        };
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MechanicShop.Application.DTO.Customer;
using MechanicShop.Application.DTO.Vehicle;

namespace MechanicShop.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<CustomerWithUserDto> RegisterCustomerAsync(RegisterCustomerDto dto);
        Task<CustomerDto> GetCustomerProfileAsync(int customerId);
        Task<CustomerDto> UpdateCustomerProfileAsync(int customerId, UpdateCustomerProfileDto dto);
        Task<IEnumerable<VehicleDto>> GetCustomerVehiclesAsync(int customerId);
        Task<VehicleDto> AddVehicleAsync(int customerId, CreateVehicleDto dto);
        Task<VehicleDto> UpdateVehicleAsync(int customerId, int vehicleId, UpdateVehicleDto dto);
        Task DeleteVehicleAsync(int customerId, int vehicleId);
    }
}

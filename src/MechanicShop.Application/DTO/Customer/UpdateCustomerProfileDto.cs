using System;

namespace MechanicShop.Application.DTO.Customer
{
    public record UpdateCustomerProfileDto
    {
        public string? PhoneNumber { get; init; }
    }
}

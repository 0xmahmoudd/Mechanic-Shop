using System;

namespace MechanicShop.Application.DTO.Vehicle
{
    public record VehicleDto
    {
        public int Id { get; init; }
        public int CustomerId { get; init; }
        public string Make { get; init; } = string.Empty;
        public string Model { get; init; } = string.Empty;
        public int Year { get; init; }
        public string LicensePlate { get; init; } = string.Empty;
        public string? Vin { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}

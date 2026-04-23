using System;

namespace MechanicShop.Application.DTO.Vehicle
{
    public record UpdateVehicleDto
    {
        public string Make { get; init; } = string.Empty;
        public string Model { get; init; } = string.Empty;
        public int Year { get; init; }
        public string LicensePlate { get; init; } = string.Empty;
        public string? Vin { get; init; }
    }
}

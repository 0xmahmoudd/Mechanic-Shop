using System;

namespace MechanicShop.Application.DTO.Customer
{
    public record CustomerDto
    {
        public int Id { get; init; }
        public int UserId { get; init; }
        public string? PhoneNumber { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public UserInfo User { get; init; } = null!;
    }

    public record UserInfo
    {
        public int Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
    }

    public record CustomerWithUserDto
    {
        public int Id { get; init; }
        public int UserId { get; init; }
        public string? PhoneNumber { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public UserDetails User { get; init; } = null!;
    }

    public record UserDetails
    {
        public int Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string? Username { get; init; }
        public string Email { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
    }
}

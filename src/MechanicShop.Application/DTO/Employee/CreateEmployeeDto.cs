using System;
using System.ComponentModel.DataAnnotations;

namespace MechanicShop.Application.DTO.Employee
{
    public class CreateEmployeeDto
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = null!;

        [StringLength(50)]
        public string? Title { get; set; }

        [Range(1, 24)]
        public int WorkHoursPerDay { get; set; }

        [Required]
        public decimal InitialHourlyRate { get; set; }

        public DateOnly EmploymentStartDate { get; set; }
    }
}

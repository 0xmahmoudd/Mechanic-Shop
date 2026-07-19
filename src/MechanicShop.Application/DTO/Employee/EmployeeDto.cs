using System;

namespace MechanicShop.Application.DTO.Employee
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Title { get; set; }
        public int WorkHoursPerDay { get; set; }
        public DateOnly EmploymentStartDate { get; set; }
        public bool IsActive { get; set; }
        public decimal CurrentHourlyRate { get; set; }
    }
}

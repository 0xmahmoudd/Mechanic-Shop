using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MechanicShop.Application.DTO.WorkOrder;
using MechanicShop.Application.DTO.Employee;
using MechanicShop.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MechanicShop.Api.Controller
{
    [ApiController]
    [Route("api/employees")]
    public class EmployeeController : ControllerBase
    {
        private readonly IWorkOrderService _workOrderService;

        private readonly IEmployeeService _employeeService;

        public EmployeeController(IWorkOrderService workOrderService, IEmployeeService employeeService)
        {
            _workOrderService = workOrderService;
            _employeeService = employeeService;
        }

        [HttpPost]
        public async Task<ActionResult<EmployeeDto>> CreateEmployee([FromBody] CreateEmployeeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var employee = await _employeeService.CreateEmployeeAsync(dto);
                return CreatedAtAction(nameof(GetEmployeeById), new { employeeId = employee.Id }, employee);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetAllEmployees([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        {
            try
            {
                var (items, totalCount) = await _employeeService.GetAllEmployeesAsync(pageNumber, pageSize, search);
                return Ok(new
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{employeeId}")]
        public async Task<ActionResult<EmployeeDto>> GetEmployeeById(int employeeId)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
                return Ok(employee);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("me/work-orders")]
        public async Task<ActionResult<IEnumerable<WorkOrderDto>>> GetMyWorkOrders([FromHeader(Name = "X-Employee-Id")] int employeeId)
        {
            if (employeeId <= 0)
                return BadRequest("Invalid X-Employee-Id header.");

            try
            {
                var workOrders = await _workOrderService.GetWorkOrdersByEmployeeIdAsync(employeeId);
                return Ok(workOrders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

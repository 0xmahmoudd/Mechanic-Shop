using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MechanicShop.Application.DTO.Customer;
using MechanicShop.Application.DTO.Vehicle;
using MechanicShop.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MechanicShop.Api.Controller
{
    [ApiController]
    [Route("api/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<CustomerWithUserDto>> RegisterCustomer([FromBody] RegisterCustomerDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var customer = await _customerService.RegisterCustomerAsync(dto);
                return CreatedAtAction(nameof(GetCustomerProfile), new { customerId = customer.Id }, customer);
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

        [HttpGet("me")]
        public async Task<ActionResult<CustomerDto>> GetCustomerProfile([FromHeader(Name = "X-Customer-Id")] int customerId)
        {
            if (customerId <= 0)
                return BadRequest("Invalid X-Customer-Id header.");

            try
            {
                var customer = await _customerService.GetCustomerProfileAsync(customerId);
                return Ok(customer);
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

        [HttpPut("me")]
        public async Task<ActionResult<CustomerDto>> UpdateCustomerProfile(
            [FromHeader(Name = "X-Customer-Id")] int customerId,
            [FromBody] UpdateCustomerProfileDto dto)
        {
            if (customerId <= 0)
                return BadRequest("Invalid X-Customer-Id header.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var customer = await _customerService.UpdateCustomerProfileAsync(customerId, dto);
                return Ok(customer);
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

        [HttpGet("me/vehicles")]
        public async Task<ActionResult<IEnumerable<VehicleDto>>> GetCustomerVehicles([FromHeader(Name = "X-Customer-Id")] int customerId)
        {
            if (customerId <= 0)
                return BadRequest("Invalid X-Customer-Id header.");

            try
            {
                var vehicles = await _customerService.GetCustomerVehiclesAsync(customerId);
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("me/vehicles")]
        public async Task<ActionResult<VehicleDto>> AddVehicle(
            [FromHeader(Name = "X-Customer-Id")] int customerId,
            [FromBody] CreateVehicleDto dto)
        {
            if (customerId <= 0)
                return BadRequest("Invalid X-Customer-Id header.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var vehicle = await _customerService.AddVehicleAsync(customerId, dto);
                return CreatedAtAction(nameof(GetCustomerVehicles), new { customerId }, vehicle);
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
    }
}

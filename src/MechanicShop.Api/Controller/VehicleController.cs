using System;
using System.Threading.Tasks;
using MechanicShop.Application.DTO.Vehicle;
using MechanicShop.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MechanicShop.Api.Controller
{
    [ApiController]
    [Route("api/vehicles")]
    public class VehicleController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public VehicleController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllVehicles([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        {
            try
            {
                var (items, totalCount) = await _customerService.GetAllVehiclesAsync(pageNumber, pageSize, search);
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

        [HttpPut("{vehicleId}")]
        public async Task<ActionResult<VehicleDto>> UpdateVehicle(
            int vehicleId,
            [FromHeader(Name = "X-Customer-Id")] int customerId,
            [FromBody] UpdateVehicleDto dto)
        {
            if (customerId <= 0)
                return BadRequest("Invalid X-Customer-Id header.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var vehicle = await _customerService.UpdateVehicleAsync(customerId, vehicleId, dto);
                return Ok(vehicle);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{vehicleId}")]
        public async Task<IActionResult> DeleteVehicle(
            int vehicleId,
            [FromHeader(Name = "X-Customer-Id")] int customerId)
        {
            if (customerId <= 0)
                return BadRequest("Invalid X-Customer-Id header.");

            try
            {
                await _customerService.DeleteVehicleAsync(customerId, vehicleId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

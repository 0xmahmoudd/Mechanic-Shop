using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MechanicShop.Application.DTO.Part;
using MechanicShop.Application.Interfaces;
using MechanicShop.Domain.Common;
using MechanicShop.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Api.Controller
{
    [Route("[controller]")]
    [ApiController]
    //[Route("api/[controller]")]
    public class PartController : ControllerBase
    {
        private readonly IPartService _partService;

        public PartController(IPartService partService)
        {
            _partService = partService;
        }
        [HttpGet]

        public async Task<ActionResult<PagedResult<PartDto>>> GetParts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? category = null,
            [FromQuery] string? supplier = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var result = await _partService.GetAllPartsAsync(pageNumber, pageSize, category, supplier, search);
                if (result == null) return BadRequest();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PartDto>> GetPartById(int id)
        {
            try
            {
                var part = await _partService.GetPartByIdAsync(id);
                if (part == null) return NotFound();
                return Ok(part);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<PartDto>> CreatePart([FromBody] CreatePartDto dto)
        {
            try
            {
                var part = await _partService.CreatePartAsync(dto);
                return CreatedAtAction(nameof(GetPartById), new { id = part.Id }, part);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("partId")]
        public async Task<ActionResult<PartDto>> UpdatePart(int partId, UpdatePartDto dto)
        {
            try
            {
                var part = await _partService.UpdatePartAsync(partId, dto);
                return Ok(part);
            }
            catch (KeyNotFoundException)
            {
                
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{partId}")]
        public async Task<IActionResult> DeletePart(int partId)
        {
            try
            {
                await _partService.DeletePartAsync(partId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPatch("{partId}/adjust-stock")]
        public async Task<ActionResult<PartDto>> AdjustStock(int partId, [FromBody] AdjustStockDto dto)
        {
            try
            {
                var part = await _partService.AdjustStockAsync(partId, dto.Adjustment);
                return Ok(part);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
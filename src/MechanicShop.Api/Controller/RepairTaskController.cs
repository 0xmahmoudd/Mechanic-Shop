using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MechanicShop.Application.DTO.Part;
using MechanicShop.Application.DTO.RepairTask;
using MechanicShop.Application.Interfaces;
using MechanicShop.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace MechanicShop.Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class RepairTaskController : ControllerBase
    {
        private readonly IRepairTaskService _repairTaskService;

        public RepairTaskController(IRepairTaskService repairTaskService)
        {
            _repairTaskService = repairTaskService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<RepairTaskDto>>> GetAllTasks(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var res = await _repairTaskService.GetAllTasks(pageNumber, pageSize);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<RepairTaskDto>> CreateTask([FromBody] CreateRepairTaskDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var createdTask = await _repairTaskService.CreateTaskAsync(createDto);
                return CreatedAtAction(nameof(GetAllTasks), new { }, createdTask);
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

        [HttpPut("{taskId}")]
        public async Task<ActionResult<RepairTaskDto>> UpdateTask(int taskId, [FromBody] UpdateRepairTaskDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedTask = await _repairTaskService.UpdateTaskAsync(taskId, updateDto);
                return Ok(updatedTask);
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

        [HttpDelete("{taskId}")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            try
            {
                await _repairTaskService.DeleteTaskAsync(taskId);
                return NoContent();
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

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<MechanicShop.Domain.Entities.RepairTask>>> SearchByName([FromQuery] string searchTerm)
        {
            try
            {
                var tasks = await _repairTaskService.SearchByNameAsync(searchTerm);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{taskId}/parts")]
        public async Task<ActionResult<IEnumerable<PartDto>>> GetAssociatedParts(int taskId)
        {
            try
            {
                var parts = await _repairTaskService.GetAssociatedPartsAsync(taskId);
                return Ok(parts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{taskId}/parts")]
        public async Task<IActionResult> LinkParts(int taskId, [FromBody] List<int> partIds)
        {
            try
            {
                await _repairTaskService.LinkPartsAsync(taskId, partIds);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
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

        [HttpDelete("{taskId}/parts")]
        public async Task<IActionResult> UnlinkParts(int taskId, [FromBody] List<int> partIds)
        {
            try
            {
                await _repairTaskService.UnlinkPartsAsync(taskId, partIds);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
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

    }
}
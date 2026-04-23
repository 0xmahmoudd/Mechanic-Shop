using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MechanicShop.Application.DTO.WorkOrder;
using MechanicShop.Application.Interfaces;
using MechanicShop.Domain.Common;
using MechanicShop.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MechanicShop.Api.Controller
{
    [ApiController]
    [Route("api/work-orders")]
    public class WorkOrderController : ControllerBase
    {
        private readonly IWorkOrderService _workOrderService;

        public WorkOrderController(IWorkOrderService workOrderService)
        {
            _workOrderService = workOrderService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<WorkOrderDto>>> GetAllWorkOrders(
            int pageNumber = 1, 
            int pageSize = 20, 
            string? state = null, 
            string? search = null)
        {
            try
            {
                var result = await _workOrderService.GetAllWorkOrdersAsync(pageNumber, pageSize, state, search);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{workOrderId}")]
        public async Task<ActionResult<WorkOrderDto>> GetWorkOrderById(int workOrderId)
        {
            try
            {
                var workOrder = await _workOrderService.GetWorkOrderByIdAsync(workOrderId);
                return Ok(workOrder);
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

        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<IEnumerable<WorkOrderDto>>> GetWorkOrdersByEmployee(int employeeId)
        {
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

        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<WorkOrderDto>>> GetWorkOrdersByCustomer(int customerId)
        {
            try
            {
                var workOrders = await _workOrderService.GetWorkOrdersByCustomerIdAsync(customerId);
                return Ok(workOrders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("vehicle/{vehicleId}")]
        public async Task<ActionResult<IEnumerable<WorkOrderDto>>> GetWorkOrdersByVehicle(int vehicleId)
        {
            try
            {
                var workOrders = await _workOrderService.GetWorkOrdersByVehicleIdAsync(vehicleId);
                return Ok(workOrders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<WorkOrderDto>> CreateWorkOrder([FromBody] CreateWorkOrderDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var createdWorkOrder = await _workOrderService.CreateWorkOrderAsync(createDto);
                return CreatedAtAction(nameof(GetWorkOrderById), new { workOrderId = createdWorkOrder.Id }, createdWorkOrder);
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

        [HttpPut("{workOrderId}/assign-employees")]
        public async Task<ActionResult<WorkOrderDto>> AssignEmployees(int workOrderId, [FromBody] List<int> employeeIds)
        {
            try
            {
                var updatedWorkOrder = await _workOrderService.AssignEmployeesAsync(workOrderId, employeeIds);
                return Ok(updatedWorkOrder);
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

        [HttpPut("{workOrderId}/add-repair-tasks")]
        public async Task<ActionResult<WorkOrderDto>> AddRepairTasks(int workOrderId, [FromBody] List<int> taskIds)
        {
            try
            {
                var updatedWorkOrder = await _workOrderService.AddRepairTasksAsync(workOrderId, taskIds);
                return Ok(updatedWorkOrder);
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

        [HttpPut("{workOrderId}/add-parts")]
        public async Task<ActionResult<WorkOrderDto>> AddParts(int workOrderId, [FromBody] List<int> partIds)
        {
            try
            {
                var updatedWorkOrder = await _workOrderService.AddPartsAsync(workOrderId, partIds);
                return Ok(updatedWorkOrder);
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

        [HttpPut("{workOrderId}/state")]
        public async Task<ActionResult<WorkOrderDto>> ChangeState(int workOrderId, [FromBody] WorkOrderState newState)
        {
            try
            {
                var updatedWorkOrder = await _workOrderService.ChangeStateAsync(workOrderId, newState);
                return Ok(updatedWorkOrder);
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

        [HttpPut("{workOrderId}")]
        public async Task<ActionResult<WorkOrderDto>> UpdateWorkOrder(int workOrderId)
        {
            try
            {
                var updatedWorkOrder = await _workOrderService.UpdateWorkOrderAsync(workOrderId);
                return Ok(updatedWorkOrder);
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
        [HttpPut("{workOrderId}/complete")]
        public async Task<ActionResult<WorkOrderDto>> CompleteWorkOrder(int workOrderId, [FromHeader(Name = "X-Employee-Id")] int? employeeId = null)
        {
            try
            {
                var completedWorkOrder = await _workOrderService.CompleteWorkOrderAsync(workOrderId, employeeId);
                return Ok(completedWorkOrder);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{workOrderId}/start")]
        public async Task<ActionResult<WorkOrderDto>> StartWorkOrder(int workOrderId, [FromHeader(Name = "X-Employee-Id")] int employeeId)
        {
            if (employeeId <= 0)
                return BadRequest("Invalid or missing X-Employee-Id header.");

            try
            {
                var workOrder = await _workOrderService.StartWorkOrderAsync(workOrderId, employeeId);
                return Ok(workOrder);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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

        [HttpPut("{workOrderId}/cancel")]
        public async Task<ActionResult<WorkOrderDto>> CancelWorkOrder(int workOrderId, [FromHeader(Name = "X-Employee-Id")] int employeeId)
        {
            if (employeeId <= 0)
                return BadRequest("Invalid or missing X-Employee-Id header.");

            try
            {
                var workOrder = await _workOrderService.CancelWorkOrderAsync(workOrderId, employeeId);
                return Ok(workOrder);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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

        [HttpPut("{workOrderId}/update-hours")]
        public async Task<ActionResult<WorkOrderDto>> UpdateHours(int workOrderId, [FromHeader(Name = "X-Employee-Id")] int employeeId, [FromBody] UpdateHoursDto updateDto)
        {
            if (employeeId <= 0)
                return BadRequest("Invalid or missing X-Employee-Id header.");

            if (updateDto == null || updateDto.HoursWorked < 0)
                return BadRequest("Invalid hours value.");

            try
            {
                var workOrder = await _workOrderService.UpdateHoursAsync(workOrderId, employeeId, updateDto.HoursWorked);
                return Ok(workOrder);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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

        [HttpPut("{workOrderId}/update-part-usage")]
        public async Task<ActionResult<WorkOrderDto>> UpdatePartUsage(int workOrderId, [FromHeader(Name = "X-Employee-Id")] int employeeId, [FromBody] List<PartUsageUpdateDto> partUsages)
        {
            if (employeeId <= 0)
                return BadRequest("Invalid or missing X-Employee-Id header.");

            if (partUsages == null || !partUsages.Any())
                return BadRequest("Part usages cannot be null or empty.");

            try
            {
                var workOrder = await _workOrderService.UpdatePartUsageAsync(workOrderId, employeeId, partUsages);
                return Ok(workOrder);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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

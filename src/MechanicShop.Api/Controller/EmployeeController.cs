using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MechanicShop.Application.DTO.WorkOrder;
using MechanicShop.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MechanicShop.Api.Controller
{
    [ApiController]
    [Route("api/employees")]
    public class EmployeeController : ControllerBase
    {
        private readonly IWorkOrderService _workOrderService;

        public EmployeeController(IWorkOrderService workOrderService)
        {
            _workOrderService = workOrderService;
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

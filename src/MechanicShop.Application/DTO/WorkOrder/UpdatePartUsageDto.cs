using System;

namespace MechanicShop.Application.DTO.WorkOrder
{
    public class PartUsageUpdateDto
    {
        public int PartId { get; set; }
        public decimal QuantityUsed { get; set; }
    }

    public class UpdatePartUsageDto
    {
        public List<PartUsageUpdateDto> PartUsages { get; set; } = new();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MechanicShop.Application.DTO.Invoice;

namespace MechanicShop.Application.Interfaces
{
    public interface IInvoiceService
    {
        Task<(IEnumerable<InvoiceDto> Items, int TotalCount)> GetAllInvoicesAsync(int pageNumber, int pageSize, string? search);
        Task<InvoiceDto> GetInvoiceByIdAsync(int invoiceId);
    }
}

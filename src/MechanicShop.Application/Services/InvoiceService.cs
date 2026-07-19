using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MechanicShop.Application.DTO.Invoice;
using MechanicShop.Application.Interfaces;
using MechanicShop.Domain.Entities;
using MechanicShop.Domain.Interfaces;
using System.Linq;

namespace MechanicShop.Application.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceService(IUnitOfWork unitOfWork, IInvoiceRepository invoiceRepository)
        {
            _unitOfWork = unitOfWork;
            _invoiceRepository = invoiceRepository;
        }

        private static InvoiceDto MapToDto(Invoice invoice) => new()
        {
            Id = invoice.Id,
            WorkOrderId = invoice.WorkOrderId,
            Subtotal = invoice.Subtotal,
            Discount = invoice.Discount,
            TaxRate = invoice.TaxRate,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            PaymentStatus = invoice.PaymentStatus.ToString(),
            IssuedAt = invoice.IssuedAt,
            CreatedAt = invoice.CreatedAt
        };

        public async Task<(IEnumerable<InvoiceDto> Items, int TotalCount)> GetAllInvoicesAsync(int pageNumber, int pageSize, string? search)
        {
            var (items, totalCount) = await _unitOfWork.Invoices.GetPagedInvoicesAsync(pageNumber, pageSize, search);
            var dtos = items.Select(MapToDto).ToList();
            return (dtos, totalCount);
        }

        public async Task<InvoiceDto> GetInvoiceByIdAsync(int invoiceId)
        {
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);

            if (invoice == null || invoice.IsDeleted)
                throw new KeyNotFoundException($"Invoice with ID {invoiceId} not found.");

            return MapToDto(invoice);
        }

        public async Task<InvoiceDto> PayInvoiceAsync(int invoiceId)
        {
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);
            if (invoice == null || invoice.IsDeleted)
                throw new KeyNotFoundException($"Invoice with ID {invoiceId} not found.");

            if (invoice.PaymentStatus == Domain.Enums.PaymentStatus.Paid)
                throw new InvalidOperationException("Invoice is already paid.");

            invoice.PaymentStatus = Domain.Enums.PaymentStatus.Paid;

            await _unitOfWork.Invoices.UpdateAsync(invoice);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(invoice);
        }
    }
}

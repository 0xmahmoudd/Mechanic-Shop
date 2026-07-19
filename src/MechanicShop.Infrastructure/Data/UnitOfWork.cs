using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MechanicShop.Domain.Entities;
using MechanicShop.Domain.Interfaces;
using static MechanicShop.Domain.Interfaces.IGenericRepository;
using MechanicShop.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace MechanicShop.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MechanicShopDbContext _context;
        private IDbContextTransaction? _transaction;
        private IGenericRepository<Part> _parts;
        private IGenericRepository<RepairTask> _repairTasks;
        private IWorkOrderRepository _workOrder;
        private IInvoiceRepository _invoice;
        private IGenericRepository<User> _users;
        private ICustomerRepository _customers;
        private IVehicleRepository _vehicles;
        private IEmployeeRepository _employees;


        public UnitOfWork(MechanicShopDbContext context)
        {
            _context = context;
        }


        public IGenericRepository<Part> Parts
        {
            get
            {
                if (_parts == null)
                {
                    _parts = new GenericRepository<Part>(_context);
                }
                return _parts;
            }
        }

        public IGenericRepository<RepairTask> RepairTasks
        {
            get
            {
                if (_repairTasks == null)
                {
                    _repairTasks = new GenericRepository<RepairTask>(_context);
                }
                return _repairTasks;
            }
        }

        public IWorkOrderRepository WorkOrders
        {
            get
            {
                if (_workOrder == null)
                {
                    _workOrder = new WorkOrderRepository(_context);
                }
                return _workOrder;
            }
        }

        public IInvoiceRepository Invoices
        {
            get
            {
                if (_invoice == null)
                {
                    _invoice = new InvoiceRepository(_context);
                }
                return _invoice;
            }
        }

        public IGenericRepository<User> Users
        {
            get
            {
                if (_users == null)
                {
                    _users = new GenericRepository<User>(_context);
                }
                return _users;
            }
        }

        public ICustomerRepository Customers
        {
            get
            {
                if (_customers == null)
                {
                    _customers = new CustomerRepository(_context);
                }
                return _customers;
            }
        }

        public IVehicleRepository Vehicles
        {
            get
            {
                if (_vehicles == null)
                {
                    _vehicles = new VehicleRepository(_context);
                }
                return _vehicles;
            }
        }

        public IEmployeeRepository Employees
        {
            get
            {
                if (_employees == null)
                {
                    _employees = new EmployeeRepository(_context);
                }
                return _employees;
            }
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
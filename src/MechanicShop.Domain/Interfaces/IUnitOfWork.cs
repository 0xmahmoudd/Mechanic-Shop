using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MechanicShop.Domain.Entities;
using static MechanicShop.Domain.Interfaces.IGenericRepository;

namespace MechanicShop.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Part> Parts { get; }
        IGenericRepository<RepairTask>  RepairTasks { get; }
        IGenericRepository<WorkOrder>  WorkOrders { get; }
        IGenericRepository<Invoice>  Invoices { get; }
        IGenericRepository<User> Users { get; }
        IEmployeeRepository Employees { get; }
        IGenericRepository<Customer> Customers { get; }
        IGenericRepository<Vehicle> Vehicles { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
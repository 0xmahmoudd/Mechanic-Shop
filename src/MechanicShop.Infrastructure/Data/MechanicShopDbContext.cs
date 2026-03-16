using System;
using System.Collections.Generic;
using MechanicShop.Domain.Entities;
using MechanicShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Infrastructure.Data;

public partial class MechanicShopDbContext : DbContext
{
    public MechanicShopDbContext(DbContextOptions<MechanicShopDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeSalaryHistory> EmployeeSalaryHistories { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Part> Parts { get; set; }

    public virtual DbSet<PartPriceHistory> PartPriceHistories { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<RepairTask> RepairTasks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    public virtual DbSet<WorkOrder> WorkOrders { get; set; }

    public virtual DbSet<WorkOrderEmployee> WorkOrderEmployees { get; set; }

    public virtual DbSet<WorkOrderPart> WorkOrderParts { get; set; }

    public virtual DbSet<WorkOrderRepairTask> WorkOrderRepairTasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum<PaymentStatus>(name: "payment_status", nameTranslator: new Npgsql.NameTranslation.NpgsqlNullNameTranslator())
            .HasPostgresEnum<UserRole>(name: "user_role", nameTranslator: new Npgsql.NameTranslation.NpgsqlNullNameTranslator())
            .HasPostgresEnum<WorkOrderState>(name: "workorder_state", nameTranslator: new Npgsql.NameTranslation.NpgsqlNullNameTranslator());

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("customer_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.User).WithOne(p => p.Customer)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("customer_user_id_fkey");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("employee_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.WorkHoursPerDay).HasDefaultValue(8);

            entity.HasOne(d => d.User).WithOne(p => p.Employee)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("employee_user_id_fkey");
        });

        modelBuilder.Entity<EmployeeSalaryHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("employee_salary_history_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeSalaryHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("employee_salary_history_employee_id_fkey");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("invoice_pkey");

            entity.Property(e => e.PaymentStatus);


            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.WorkOrder).WithOne(p => p.Invoice)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("invoice_work_order_id_fkey");
        });

        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("part_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.StockQuantity).HasDefaultValue(0);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<PartPriceHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("part_price_history_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Part).WithMany(p => p.PartPriceHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("part_price_history_part_id_fkey");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("refresh_tokens_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens).HasConstraintName("refresh_tokens_user_id_fkey");
        });

        modelBuilder.Entity<RepairTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("repair_task_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasMany(d => d.Parts).WithMany(p => p.RepairTasks)
                .UsingEntity<Dictionary<string, object>>(
                    "RepairTaskPart",
                    r => r.HasOne<Part>().WithMany()
                        .HasForeignKey("PartId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("repair_task_part_part_id_fkey"),
                    l => l.HasOne<RepairTask>().WithMany()
                        .HasForeignKey("RepairTaskId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("repair_task_part_repair_task_id_fkey"),
                    j =>
                    {
                        j.HasKey("RepairTaskId", "PartId").HasName("repair_task_part_pkey");
                        j.ToTable("repair_task_part");
                        j.IndexerProperty<int>("RepairTaskId").HasColumnName("repair_task_id");
                        j.IndexerProperty<int>("PartId").HasColumnName("part_id");
                    });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_pkey");

            entity.Property(e => e.Role);


            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("vehicle_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Customer).WithMany(p => p.Vehicles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("vehicle_customer_id_fkey");
        });

        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("work_order_pkey");

            entity.Property(e => e.State);


            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.WorkOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("work_order_vehicle_id_fkey");
        });

        modelBuilder.Entity<WorkOrderEmployee>(entity =>
        {
            entity.HasKey(e => new { e.WorkOrderId, e.EmployeeId }).HasName("work_order_employee_pkey");

            entity.HasOne(d => d.Employee).WithMany(p => p.WorkOrderEmployees)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("work_order_employee_employee_id_fkey");

            entity.HasOne(d => d.WorkOrder).WithMany(p => p.WorkOrderEmployees)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("work_order_employee_work_order_id_fkey");
        });

        modelBuilder.Entity<WorkOrderPart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("work_order_part_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Part).WithMany(p => p.WorkOrderParts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("work_order_part_part_id_fkey");

            entity.HasOne(d => d.WorkOrder).WithMany(p => p.WorkOrderParts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("work_order_part_work_order_id_fkey");
        });

        modelBuilder.Entity<WorkOrderRepairTask>(entity =>
        {
            entity.HasKey(e => new { e.WorkOrderId, e.RepairTaskId }).HasName("work_order_repair_task_pkey");

            entity.Property(e => e.Quantity).HasDefaultValueSql("1");

            entity.HasOne(d => d.RepairTask).WithMany(p => p.WorkOrderRepairTasks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("work_order_repair_task_repair_task_id_fkey");

            entity.HasOne(d => d.WorkOrder).WithMany(p => p.WorkOrderRepairTasks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("work_order_repair_task_work_order_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

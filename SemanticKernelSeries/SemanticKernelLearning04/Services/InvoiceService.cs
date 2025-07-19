using Microsoft.EntityFrameworkCore;
using SemanticKernelLearning04.Data;
using SemanticKernelLearning04.Models;

namespace SemanticKernelLearning04.Services;

public class InvoiceService(SkInvoiceDbContext context)
{
    public async Task<Invoice?> GetInvoiceByNumberAsync(string invoiceNumber)
    {
        return await context.Invoices
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public async Task<List<Invoice>> GetUnpaidInvoicesAsync()
    {
        return await context.Invoices
            .Include(i => i.Customer)
            .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled)
            .OrderBy(i => i.DueDate)
            .ToListAsync();
    }

    public async Task<List<Invoice>> GetOverdueInvoicesAsync()
    {
        return await context.Invoices
            .Include(i => i.Customer)
            .Where(i => i.Status != InvoiceStatus.Paid &&
                       i.Status != InvoiceStatus.Cancelled &&
                       i.DueDate < DateTime.UtcNow)
            .OrderBy(i => i.DueDate)
            .ToListAsync();
    }

    public async Task<List<Invoice>> GetInvoicesByCustomerAsync(int customerId)
    {
        return await context.Invoices
            .Include(i => i.Customer)
            .Where(i => i.CustomerId == customerId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<Invoice> CreateInvoiceAsync(int customerId, string description, decimal amount, DateTime dueDate, string? notes = null)
    {
        var customer = await context.Customers.FindAsync(customerId);
        if (customer == null)
        {
            throw new ArgumentException($"Customer with ID {customerId} not found.");
        }

        // Generate invoice number
        var invoiceCount = await context.Invoices.CountAsync() + 1;
        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMM}-{invoiceCount:D4}";

        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            CustomerId = customerId,
            Description = description,
            Amount = amount,
            DueDate = dueDate,
            Notes = notes,
            Status = InvoiceStatus.Draft
        };

        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();

        return await GetInvoiceByNumberAsync(invoiceNumber) ?? invoice;
    }

    public async Task<bool> MarkInvoiceAsPaidAsync(string invoiceNumber)
    {
        var invoice = await context.Invoices.FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
        if (invoice == null)
        {
            return false;
        }

        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidDate = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<Customer> CreateCustomerAsync(string name, string email, string? phone = null, string? address = null)
    {
        var customer = new Customer
        {
            Name = name,
            Email = email,
            Phone = phone,
            Address = address
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        return customer;
    }

    public async Task<Customer?> GetCustomerByEmailAsync(string email)
    {
        return await context.Customers.FirstOrDefaultAsync(c => c.Email == email);
    }

    public async Task SeedSampleDataAsync()
    {
        // Only seed if no customers exist
        if (await context.Customers.AnyAsync())
        {
            return;
        }

        // Create sample customers
        var customers = new[]
        {
            new Customer { Name = "Juan Pérez", Email = "juan.perez@email.com", Phone = "555-0001", Address = "Calle Principal 123" },
            new Customer { Name = "María García", Email = "maria.garcia@email.com", Phone = "555-0002", Address = "Av. Central 456" },
            new Customer { Name = "Carlos López", Email = "carlos.lopez@email.com", Phone = "555-0003", Address = "Plaza Mayor 789" },
            new Customer { Name = "Ana Rodríguez", Email = "ana.rodriguez@email.com", Phone = "555-0004", Address = "Calle Nueva 321" }
        };

        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();

        // Create sample invoices
        var invoices = new[]
        {
            new Invoice
            {
                InvoiceNumber = "INV-202412-0001",
                CustomerId = 1,
                Description = "Servicios legales - Compraventa",
                Amount = 1500.00m,
                Status = InvoiceStatus.Paid,
                IssueDate = DateTime.UtcNow.AddDays(-30),
                DueDate = DateTime.UtcNow.AddDays(-15),
                PaidDate = DateTime.UtcNow.AddDays(-10),
                Notes = "Escritura de compraventa de inmueble"
            },
            new Invoice
            {
                InvoiceNumber = "INV-202412-0002",
                CustomerId = 2,
                Description = "Constitución de sociedad",
                Amount = 2500.00m,
                Status = InvoiceStatus.Sent,
                IssueDate = DateTime.UtcNow.AddDays(-15),
                DueDate = DateTime.UtcNow.AddDays(15),
                Notes = "Constitución de S.A. de C.V."
            },
            new Invoice
            {
                InvoiceNumber = "INV-202412-0003",
                CustomerId = 3,
                Description = "Testamento público abierto",
                Amount = 800.00m,
                Status = InvoiceStatus.Overdue,
                IssueDate = DateTime.UtcNow.AddDays(-45),
                DueDate = DateTime.UtcNow.AddDays(-15),
                Notes = "Elaboración de testamento"
            },
            new Invoice
            {
                InvoiceNumber = "INV-202412-0004",
                CustomerId = 4,
                Description = "Poder notarial",
                Amount = 350.00m,
                Status = InvoiceStatus.Draft,
                IssueDate = DateTime.UtcNow.AddDays(-5),
                DueDate = DateTime.UtcNow.AddDays(25),
                Notes = "Poder para actos de administración"
            },
            new Invoice
            {
                InvoiceNumber = "INV-202412-0005",
                CustomerId = 1,
                Description = "Cancelación de hipoteca",
                Amount = 1200.00m,
                Status = InvoiceStatus.Overdue,
                IssueDate = DateTime.UtcNow.AddDays(-60),
                DueDate = DateTime.UtcNow.AddDays(-30),
                Notes = "Cancelación de gravamen hipotecario"
            }
        };

        context.Invoices.AddRange(invoices);
        await context.SaveChangesAsync();
    }
}
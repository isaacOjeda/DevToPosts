using Microsoft.SemanticKernel;
using SemanticKernelLearning04.Models;
using SemanticKernelLearning04.Services;
using System.ComponentModel;
using System.Text;

namespace SemanticKernelLearning04.Plugins;

public class InvoicesPlugin
{
    private readonly InvoiceService _invoiceService;

    public InvoicesPlugin(InvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [KernelFunction]
    [Description("Verifica el estado de pago de una factura específica usando su número de factura.")]
    public async Task<string> VerifyPaymentAsync([Description("Número de la factura a verificar (ej: INV-202412-0001)")] string numeroFactura)
    {
        try
        {
            var invoice = await _invoiceService.GetInvoiceByNumberAsync(numeroFactura);
            
            if (invoice == null)
            {
                return $"❌ No se encontró ninguna factura con el número: {numeroFactura}";
            }

            var statusText = GetStatusText(invoice.Status);
            var result = new StringBuilder();
            
            result.AppendLine($"📋 **Factura: {invoice.InvoiceNumber}**");
            result.AppendLine($"👤 Cliente: {invoice.Customer.Name}");
            result.AppendLine($"📄 Descripción: {invoice.Description}");
            result.AppendLine($"💰 Monto: ${invoice.Amount:F2}");
            result.AppendLine($"📅 Fecha de emisión: {invoice.IssueDate:dd/MM/yyyy}");
            result.AppendLine($"📅 Fecha de vencimiento: {invoice.DueDate:dd/MM/yyyy}");
            result.AppendLine($"🔸 Estado: {statusText}");

            if (invoice.Status == InvoiceStatus.Paid && invoice.PaidDate.HasValue)
            {
                result.AppendLine($"💳 Fecha de pago: {invoice.PaidDate.Value:dd/MM/yyyy}");
            }
            else if (invoice.IsOverdue)
            {
                result.AppendLine($"⚠️ Días vencida: {invoice.DaysOverdue} días");
            }

            if (!string.IsNullOrEmpty(invoice.Notes))
            {
                result.AppendLine($"📝 Notas: {invoice.Notes}");
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"❌ Error al verificar la factura: {ex.Message}";
        }
    }

    [KernelFunction]
    [Description("Realiza una prefactura (borrador) para un cliente específico. Requiere email del cliente, descripción del servicio, monto y días para vencimiento.")]
    public async Task<string> CreateInvoiceDraftAsync(
        [Description("Email del cliente para la prefactura")] string clienteEmail,
        [Description("Descripción del servicio o trabajo realizado")] string descripcion,
        [Description("Monto de la factura en pesos")] decimal monto,
        [Description("Días hasta el vencimiento (opcional, por defecto 30 días)")] int diasVencimiento = 30)
    {
        try
        {
            var customer = await _invoiceService.GetCustomerByEmailAsync(clienteEmail);
            
            if (customer == null)
            {
                return $"❌ No se encontró ningún cliente con el email: {clienteEmail}";
            }

            var dueDate = DateTime.UtcNow.AddDays(diasVencimiento);
            var invoice = await _invoiceService.CreateInvoiceAsync(
                customer.Id, 
                descripcion, 
                monto, 
                dueDate,
                "Factura generada automáticamente");

            var result = new StringBuilder();
            result.AppendLine($"✅ **Prefactura creada exitosamente**");
            result.AppendLine($"📋 Número: {invoice.InvoiceNumber}");
            result.AppendLine($"👤 Cliente: {customer.Name} ({customer.Email})");
            result.AppendLine($"📄 Descripción: {descripcion}");
            result.AppendLine($"💰 Monto: ${monto:F2}");
            result.AppendLine($"📅 Fecha de vencimiento: {dueDate:dd/MM/yyyy}");
            result.AppendLine($"🔸 Estado: Borrador (Lista para enviar)");

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"❌ Error al crear la prefactura: {ex.Message}";
        }
    }

    [KernelFunction]
    [Description("Obtiene la lista de todas las facturas sin pagar (pendientes y vencidas).")]
    public async Task<string> GetUnpaidInvoicesAsync()
    {
        try
        {
            var unpaidInvoices = await _invoiceService.GetUnpaidInvoicesAsync();
            
            if (!unpaidInvoices.Any())
            {
                return "✅ ¡Excelente! No hay facturas pendientes de pago.";
            }

            var result = new StringBuilder();
            result.AppendLine($"📊 **Facturas Pendientes de Pago ({unpaidInvoices.Count})**");
            result.AppendLine();

            var totalAmount = 0m;
            var overdueCount = 0;

            foreach (var invoice in unpaidInvoices)
            {
                var statusIcon = invoice.IsOverdue ? "🔴" : "🟡";
                var statusText = invoice.IsOverdue ? $"VENCIDA ({invoice.DaysOverdue} días)" : GetStatusText(invoice.Status);
                
                result.AppendLine($"{statusIcon} **{invoice.InvoiceNumber}**");
                result.AppendLine($"   👤 {invoice.Customer.Name}");
                result.AppendLine($"   📄 {invoice.Description}");
                result.AppendLine($"   💰 ${invoice.Amount:F2}");
                result.AppendLine($"   📅 Vence: {invoice.DueDate:dd/MM/yyyy}");
                result.AppendLine($"   🔸 {statusText}");
                result.AppendLine();

                totalAmount += invoice.Amount;
                if (invoice.IsOverdue) overdueCount++;
            }

            result.AppendLine($"💰 **Total pendiente: ${totalAmount:F2}**");
            if (overdueCount > 0)
            {
                result.AppendLine($"⚠️ **Facturas vencidas: {overdueCount}**");
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"❌ Error al obtener facturas pendientes: {ex.Message}";
        }
    }

    [KernelFunction]
    [Description("Marca una factura como pagada usando su número de factura.")]
    public async Task<string> MarkInvoiceAsPaidAsync([Description("Número de la factura a marcar como pagada")] string numeroFactura)
    {
        try
        {
            var invoice = await _invoiceService.GetInvoiceByNumberAsync(numeroFactura);
            
            if (invoice == null)
            {
                return $"❌ No se encontró ninguna factura con el número: {numeroFactura}";
            }

            if (invoice.Status == InvoiceStatus.Paid)
            {
                return $"ℹ️ La factura {numeroFactura} ya estaba marcada como pagada desde el {invoice.PaidDate:dd/MM/yyyy}.";
            }

            var success = await _invoiceService.MarkInvoiceAsPaidAsync(numeroFactura);
            
            if (success)
            {
                return $"✅ Factura {numeroFactura} marcada como PAGADA exitosamente.\n" +
                       $"👤 Cliente: {invoice.Customer.Name}\n" +
                       $"💰 Monto: ${invoice.Amount:F2}\n" +
                       $"📅 Fecha de pago: {DateTime.UtcNow:dd/MM/yyyy}";
            }
            else
            {
                return $"❌ No se pudo marcar la factura {numeroFactura} como pagada.";
            }
        }
        catch (Exception ex)
        {
            return $"❌ Error al marcar factura como pagada: {ex.Message}";
        }
    }

    [KernelFunction]
    [Description("Obtiene información detallada de un cliente específico y sus facturas usando su email.")]
    public async Task<string> GetCustomerInfoAsync([Description("Email del cliente a consultar")] string email)
    {
        try
        {
            var customer = await _invoiceService.GetCustomerByEmailAsync(email);
            
            if (customer == null)
            {
                return $"❌ No se encontró ningún cliente con el email: {email}";
            }

            var invoices = await _invoiceService.GetInvoicesByCustomerAsync(customer.Id);
            
            var result = new StringBuilder();
            result.AppendLine($"👤 **Información del Cliente**");
            result.AppendLine($"📛 Nombre: {customer.Name}");
            result.AppendLine($"📧 Email: {customer.Email}");
            result.AppendLine($"📞 Teléfono: {customer.Phone ?? "No especificado"}");
            result.AppendLine($"🏠 Dirección: {customer.Address ?? "No especificada"}");
            result.AppendLine($"📅 Cliente desde: {customer.CreatedAt:dd/MM/yyyy}");
            result.AppendLine();

            if (!invoices.Any())
            {
                result.AppendLine("📊 Este cliente no tiene facturas registradas.");
                return result.ToString();
            }

            result.AppendLine($"📊 **Facturas ({invoices.Count})**");
            
            var totalAmount = invoices.Sum(i => i.Amount);
            var paidAmount = invoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.Amount);
            var pendingAmount = totalAmount - paidAmount;

            foreach (var invoice in invoices.Take(5)) // Mostrar las últimas 5
            {
                var statusIcon = GetStatusIcon(invoice.Status);
                result.AppendLine($"{statusIcon} {invoice.InvoiceNumber} - {invoice.Description}");
                result.AppendLine($"   💰 ${invoice.Amount:F2} | 📅 {invoice.IssueDate:dd/MM/yyyy} | {GetStatusText(invoice.Status)}");
            }

            if (invoices.Count > 5)
            {
                result.AppendLine($"... y {invoices.Count - 5} facturas más");
            }

            result.AppendLine();
            result.AppendLine($"💰 **Total facturado: ${totalAmount:F2}**");
            result.AppendLine($"✅ **Pagado: ${paidAmount:F2}**");
            result.AppendLine($"⏳ **Pendiente: ${pendingAmount:F2}**");

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"❌ Error al obtener información del cliente: {ex.Message}";
        }
    }

    private static string GetStatusText(InvoiceStatus status)
    {
        return status switch
        {
            InvoiceStatus.Draft => "Borrador",
            InvoiceStatus.Sent => "Enviada",
            InvoiceStatus.Paid => "Pagada",
            InvoiceStatus.Overdue => "Vencida",
            InvoiceStatus.Cancelled => "Cancelada",
            _ => "Desconocido"
        };
    }

    private static string GetStatusIcon(InvoiceStatus status)
    {
        return status switch
        {
            InvoiceStatus.Draft => "📝",
            InvoiceStatus.Sent => "📤",
            InvoiceStatus.Paid => "✅",
            InvoiceStatus.Overdue => "🔴",
            InvoiceStatus.Cancelled => "❌",
            _ => "❓"
        };
    }
}

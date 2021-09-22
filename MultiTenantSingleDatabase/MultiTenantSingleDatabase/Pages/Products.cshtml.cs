namespace MultiTenantSingleDatabase.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MultiTenantSingleDatabase.Models;
using MultiTenantSingleDatabase.Persistence;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ProductsModel : PageModel
{
    private readonly MultiTenantDbContext _context;

    public ProductsModel(MultiTenantDbContext context)
    {
        _context = context;
    }

    public List<Product> Products { get; set; } = new();

    public async Task OnGet()
    {
        Products = await _context.Products.ToListAsync();
    }
}

namespace MultiTenantSingleDatabase.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MultiTenantSingleDatabase.Models;
using MultiTenantSingleDatabase.Persistence;


public class ProductsModel : PageModel
{
    private readonly MultiTenantDbContext _context;

    public ProductsModel(MultiTenantDbContext context)
    {
        _context = context;
    }

    public List<Product> Products { get; set; }

    public async Task OnGet()
    {
        Products = await _context.Products.ToListAsync();
    }
}

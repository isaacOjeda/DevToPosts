using Microsoft.AspNetCore.Mvc.RazorPages;
using MultiTenantSingleDatabase.Models;
using MultiTenantSingleDatabase.Persistence;

namespace MultiTenantMultiDatabase.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly SingleTenantDbContext _context;

        public IndexModel(SingleTenantDbContext context)
        {
            _context = context;
        }

        public ICollection<Product> Products { get; set; }

        public void OnGet()
        {
            Products = _context.Products.ToList();
        }
    }
}

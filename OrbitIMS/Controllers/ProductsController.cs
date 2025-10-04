using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OrbitIMS.Data;
using OrbitIMS.Helpers;

namespace OrbitIMS.Controllers
{
    public class ProductsController : Controller
    {
        private readonly OrbitDbContext _context;

        public ProductsController(OrbitDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Products.Include(p => p.Category).Include(p => p.Supplier);
            return View(await applicationDbContext.ToListAsync());
        }

        // AJAX method to get product data for editing
        [HttpGet]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (product == null)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Product not found"));
            }
            return Json(NotificationHelper.CreateNotificationResponse(true, "Product loaded successfully", product));
        }

        // AJAX method to get categories and suppliers for dropdowns
        [HttpGet]
        public async Task<IActionResult> GetDropdownData()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .Select(c => new { Id = c.Id, Name = c.Name })
                .ToListAsync();
            
            var suppliers = await _context.Suppliers
                .Where(s => s.IsActive)
                .Select(s => new { Id = s.Id, Name = s.Name })
                .ToListAsync();

            return Json(NotificationHelper.CreateNotificationResponse(true, "Dropdown data loaded", new { categories, suppliers }));
        }

        // AJAX method to create product        
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    product.CreatedAt = DateTime.Now;
                    product.CreatedBy = User.Identity.Name ?? "Default";
                    product.IsActive = true;
                    _context.Add(product);
                    await _context.SaveChangesAsync();

                    // Load related data for response
                    await _context.Entry(product)
                        .Reference(p => p.Category)
                        .LoadAsync();
                    await _context.Entry(product)
                        .Reference(p => p.Supplier)
                        .LoadAsync();

                    return Json(NotificationHelper.CreateNotificationResponse(true, "Product created successfully", product));
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(NotificationHelper.CreateNotificationResponse(false, string.Join(", ", errors)));
                }
            }
            catch (Exception ex)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Error creating product: " + ex.Message));
            }
        }

        // AJAX method to update product
        [HttpPost]
        public async Task<IActionResult> UpdateProduct([FromBody] Product product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingProduct = await _context.Products.FindAsync(product.Id);
                    if (existingProduct == null)
                    {
                        return Json(NotificationHelper.CreateNotificationResponse(false, "Product not found"));
                    }

                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.StockQuantity = product.StockQuantity;
                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.SupplierId = product.SupplierId;
                    existingProduct.UpdatedAt = DateTime.Now;
                    existingProduct.UpdatedBy = User.Identity.Name ?? "Default";

                    _context.Update(existingProduct);
                    await _context.SaveChangesAsync();

                    // Load related data for response
                    await _context.Entry(existingProduct)
                        .Reference(p => p.Category)
                        .LoadAsync();
                    await _context.Entry(existingProduct)
                        .Reference(p => p.Supplier)
                        .LoadAsync();

                    return Json(NotificationHelper.CreateNotificationResponse(true, "Product updated successfully", existingProduct));
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(NotificationHelper.CreateNotificationResponse(false, string.Join(", ", errors)));
                }
            }
            catch (Exception ex)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Error updating product: " + ex.Message));
            }
        }

        // AJAX method to delete product
        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return Json(NotificationHelper.CreateNotificationResponse(false, "Product not found"));
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return Json(NotificationHelper.CreateNotificationResponse(true, "Product deleted successfully"));
            }
            catch (Exception ex)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Error deleting product: " + ex.Message));
            }
        }

        
    }
}

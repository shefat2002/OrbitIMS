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

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Price,StockQuantity,CategoryId,SupplierId")] Product product)
        {
            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.Now;
                product.CreatedBy = User.Identity.Name ?? "Default";
                product.IsActive = true;
                _context.Add(product);
                await _context.SaveChangesAsync();
                this.SetSuccessMessage("Product created successfully!");
                return RedirectToAction(nameof(Index));
            }
            else
            {
                string msg = "";
                foreach (var err in ModelState.Values)
                {
                    foreach (var ms in err.Errors)
                    {
                        msg += $"{ms.ErrorMessage}\n";
                    }
                }
                this.SetErrorMessage(msg);
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", product.SupplierId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", product.SupplierId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Description,Price,StockQuantity,CategoryId,SupplierId,Id,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,IsActive")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    this.SetSuccessMessage("Product updated successfully!");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", product.SupplierId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                this.SetSuccessMessage("Product deleted successfully!");
            }
            else
            {
                this.SetErrorMessage("Product not found!");
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}

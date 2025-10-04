using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrbitIMS.Data;
using OrbitIMS.Helpers;

namespace OrbitIMS.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly OrbitDbContext _context;

        public SuppliersController(OrbitDbContext context)
        {
            _context = context;
        }

        // GET: Suppliers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Suppliers.ToListAsync());
        }

        // AJAX method to get supplier data for editing
        [HttpGet]
        public async Task<IActionResult> GetSupplier(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Supplier not found"));
            }
            return Json(NotificationHelper.CreateNotificationResponse(true, "Supplier loaded successfully", supplier));
        }

        // AJAX method to create supplier
        [HttpPost]
        public async Task<IActionResult> CreateSupplier([FromBody] Supplier supplier)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    supplier.CreatedAt = DateTime.Now;
                    supplier.CreatedBy = User.Identity.Name ?? "Default";
                    supplier.IsActive = true;
                    _context.Add(supplier);
                    await _context.SaveChangesAsync();
                    return Json(NotificationHelper.CreateNotificationResponse(true, "Supplier created successfully", supplier));
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(NotificationHelper.CreateNotificationResponse(false, string.Join(", ", errors)));
                }
            }
            catch (Exception ex)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Error creating supplier: " + ex.Message));
            }
        }

        // AJAX method to update supplier
        [HttpPost]
        public async Task<IActionResult> UpdateSupplier([FromBody] Supplier supplier)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingSupplier = await _context.Suppliers.FindAsync(supplier.Id);
                    if (existingSupplier == null)
                    {
                        return Json(NotificationHelper.CreateNotificationResponse(false, "Supplier not found"));
                    }

                    existingSupplier.Name = supplier.Name;
                    existingSupplier.Email = supplier.Email;
                    existingSupplier.Mobile = supplier.Mobile;
                    existingSupplier.Address = supplier.Address;
                    existingSupplier.UpdatedAt = DateTime.Now;
                    existingSupplier.UpdatedBy = User.Identity.Name ?? "Default";

                    _context.Update(existingSupplier);
                    await _context.SaveChangesAsync();
                    return Json(NotificationHelper.CreateNotificationResponse(true, "Supplier updated successfully", existingSupplier));
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(NotificationHelper.CreateNotificationResponse(false, string.Join(", ", errors)));
                }
            }
            catch (Exception ex)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Error updating supplier: " + ex.Message));
            }
        }

        // AJAX method to delete supplier
        [HttpPost]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            try
            {
                var supplier = await _context.Suppliers.FindAsync(id);
                if (supplier == null)
                {
                    return Json(NotificationHelper.CreateNotificationResponse(false, "Supplier not found"));
                }

                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();
                return Json(NotificationHelper.CreateNotificationResponse(true, "Supplier deleted successfully"));
            }
            catch (Exception ex)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Error deleting supplier: " + ex.Message));
            }
        }

    }
}

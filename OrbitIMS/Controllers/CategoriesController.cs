using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrbitIMS.Data;
using OrbitIMS.Helpers;

namespace OrbitIMS.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly OrbitDbContext _context;
        public CategoriesController(OrbitDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        // AJAX method to get category data for editing
        [HttpGet]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Category not found"));
            }
            return Json(NotificationHelper.CreateNotificationResponse(true, "Category loaded successfully", category));
        }

        // AJAX method to create category
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    category.CreatedAt = DateTime.Now;
                    category.CreatedBy = User.Identity.Name ?? "Default";
                    category.IsActive = true;
                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    return Json(NotificationHelper.CreateNotificationResponse(true, "Category created successfully", category));
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(NotificationHelper.CreateNotificationResponse(false, string.Join(", ", errors)));
                }
            }
            catch (Exception ex)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Error creating category: " + ex.Message));
            }
        }

        // AJAX method to update category
        [HttpPost]
        public async Task<IActionResult> UpdateCategory([FromBody] Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingCategory = await _context.Categories.FindAsync(category.Id);
                    if (existingCategory == null)
                    {
                        return Json(NotificationHelper.CreateNotificationResponse(false, "Category not found"));
                    }

                    existingCategory.Name = category.Name;
                    existingCategory.Description = category.Description;
                    existingCategory.UpdatedAt = DateTime.Now;
                    existingCategory.UpdatedBy = User.Identity.Name ?? "Default";

                    _context.Update(existingCategory);
                    await _context.SaveChangesAsync();
                    return Json(NotificationHelper.CreateNotificationResponse(true, "Category updated successfully", existingCategory));
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(NotificationHelper.CreateNotificationResponse(false, string.Join(", ", errors)));
                }
            }
            catch (Exception ex)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Error updating category: " + ex.Message));
            }
        }

        // AJAX method to delete category
        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return Json(NotificationHelper.CreateNotificationResponse(false, "Category not found"));
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return Json(NotificationHelper.CreateNotificationResponse(true, "Category deleted successfully"));
            }
            catch (Exception ex)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Error deleting category: " + ex.Message));
            }
        }

        
    }
}

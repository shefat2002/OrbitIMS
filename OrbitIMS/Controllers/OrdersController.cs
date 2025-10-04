using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OrbitIMS.Data;
using OrbitIMS.Helpers;

namespace OrbitIMS.Controllers
{
    public class OrdersController : Controller
    {
        private readonly OrbitDbContext _context;

        public OrdersController(OrbitDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Orders.Include(o => o.Customer).Include(o => o.OrderDetails).ThenInclude(o => o.Product);
            return View(await applicationDbContext.ToListAsync());
        }

        // AJAX method to get order data for editing
        [HttpGet]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
            
            if (order == null)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Order not found"));
            }
            return Json(NotificationHelper.CreateNotificationResponse(true, "Order loaded successfully", order));
        }

        // AJAX method to get customers for dropdown
        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _context.Customers
                .Where(c => c.IsActive)
                .Select(c => new { Id = c.Id, Name = c.Name })
                .ToListAsync();

            return Json(NotificationHelper.CreateNotificationResponse(true, "Customers loaded", customers));
        }

        // AJAX method to create order
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    order.CreatedAt = DateTime.Now;
                    order.CreatedBy = User.Identity.Name ?? "Default";
                    order.IsActive = true;
                    
                    // Calculate total amount
                    order.TotalAmount = order.OrderDetails?.Sum(od => od.TotalPrice) ?? order.TotalAmount;
                    
                    _context.Add(order);
                    await _context.SaveChangesAsync();

                    // Load related data for response
                    await _context.Entry(order)
                        .Reference(o => o.Customer)
                        .LoadAsync();

                    return Json(NotificationHelper.CreateNotificationResponse(true, "Order created successfully", order));
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(NotificationHelper.CreateNotificationResponse(false, string.Join(", ", errors)));
                }
            }
            catch (Exception ex)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Error creating order: " + ex.Message));
            }
        }

        // AJAX method to update order
        [HttpPost]
        public async Task<IActionResult> UpdateOrder([FromBody] Order order)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingOrder = await _context.Orders.FindAsync(order.Id);
                    if (existingOrder == null)
                    {
                        return Json(NotificationHelper.CreateNotificationResponse(false, "Order not found"));
                    }

                    existingOrder.OrderDate = order.OrderDate;
                    existingOrder.Status = order.Status;
                    existingOrder.CustomerId = order.CustomerId;
                    existingOrder.TotalAmount = order.TotalAmount;
                    existingOrder.UpdatedAt = DateTime.Now;
                    existingOrder.UpdatedBy = User.Identity.Name ?? "Default";

                    _context.Update(existingOrder);
                    await _context.SaveChangesAsync();

                    // Load related data for response
                    await _context.Entry(existingOrder)
                        .Reference(o => o.Customer)
                        .LoadAsync();

                    return Json(NotificationHelper.CreateNotificationResponse(true, "Order updated successfully", existingOrder));
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(NotificationHelper.CreateNotificationResponse(false, string.Join(", ", errors)));
                }
            }
            catch (Exception ex)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Error updating order: " + ex.Message));
            }
        }

        // AJAX method to delete order - Fixed to accept int parameter directly
        [HttpPost]
        public async Task<IActionResult> DeleteOrder([FromBody] int id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    return Json(NotificationHelper.CreateNotificationResponse(false, "Order not found"));
                }

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                return Json(NotificationHelper.CreateNotificationResponse(true, "Order deleted successfully"));
            }
            catch (Exception ex)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Error deleting order: " + ex.Message));
            }
        }

        
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrbitIMS.Data;
using OrbitIMS.Helpers;

namespace OrbitIMS.Controllers
{
    public class OrderDetailsController : Controller
    {
        private readonly OrbitDbContext _context;

        public OrderDetailsController(OrbitDbContext context)
        {
            _context = context;
        }

        // GET: OrderDetails
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.OrderDetails.Include(o => o.Order).Include(o => o.Product);
            return View(await applicationDbContext.ToListAsync());
        }

        // AJAX method to get order detail data for editing
        [HttpGet]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            var orderDetail = await _context.OrderDetails
                .Include(od => od.Order)
                .ThenInclude(o => o.Customer)
                .Include(od => od.Product)
                .FirstOrDefaultAsync(od => od.Id == id);
            
            if (orderDetail == null)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Order detail not found"));
            }
            return Json(NotificationHelper.CreateNotificationResponse(true, "Order detail loaded successfully", orderDetail));
        }

        // AJAX method to get dropdown data (orders and products)
        [HttpGet]
        public async Task<IActionResult> GetDropdownData()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Where(o => o.IsActive)
                .Select(o => new { Id = o.Id, DisplayText = $"Order #{o.Id} - {o.Customer!.Name}" })
                .ToListAsync();
            
            var products = await _context.Products
                .Where(p => p.IsActive)
                .Select(p => new { Id = p.Id, Name = p.Name, Price = p.Price })
                .ToListAsync();

            return Json(NotificationHelper.CreateNotificationResponse(true, "Dropdown data loaded", new { orders, products }));
        }

        // AJAX method to create order detail
        [HttpPost]
        public async Task<IActionResult> CreateOrderDetail([FromBody] OrderDetails orderDetail)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    orderDetail.CreatedAt = DateTime.Now;
                    orderDetail.CreatedBy = User.Identity.Name ?? "Default";
                    orderDetail.IsActive = true;
                    
                    // Calculate total price
                    orderDetail.TotalPrice = orderDetail.Quantity * orderDetail.UnitPrice;
                    
                    _context.Add(orderDetail);
                    await _context.SaveChangesAsync();

                    // Load related data for response
                    await _context.Entry(orderDetail)
                        .Reference(od => od.Order)
                        .LoadAsync();
                    await _context.Entry(orderDetail.Order!)
                        .Reference(o => o.Customer)
                        .LoadAsync();
                    await _context.Entry(orderDetail)
                        .Reference(od => od.Product)
                        .LoadAsync();

                    return Json(NotificationHelper.CreateNotificationResponse(true, "Order detail created successfully", orderDetail));
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(NotificationHelper.CreateNotificationResponse(false, string.Join(", ", errors)));
                }
            }
            catch (Exception ex)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Error creating order detail: " + ex.Message));
            }
        }

        // AJAX method to update order detail
        [HttpPost]
        public async Task<IActionResult> UpdateOrderDetail([FromBody] OrderDetails orderDetail)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingOrderDetail = await _context.OrderDetails.FindAsync(orderDetail.Id);
                    if (existingOrderDetail == null)
                    {
                        return Json(NotificationHelper.CreateNotificationResponse(false, "Order detail not found"));
                    }

                    existingOrderDetail.OrderId = orderDetail.OrderId;
                    existingOrderDetail.ProductId = orderDetail.ProductId;
                    existingOrderDetail.Quantity = orderDetail.Quantity;
                    existingOrderDetail.UnitPrice = orderDetail.UnitPrice;
                    existingOrderDetail.TotalPrice = orderDetail.Quantity * orderDetail.UnitPrice;
                    existingOrderDetail.UpdatedAt = DateTime.Now;
                    existingOrderDetail.UpdatedBy = User.Identity.Name ?? "Default";

                    _context.Update(existingOrderDetail);
                    await _context.SaveChangesAsync();

                    // Load related data for response
                    await _context.Entry(existingOrderDetail)
                        .Reference(od => od.Order)
                        .LoadAsync();
                    await _context.Entry(existingOrderDetail.Order!)
                        .Reference(o => o.Customer)
                        .LoadAsync();
                    await _context.Entry(existingOrderDetail)
                        .Reference(od => od.Product)
                        .LoadAsync();

                    return Json(NotificationHelper.CreateNotificationResponse(true, "Order detail updated successfully", existingOrderDetail));
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(NotificationHelper.CreateNotificationResponse(false, string.Join(", ", errors)));
                }
            }
            catch (Exception ex)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Error updating order detail: " + ex.Message));
            }
        }

        // AJAX method to delete order detail - Fixed to accept int parameter directly
        [HttpPost]
        public async Task<IActionResult> DeleteOrderDetail([FromBody] int id)
        {
            try
            {
                var orderDetail = await _context.OrderDetails.FindAsync(id);
                if (orderDetail == null)
                {
                    return Json(NotificationHelper.CreateNotificationResponse(false, "Order detail not found"));
                }

                _context.OrderDetails.Remove(orderDetail);
                await _context.SaveChangesAsync();
                return Json(NotificationHelper.CreateNotificationResponse(true, "Order detail deleted successfully"));
            }
            catch (Exception ex)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Error deleting order detail: " + ex.Message));
            }
        }

    }
}

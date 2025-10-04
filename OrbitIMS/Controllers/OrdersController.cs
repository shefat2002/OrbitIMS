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

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name");
            var order = new Order();
            ViewBag.ProductId = new SelectList(_context.Products.OrderBy(p => p.Name), "Id", "Name");
            order.OrderDetails = new List<OrderDetails> {
                new OrderDetails() {ProductId=0,  Quantity=1, UnitPrice=0, TotalPrice=0  }
            };
            return View(order);
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    order.CreatedAt = DateTime.Now;
                    order.CreatedBy = User.Identity.Name ?? "Default";
                    order.IsActive = true;
                    _context.Add(order);
                    await _context.SaveChangesAsync();
                    this.SetSuccessMessage("Order created successfully!");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    this.SetErrorMessage(ex.Message);
                }
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
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", order.CustomerId);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = _context.Orders.Include(o => o.OrderDetails).Where(o => o.Id.Equals(id)).FirstOrDefault();
            if (order == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", order.CustomerId);
            ViewBag.ProductId = new SelectList(_context.Products.OrderBy(p => p.Name), "Id", "Name");
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Order order)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    order.UpdatedAt = DateTime.Now;
                    order.UpdatedBy = User.Identity.Name ?? "Default";
                    order.OrderDetails = order.OrderDetails;

                    var od = _context.OrderDetails.Where(o => o.OrderId.Equals(order.Id)).AsNoTracking();
                    //var rid= order.OrderDetails.Contains(od);
                    foreach (var item in od)
                    {
                        if (!order.OrderDetails.Any(o => o.Id == item.Id))
                        {
                            _context.OrderDetails.Remove(item);
                        }
                    }
                    _context.Update(order);

                    await _context.SaveChangesAsync();
                    this.SetSuccessMessage("Order updated successfully!");
                }
                catch (DbUpdateConcurrencyException e)
                {
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        this.SetErrorMessage(e.InnerException?.Message ?? e.Message);
                    }
                }
                catch (Exception ex)
                {
                    this.SetErrorMessage(ex.InnerException?.Message ?? ex.Message);
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", order.CustomerId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                this.SetSuccessMessage("Order deleted successfully!");
            }
            else
            {
                this.SetErrorMessage("Order not found!");
            }

            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}

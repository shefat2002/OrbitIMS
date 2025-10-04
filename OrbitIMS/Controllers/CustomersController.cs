using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrbitIMS.Data;
using OrbitIMS.Helpers;

namespace OrbitIMS.Controllers
{
    public class CustomersController : Controller
    {
        private readonly OrbitDbContext _context;

        public CustomersController(OrbitDbContext context)
        {
            _context = context;
        }

        // GET: Customers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Customers.ToListAsync());
        }

        // AJAX method to get customer data for editing
        [HttpGet]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Customer not found"));
            }
            return Json(NotificationHelper.CreateNotificationResponse(true, "Customer loaded successfully", customer));
        }

        // AJAX method to create customer
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    customer.CreatedAt = DateTime.Now;
                    customer.CreatedBy = User.Identity.Name ?? "Default";
                    customer.IsActive = true;
                    _context.Add(customer);
                    await _context.SaveChangesAsync();
                    return Json(NotificationHelper.CreateNotificationResponse(true, "Customer created successfully", customer));
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(NotificationHelper.CreateNotificationResponse(false, string.Join(", ", errors)));
                }
            }
            catch (Exception ex)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Error creating customer: " + ex.Message));
            }
        }

        // AJAX method to update customer
        [HttpPost]
        public async Task<IActionResult> UpdateCustomer([FromBody] Customer customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingCustomer = await _context.Customers.FindAsync(customer.Id);
                    if (existingCustomer == null)
                    {
                        return Json(NotificationHelper.CreateNotificationResponse(false, "Customer not found"));
                    }

                    existingCustomer.Name = customer.Name;
                    existingCustomer.Email = customer.Email;
                    existingCustomer.Mobile = customer.Mobile;
                    existingCustomer.Address = customer.Address;
                    existingCustomer.UpdatedAt = DateTime.Now;
                    existingCustomer.UpdatedBy = User.Identity.Name ?? "Default";

                    _context.Update(existingCustomer);
                    await _context.SaveChangesAsync();
                    return Json(NotificationHelper.CreateNotificationResponse(true, "Customer updated successfully", existingCustomer));
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(NotificationHelper.CreateNotificationResponse(false, string.Join(", ", errors)));
                }
            }
            catch (Exception ex)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Error updating customer: " + ex.Message));
            }
        }

        // AJAX method to delete customer
        [HttpPost]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    return Json(NotificationHelper.CreateNotificationResponse(false, "Customer not found"));
                }

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                return Json(NotificationHelper.CreateNotificationResponse(true, "Customer deleted successfully"));
            }
            catch (Exception ex)
            {
                return Json(NotificationHelper.CreateNotificationResponse(false, "Error deleting customer: " + ex.Message));
            }
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Email,Mobile,Address")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.CreatedAt = DateTime.Now;
                customer.CreatedBy = User.Identity.Name ?? "Default";
                customer.IsActive = true;
                _context.Add(customer);
                await _context.SaveChangesAsync();
                this.SetSuccessMessage("Customer created successfully!");
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
            return View(customer);
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Email,Mobile,Address,Id,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,IsActive")] Customer customer)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                    this.SetSuccessMessage("Customer updated successfully!");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.Id))
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
            return View(customer);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                this.SetSuccessMessage("Customer deleted successfully!");
            }
            else
            {
                this.SetErrorMessage("Customer not found!");
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }
    }
}

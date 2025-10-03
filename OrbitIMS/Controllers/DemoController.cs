using Microsoft.AspNetCore.Mvc;
using OrbitIMS.Helpers;

namespace OrbitIMS.Controllers
{
    public class DemoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult TestSuccess()
        {
            this.SetSuccessMessage("This is a success notification! Operation completed successfully.");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult TestError()
        {
            this.SetErrorMessage("This is an error notification! Something went wrong.");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult TestWarning()
        {
            this.SetWarningMessage("This is a warning notification! Please be careful.");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult TestInfo()
        {
            this.SetInfoMessage("This is an info notification! Here's some useful information.");
            return RedirectToAction("Index");
        }

        // AJAX endpoints for testing
        [HttpPost]
        public IActionResult TestAjaxSuccess()
        {
            return Json(NotificationHelper.CreateNotificationResponse(true, "AJAX Success notification! Data processed successfully."));
        }

        [HttpPost]
        public IActionResult TestAjaxError()
        {
            return Json(NotificationHelper.CreateNotificationResponse(false, "AJAX Error notification! Request failed."));
        }
    }
}
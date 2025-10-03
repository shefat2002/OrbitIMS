using Microsoft.AspNetCore.Mvc;

namespace OrbitIMS.Helpers
{
    public static class NotificationHelper
    {
        public static void SetSuccessMessage(this Controller controller, string message)
        {
            controller.TempData["SuccessMessage"] = message;
        }

        public static void SetErrorMessage(this Controller controller, string message)
        {
            controller.TempData["ErrorMessage"] = message;
        }

        public static void SetWarningMessage(this Controller controller, string message)
        {
            controller.TempData["WarningMessage"] = message;
        }

        public static void SetInfoMessage(this Controller controller, string message)
        {
            controller.TempData["InfoMessage"] = message;
        }

        // For AJAX responses
        public static object CreateNotificationResponse(bool success, string message, object data = null)
        {
            return new
            {
                success = success,
                message = message,
                data = data,
                notificationType = success ? "success" : "error"
            };
        }
    }
}
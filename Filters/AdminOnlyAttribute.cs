using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ParkMate.Filters
{
    public class AdminOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(
            ActionExecutingContext context)
        {
            int? userId =
                context.HttpContext.Session.GetInt32("UserId");

            int? userRole =
                context.HttpContext.Session.GetInt32("UserRole");

            // Not logged in
            if (userId == null)
            {
                context.Result = new RedirectToActionResult(
                    "Login",
                    "Auth",
                    null);

                return;
            }

            // Logged in but not admin
            if (userRole != 1)
            {
                context.Result = new RedirectToActionResult(
                    "AccessDenied",
                    "Auth",
                    null);

                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
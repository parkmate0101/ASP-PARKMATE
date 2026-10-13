using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ParkMate.Filters
{
    public class ApiAdminOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            int? userId = context.HttpContext.Session.GetInt32("UserId");
            int? role = context.HttpContext.Session.GetInt32("UserRole");

            if (!userId.HasValue)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    success = false,
                    message = "Please login as an administrator."
                });
                return;
            }

            if (role != 1)
            {
                context.Result = new ObjectResult(new
                {
                    success = false,
                    message = "Administrator access is required."
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}

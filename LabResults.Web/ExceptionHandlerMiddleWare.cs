using LabResults.Domain;

namespace LabResults.Web
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // Default to 500 Internal Server Error
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            string message = "An internal server error occurred.";

            // Map custom exceptions to HTTP status codes
            switch (exception)
            {
                case NotFoundException _:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    message = exception.Message;
                    break;
                    // Add other exceptions here (e.g., BadRequest for validation errors)
            }

            // Return a standardized error response body
            return context.Response.WriteAsJsonAsync(new
            {
                status = context.Response.StatusCode,
                error = message
            });
        }
    }
}

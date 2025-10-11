using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Shelter.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
            => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (KeyNotFoundException ex)
            {
                await WriteProblem(context, StatusCodes.Status404NotFound, "Resource not found", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // Gdy naruszasz regułę biznesową (np. zmiana Type/AnimalId)
                await WriteProblem(context, StatusCodes.Status400BadRequest, "Invalid operation", ex.Message);
                // jeśli wolisz „konflikt” historii zamiast 400, użyj Status409Conflict
            }
            catch (UnauthorizedAccessException ex)
            {
                await WriteProblem(context, StatusCodes.Status403Forbidden, "Forbidden", ex.Message);
            }
            catch (Exception ex)
            {
                // Fallback na 500
                await WriteProblem(context, StatusCodes.Status500InternalServerError, "Server error",
                    "Unexpected server error. Please try again later.");
                // (opcjonalnie zaloguj ex)
            }
        }

        private static async Task WriteProblem(HttpContext context, int statusCode, string title, string? detail)
        {
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = statusCode;

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}

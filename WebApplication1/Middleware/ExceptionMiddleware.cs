using System;

namespace WebApplication1.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch(Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
     }
    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var statuseCode = ex switch
        {
            KeyNotFoundException => StatusCodes.Status404NotFound,
          //  ArgumentException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statuseCode;
        var response = new ErrorResponseDto
        {
            StatuseCode = statuseCode,
            Message = statuseCode == StatusCodes.Status500InternalServerError ? "Ошибка сервера" : ex.Message,
            TraceId = context.TraceIdentifier
        };
        await context.Response.WriteAsJsonAsync(response);
    }
}

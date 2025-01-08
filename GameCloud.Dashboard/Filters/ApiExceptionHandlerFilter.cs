using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;
using System.Text.Json;
using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace GameCloud.Dashboard.Filters;

public class ApiExceptionHandlerFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ApiException apiException)
        {
            HandleApiException(context, apiException);
        }
    }

    private void HandleApiException(ExceptionContext context, ApiException apiException)
    {
        try
        {
            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(
                apiException.Content ?? string.Empty,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (problemDetails != null)
            {
                HandleProblemDetails(context, problemDetails, apiException.StatusCode);
                return;
            }
        }
        catch (JsonException)
        {
            if (apiException.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                try
                {
                    var validationProblemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(
                        apiException.Content ?? string.Empty,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (validationProblemDetails?.Errors != null)
                    {
                        foreach (var error in validationProblemDetails.Errors)
                        {
                            foreach (var message in error.Value)
                            {
                                context.ModelState.AddModelError(error.Key, message);
                            }
                        }

                        SetResult(context);
                        return;
                    }
                }
                catch (JsonException)
                {
                    // Fall through to default handling
                }
            }
        }

        var defaultMessage = apiException.StatusCode switch
        {
            System.Net.HttpStatusCode.BadRequest => apiException.Message,
            System.Net.HttpStatusCode.Unauthorized => "Unauthorized access. Please log in again.",
            System.Net.HttpStatusCode.Forbidden => "You don't have permission to perform this action.",
            System.Net.HttpStatusCode.NotFound => "The requested resource was not found.",
            _ => $"An error occurred while processing your request. Status code: {apiException.StatusCode}"
        };

        context.ModelState.AddModelError(string.Empty, defaultMessage);
        SetResult(context);
    }

    private void HandleProblemDetails(ExceptionContext context, ProblemDetails problemDetails,
        System.Net.HttpStatusCode statusCode)
    {
        if (!string.IsNullOrEmpty(problemDetails.Detail))
        {
            context.ModelState.AddModelError(string.Empty, problemDetails.Detail);
        }
        else if (!string.IsNullOrEmpty(problemDetails.Title))
        {
            context.ModelState.AddModelError(string.Empty, problemDetails.Title);
        }

        if (problemDetails.Extensions?.Count > 0)
        {
            foreach (var extension in problemDetails.Extensions)
            {
                if (extension.Value is JsonElement jsonElement &&
                    jsonElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in jsonElement.EnumerateArray())
                    {
                        context.ModelState.AddModelError(extension.Key, item.GetString() ?? string.Empty);
                    }
                }
                else
                {
                    context.ModelState.AddModelError(extension.Key, extension.Value?.ToString() ?? string.Empty);
                }
            }
        }

        SetResult(context);
    }

    private void SetResult(ExceptionContext context)
    {
        context.ExceptionHandled = true;
        context.Result = new PageResult();
    }
}
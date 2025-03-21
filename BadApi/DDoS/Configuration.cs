using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace BadApi.DDoS;

public static class Configuration
{
    public static void ConfigureRateLimiting(this WebApplicationBuilder builder, string policyName)
    {
        // Configure rate limiting
        builder.Services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter(policyName, policy =>
            {
                policy.PermitLimit = 50;
                policy.Window = TimeSpan.FromSeconds(5);
                policy.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                policy.QueueLimit = 10;

            });
            options.OnRejected = (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int) retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
                }

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                return new ValueTask();
            };
        });
    }
}
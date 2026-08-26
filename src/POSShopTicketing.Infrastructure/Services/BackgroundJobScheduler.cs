using System.Linq.Expressions;
using Hangfire;
using POSShopTicketing.Application.Common.Interfaces;

namespace POSShopTicketing.Infrastructure.Services;

/// <summary>Thin wrapper so the Application layer can enqueue work
/// without referencing Hangfire directly.</summary>
public class BackgroundJobScheduler : IBackgroundJobScheduler
{
    public void Enqueue<T>(Expression<Func<T, Task>> methodCall) => BackgroundJob.Enqueue(methodCall);
}

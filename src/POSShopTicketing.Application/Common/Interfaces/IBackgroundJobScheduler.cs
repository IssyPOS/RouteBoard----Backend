using System.Linq.Expressions;

namespace POSShopTicketing.Application.Common.Interfaces;

/// <summary>
/// Thin abstraction over Hangfire so the Application layer can enqueue
/// work (send this reply, run this recurring SLA sweep) without taking a
/// direct dependency on the Hangfire package.
/// </summary>
public interface IBackgroundJobScheduler
{
    void Enqueue<T>(Expression<Func<T, Task>> methodCall);
}

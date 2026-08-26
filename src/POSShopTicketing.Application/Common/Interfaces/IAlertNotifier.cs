using POSShopTicketing.Application.Common.Models;

namespace POSShopTicketing.Application.Common.Interfaces;

/// <summary>Fires an alert (assignment, escalation, SLA breach). Default
/// implementation logs and writes a Notification row; swap in a real
/// push channel without touching event handlers.</summary>
public interface IAlertNotifier
{
    Task NotifyAsync(AlertMessage message, CancellationToken cancellationToken);
}

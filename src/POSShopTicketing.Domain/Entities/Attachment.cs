using POSShopTicketing.Domain.Common;

namespace POSShopTicketing.Domain.Entities;

public class Attachment : BaseEntity
{
    public Guid TicketMessageId { get; set; }
    public TicketMessage? TicketMessage { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }

    /// <summary>Blob storage URL - POSShopTicketing never stores file bytes
    /// itself.</summary>
    public string StorageUrl { get; set; } = string.Empty;
}

using System;
using System.Collections.Generic;

namespace NotificationApp.Infrastructure.Data.Models;

public partial class Notification
{
    public int notificationId { get; set; }

    public string title { get; set; } = null!;

    public string message { get; set; } = null!;

    public string? webLink { get; set; }

    public DateTime? expiresAtUTC { get; set; }

    public bool? isHighlighted { get; set; }

    public int createdByOracleEmployeeId { get; set; }

    public DateTimeOffset createdAtUTC { get; set; }

    public int? updatedByOracleEmployeeId { get; set; }

    public DateTimeOffset? updatedAtUTC { get; set; }

    public int? notificationTypeId { get; set; }

    public virtual NotificationType? notificationType { get; set; }

    public virtual ICollection<Participant> participant { get; set; } = new List<Participant>();

    public virtual ICollection<Partner> partner { get; set; } = new List<Partner>();
}

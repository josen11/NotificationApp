using System;
using System.Collections.Generic;

namespace NotificationApp.Infrastructure.Data.Models;

public partial class NotificationType
{
    public int notificationTypeId { get; set; }

    public string typeName { get; set; } = null!;

    public virtual ICollection<Notification> Notification { get; set; } = new List<Notification>();
}

using System;
using System.Collections.Generic;

namespace NotificationApp.Infrastructure.Data.Models;

public partial class Partner
{
    public int partnerId { get; set; }

    public string partnerName { get; set; } = null!;

    public string? emailAddress { get; set; }

    public virtual ICollection<Notification> notification { get; set; } = new List<Notification>();

    public virtual ICollection<Participant> participant { get; set; } = new List<Participant>();
}

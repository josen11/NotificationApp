using System;
using System.Collections.Generic;

namespace NotificationApp.Infrastructure.Data.Models;

public partial class Participant
{
    public int participantId { get; set; }

    public string firstName { get; set; } = null!;

    public string lastName { get; set; } = null!;

    public string? middleName { get; set; }

    public bool isDeceased { get; set; }

    public bool isVIP { get; set; }

    public bool isEscalated { get; set; }

    public virtual ICollection<Notification> notification { get; set; } = new List<Notification>();

    public virtual ICollection<Partner> partner { get; set; } = new List<Partner>();
}

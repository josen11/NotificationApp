using System;
using System.Collections.Generic;

namespace NotificationApp.Infrastructure.Data.Models;

public partial class MigrationHistory
{
    public int installed_rank { get; set; }

    public string? version { get; set; }

    public string? description { get; set; }

    public string type { get; set; } = null!;

    public string script { get; set; } = null!;

    public int? checksum { get; set; }

    public string installed_by { get; set; } = null!;

    public DateTime installed_on { get; set; }

    public int execution_time { get; set; }

    public bool success { get; set; }
}

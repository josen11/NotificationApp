using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using NotificationApp.Infrastructure.Data.Models;

namespace NotificationApp.Infrastructure.Data;

public partial class NotificationDbContext : DbContext
{
    public NotificationDbContext()
    {
    }

    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MigrationHistory> MigrationHistory { get; set; }

    public virtual DbSet<Notification> Notification { get; set; }

    public virtual DbSet<NotificationType> NotificationType { get; set; }

    public virtual DbSet<Participant> Participant { get; set; }

    public virtual DbSet<Partner> Partner { get; set; }

    public virtual DbSet<SeedDataHistory> SeedDataHistory { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=LocalServiceCenter;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Latin1_General_100_CI_AI_SC_UTF8");

        modelBuilder.Entity<MigrationHistory>(entity =>
        {
            entity.HasKey(e => e.installed_rank).HasName("MigrationHistory_pk");

            entity.HasIndex(e => e.success, "MigrationHistory_s_idx");

            entity.Property(e => e.installed_rank).ValueGeneratedNever();
            entity.Property(e => e.description).HasMaxLength(200);
            entity.Property(e => e.installed_by).HasMaxLength(100);
            entity.Property(e => e.installed_on)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.script).HasMaxLength(1000);
            entity.Property(e => e.type).HasMaxLength(20);
            entity.Property(e => e.version).HasMaxLength(50);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.notificationId).HasName("PK__Notifica__4BA5CEA9CB48889C");

            entity.Property(e => e.createdAtUTC).HasPrecision(2);
            entity.Property(e => e.expiresAtUTC).HasPrecision(2);
            entity.Property(e => e.isHighlighted).HasDefaultValue(false);
            entity.Property(e => e.message)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .UseCollation("Latin1_General_100_CI_AS_SC_UTF8");
            entity.Property(e => e.title)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.updatedAtUTC).HasPrecision(2);
            entity.Property(e => e.webLink)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.notificationType).WithMany(p => p.Notification)
                .HasForeignKey(d => d.notificationTypeId)
                .HasConstraintName("FK__Notificat__notif__5535A963");

            entity.HasMany(d => d.participant).WithMany(p => p.notification)
                .UsingEntity<Dictionary<string, object>>(
                    "NotificationParticipant",
                    r => r.HasOne<Participant>().WithMany()
                        .HasForeignKey("participantId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_NotificationParticipant_Participant"),
                    l => l.HasOne<Notification>().WithMany()
                        .HasForeignKey("notificationId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_NotificationParticipant_Notification"),
                    j =>
                    {
                        j.HasKey("notificationId", "participantId").HasName("PK__Notifica__5F4BB7887FED4F7C");
                    });

            entity.HasMany(d => d.partner).WithMany(p => p.notification)
                .UsingEntity<Dictionary<string, object>>(
                    "NotificationPartner",
                    r => r.HasOne<Partner>().WithMany()
                        .HasForeignKey("partnerId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_NotificationPartner_Partner"),
                    l => l.HasOne<Notification>().WithMany()
                        .HasForeignKey("notificationId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_NotificationPartner_Notification"),
                    j =>
                    {
                        j.HasKey("notificationId", "partnerId").HasName("PK__Notifica__1D4D63BE414FA431");
                    });
        });

        modelBuilder.Entity<NotificationType>(entity =>
        {
            entity.HasKey(e => e.notificationTypeId).HasName("PK__Notifica__9E58499EDFFCA625");

            entity.HasIndex(e => e.typeName, "UQ__Notifica__A20CDB58EC7942FA").IsUnique();

            entity.Property(e => e.typeName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Participant>(entity =>
        {
            entity.HasKey(e => e.participantId).HasName("PK__Particip__4EE7921033CEFC97");

            entity.Property(e => e.participantId).ValueGeneratedNever();
            entity.Property(e => e.firstName)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.lastName)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.middleName)
                .HasMaxLength(300)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Partner>(entity =>
        {
            entity.HasKey(e => e.partnerId).HasName("PK__Partner__6E8AD174DA5BDF81");

            entity.Property(e => e.partnerId).ValueGeneratedNever();
            entity.Property(e => e.emailAddress)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.partnerName)
                .HasMaxLength(300)
                .IsUnicode(false);

            entity.HasMany(d => d.participant).WithMany(p => p.partner)
                .UsingEntity<Dictionary<string, object>>(
                    "PartnerParticipantIdentificationMap",
                    r => r.HasOne<Participant>().WithMany()
                        .HasForeignKey("participantId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PartnerParticipant_Participant"),
                    l => l.HasOne<Partner>().WithMany()
                        .HasForeignKey("partnerId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PartnerParticipant_Partner"),
                    j =>
                    {
                        j.HasKey("partnerId", "participantId").HasName("PK__PartnerP__7A64A85508F4DEDD");
                    });
        });

        modelBuilder.Entity<SeedDataHistory>(entity =>
        {
            entity.HasKey(e => e.installed_rank).HasName("SeedDataHistory_pk");

            entity.HasIndex(e => e.success, "SeedDataHistory_s_idx");

            entity.Property(e => e.installed_rank).ValueGeneratedNever();
            entity.Property(e => e.description).HasMaxLength(200);
            entity.Property(e => e.installed_by).HasMaxLength(100);
            entity.Property(e => e.installed_on)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.script).HasMaxLength(1000);
            entity.Property(e => e.type).HasMaxLength(20);
            entity.Property(e => e.version).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

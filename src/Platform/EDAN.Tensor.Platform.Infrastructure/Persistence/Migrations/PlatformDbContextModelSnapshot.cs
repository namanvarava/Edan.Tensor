using EDAN.Tensor.Platform.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations;

/// <summary>현재 Platform 데이터베이스 모델을 Migration 비교 기준으로 보관합니다.</summary>
[DbContext(typeof(PlatformDbContext))]
public sealed class PlatformDbContextModelSnapshot : ModelSnapshot
{
    /// <summary>현재 테이블, 열, 인덱스 및 관계형 제약을 모델 스냅샷에 기록합니다.</summary>
    public static void ConfigureTargetModel(ModelBuilder modelBuilder)
    {
        new PlatformDbContextModelSnapshot().BuildModel(modelBuilder);
    }

    /// <summary>현재 테이블, 열, 인덱스 및 관계형 제약을 모델 스냅샷에 기록합니다.</summary>
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("platform_tenants");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value));
            entity.Property(x => x.Code).HasConversion(x => x.Value, x => new TenantCode(x)).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("platform_organizations");
            entity.HasKey(x => x.Id);
            entity.HasAlternateKey(x => new { x.Id, x.TenantId });
            entity.Property(x => x.Id).HasConversion(IdConverter<OrganizationId>(value => new OrganizationId(value), id => id.Value));
            entity.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value)).IsRequired();
            entity.Property(x => x.Code).HasConversion(x => x.Value, x => new OrganizationCode(x)).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("platform_projects");
            entity.HasKey(x => x.Id);
            entity.HasAlternateKey(x => new { x.Id, x.OrganizationId, x.TenantId });
            entity.Property(x => x.Id).HasConversion(IdConverter<ProjectId>(value => new ProjectId(value), id => id.Value));
            entity.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value));
            entity.Property(x => x.OrganizationId).HasConversion(IdConverter<OrganizationId>(value => new OrganizationId(value), id => id.Value));
            entity.Property(x => x.Code).HasConversion(x => x.Value, x => new ProjectCode(x)).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(x => new { x.OrganizationId, x.Code }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.OrganizationId });
            entity.HasIndex(x => new { x.OrganizationId, x.TenantId });
            entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Organization>().WithMany()
                .HasForeignKey(x => new { x.OrganizationId, x.TenantId })
                .HasPrincipalKey(x => new { x.Id, x.TenantId })
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("platform_users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasConversion(IdConverter<UserId>(value => new UserId(value), id => id.Value));
            entity.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value));
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.Email).HasConversion(x => x.Value, x => new EmailAddress(x)).HasMaxLength(320).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
            entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TenantMembership>(entity =>
        {
            entity.ToTable("platform_tenant_memberships");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasConversion(IdConverter<TenantMembershipId>(value => new TenantMembershipId(value), id => id.Value));
            entity.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value)).IsRequired();
            entity.Property(x => x.SubjectId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.SubjectId }).IsUnique();
            entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrganizationMembership>(entity =>
        {
            entity.ToTable("platform_organization_memberships");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasConversion(IdConverter<OrganizationMembershipId>(value => new OrganizationMembershipId(value), id => id.Value));
            entity.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value)).IsRequired();
            entity.Property(x => x.OrganizationId).HasConversion(IdConverter<OrganizationId>(value => new OrganizationId(value), id => id.Value)).IsRequired();
            entity.Property(x => x.SubjectId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.OrganizationId, x.SubjectId }).IsUnique();
            entity.HasIndex(x => new { x.OrganizationId, x.TenantId });
            entity.HasOne<Organization>().WithMany()
                .HasForeignKey(x => new { x.OrganizationId, x.TenantId })
                .HasPrincipalKey(x => new { x.Id, x.TenantId })
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProjectMembership>(entity =>
        {
            entity.ToTable("platform_project_memberships");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasConversion(IdConverter<ProjectMembershipId>(value => new ProjectMembershipId(value), id => id.Value));
            entity.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value)).IsRequired();
            entity.Property(x => x.OrganizationId).HasConversion(IdConverter<OrganizationId>(value => new OrganizationId(value), id => id.Value)).IsRequired();
            entity.Property(x => x.ProjectId).HasConversion(IdConverter<ProjectId>(value => new ProjectId(value), id => id.Value)).IsRequired();
            entity.Property(x => x.SubjectId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.OrganizationId, x.ProjectId, x.SubjectId }).IsUnique();
            entity.HasIndex(x => new { x.ProjectId, x.OrganizationId, x.TenantId });
            entity.HasOne<Project>().WithMany()
                .HasForeignKey(x => new { x.ProjectId, x.OrganizationId, x.TenantId })
                .HasPrincipalKey(x => new { x.Id, x.OrganizationId, x.TenantId })
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Resource>(entity =>
        {
            entity.ToTable("platform_resources", table =>
                table.HasCheckConstraint("CK_platform_resources_project_requires_organization", "project_id IS NULL OR organization_id IS NOT NULL"));
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasConversion(IdConverter<ResourceId>(value => new ResourceId(value), id => id.Value));
            entity.Ignore(x => x.Scope);
            entity.Property(x => x.ScopeTenantId).HasColumnName("tenant_id").HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value)).IsRequired();
            entity.Property(x => x.ScopeOrganizationId).HasColumnName("organization_id").HasConversion(OptionalIdConverter<OrganizationId>(value => new OrganizationId(value), id => id.Value));
            entity.Property(x => x.ScopeProjectId).HasColumnName("project_id").HasConversion(OptionalIdConverter<ProjectId>(value => new ProjectId(value), id => id.Value));
            entity.Property(x => x.ResourceType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(x => x.ScopeTenantId);
            entity.HasIndex(x => new { x.ScopeOrganizationId, x.ScopeTenantId });
            entity.HasIndex(x => new { x.ScopeProjectId, x.ScopeOrganizationId, x.ScopeTenantId });
            entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.ScopeTenantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Organization>().WithMany()
                .HasForeignKey(x => new { x.ScopeOrganizationId, x.ScopeTenantId })
                .HasPrincipalKey(x => new { x.Id, x.TenantId })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Project>().WithMany()
                .HasForeignKey(x => new { x.ScopeProjectId, x.ScopeOrganizationId, x.ScopeTenantId })
                .HasPrincipalKey(x => new { x.Id, x.OrganizationId, x.TenantId })
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Policy>(entity =>
        {
            entity.ToTable("platform_policies", table =>
                table.HasCheckConstraint("CK_platform_policies_project_requires_organization", "project_id IS NULL OR organization_id IS NOT NULL"));
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasConversion(IdConverter<PolicyId>(value => new PolicyId(value), id => id.Value));
            entity.Ignore(x => x.Scope);
            entity.Property(x => x.ScopeTenantId).HasColumnName("tenant_id").HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value));
            entity.Property(x => x.ScopeOrganizationId).HasColumnName("organization_id").HasConversion(OptionalIdConverter<OrganizationId>(value => new OrganizationId(value), id => id.Value));
            entity.Property(x => x.ScopeProjectId).HasColumnName("project_id").HasConversion(OptionalIdConverter<ProjectId>(value => new ProjectId(value), id => id.Value));
            entity.Property(x => x.ScopeTenantId).IsRequired();
            entity.Property(x => x.Key).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Value).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.Property(x => x.IsEnabled).IsRequired();
            entity.HasIndex(x => x.ScopeTenantId);
            entity.HasIndex(x => new { x.ScopeOrganizationId, x.ScopeTenantId });
            entity.HasIndex(x => new { x.ScopeProjectId, x.ScopeOrganizationId, x.ScopeTenantId });
            entity.HasIndex(x => new { x.Key, x.Version });
            entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.ScopeTenantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Organization>().WithMany()
                .HasForeignKey(x => new { x.ScopeOrganizationId, x.ScopeTenantId })
                .HasPrincipalKey(x => new { x.Id, x.TenantId })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Project>().WithMany()
                .HasForeignKey(x => new { x.ScopeProjectId, x.ScopeOrganizationId, x.ScopeTenantId })
                .HasPrincipalKey(x => new { x.Id, x.OrganizationId, x.TenantId })
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<AuthorizationPolicy>(entity =>
        {
            entity.ToTable("platform_authorization_policies", table =>
                table.HasCheckConstraint("CK_platform_authorization_policies_project_requires_organization", "ProjectId IS NULL OR OrganizationId IS NOT NULL"));
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasConversion(IdConverter<AuthorizationPolicyId>(value => new AuthorizationPolicyId(value), id => id.Value));
            entity.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value)).IsRequired();
            entity.Property(x => x.OrganizationId).HasConversion(OptionalIdConverter<OrganizationId>(value => new OrganizationId(value), id => id.Value));
            entity.Property(x => x.ProjectId).HasConversion(OptionalIdConverter<ProjectId>(value => new ProjectId(value), id => id.Value));
            entity.Property(x => x.SubjectId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(120).IsRequired();
            entity.Property(x => x.ResourceType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Effect).HasConversion<string>().HasMaxLength(16).IsRequired();
            entity.Property(x => x.Priority).IsRequired();
            entity.Property(x => x.IsEnabled).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.SubjectId, x.Action, x.ResourceType, x.IsEnabled });
            entity.HasIndex(x => new { x.OrganizationId, x.TenantId });
            entity.HasIndex(x => new { x.ProjectId, x.OrganizationId, x.TenantId });
            entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Organization>().WithMany()
                .HasForeignKey(x => new { x.OrganizationId, x.TenantId })
                .HasPrincipalKey(x => new { x.Id, x.TenantId })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Project>().WithMany()
                .HasForeignKey(x => new { x.ProjectId, x.OrganizationId, x.TenantId })
                .HasPrincipalKey(x => new { x.Id, x.OrganizationId, x.TenantId })
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuthorizationAuditEntry>(entity =>
        {
            entity.ToTable("platform_authorization_audit");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasConversion(IdConverter<AuthorizationAuditEntryId>(value => new AuthorizationAuditEntryId(value), id => id.Value));
            entity.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value)).IsRequired();
            entity.Property(x => x.SubjectId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(120).IsRequired();
            entity.Property(x => x.ResourceType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Scope).HasMaxLength(600).IsRequired();
            entity.Property(x => x.IsAllowed).IsRequired();
            entity.Property(x => x.ReasonCode).HasMaxLength(80).IsRequired();
            entity.Property(x => x.MatchedPolicyId).HasMaxLength(36);
            entity.Property(x => x.OccurredAtUtc).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.OccurredAtUtc });
            entity.HasIndex(x => new { x.SubjectId, x.OccurredAtUtc });
            entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuthorizationPolicyChangeAuditEntry>(entity =>
        {
            entity.ToTable("platform_authorization_policy_changes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasConversion(IdConverter<AuthorizationPolicyChangeAuditEntryId>(value => new AuthorizationPolicyChangeAuditEntryId(value), id => id.Value));
            entity.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value)).IsRequired();
            entity.Property(x => x.PolicyId).IsRequired();
            entity.Property(x => x.ActorSubjectId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ChangeType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.BeforeJson).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.AfterJson).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.OccurredAtUtc).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.OccurredAtUtc });
            entity.HasIndex(x => new { x.PolicyId, x.OccurredAtUtc });
            entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuthorizationAuditRetentionPolicy>(entity =>
        {
            entity.ToTable("platform_authorization_audit_retention");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value)).IsRequired();
            entity.Property(x => x.RetentionDays).IsRequired();
            entity.Property(x => x.IsLegalHold).IsRequired();
            entity.Property(x => x.LegalHoldReason).HasMaxLength(500);
            entity.Property(x => x.UpdatedBy).HasMaxLength(200).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.HasIndex(x => x.TenantId).IsUnique();
            entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuthorizationAuditRetentionChange>(entity =>
        {
            entity.ToTable("platform_authorization_audit_retention_changes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value)).IsRequired();
            entity.Property(x => x.PolicyId).IsRequired();
            entity.Property(x => x.ActorSubjectId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ChangeType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.BeforeJson).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.AfterJson).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.OccurredAtUtc).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.OccurredAtUtc });
            entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuthorizationAuditArchiveJob>(entity =>
        {
            entity.ToTable("platform_authorization_audit_archive_jobs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).IsRequired();
            entity.Property(x => x.ArchiveId);
            entity.Property(x => x.RecordCount).IsRequired();
            entity.Property(x => x.SizeBytes);
            entity.Property(x => x.CompletedAtUtc);
            entity.Property(x => x.AttemptCount).IsRequired();
            entity.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value)).IsRequired();
            entity.Property(x => x.FromUtc).IsRequired();
            entity.Property(x => x.ToUtc).IsRequired();
            entity.Property(x => x.RequestedBy).HasMaxLength(200).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Sha256).HasMaxLength(64);
            entity.Property(x => x.FailureReason).HasMaxLength(500);
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.HasAlternateKey(x => new { x.Id, x.TenantId });
            entity.HasIndex(x => new { x.TenantId, x.CreatedAtUtc });
            entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuthorizationAuditArchiveAttempt>(entity =>
        {
            entity.ToTable("platform_authorization_audit_archive_attempts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).IsRequired();
            entity.Property(x => x.JobId).IsRequired();
            entity.Property(x => x.AttemptNumber).IsRequired();
            entity.Property(x => x.FinishedAtUtc);
            entity.Property(x => x.IsSuccessful);
            entity.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value)).IsRequired();
            entity.Property(x => x.Operation).HasMaxLength(32).IsRequired();
            entity.Property(x => x.StartedAtUtc).IsRequired();
            entity.Property(x => x.ResultCode).HasMaxLength(200);
            entity.HasIndex(x => new { x.TenantId, x.JobId, x.StartedAtUtc });
            entity.HasIndex(x => new { x.JobId, x.TenantId });
            entity.HasOne<AuthorizationAuditArchiveJob>().WithMany()
                .HasForeignKey(x => new { x.JobId, x.TenantId })
                .HasPrincipalKey(x => new { x.Id, x.TenantId })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StorageQuotaPolicy>(entity =>
        {
            entity.ToTable("platform_storage_quota_policies");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value)).IsRequired();
            entity.Property(x => x.Scope).HasConversion<string>().HasMaxLength(24).IsRequired();
            entity.Property(x => x.TargetId).IsRequired();
            entity.Property(x => x.TenantLimitBytes);
            entity.Property(x => x.OrganizationLimitBytes);
            entity.Property(x => x.ProjectLimitBytes);
            entity.Property(x => x.DataSetLimitBytes);
            entity.Property(x => x.WarningThresholdPercent).IsRequired();
            entity.Property(x => x.PlanCode).HasMaxLength(64).IsRequired();
            entity.Property(x => x.IsEnabled).IsRequired();
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.Property(x => x.UpdatedBy).HasMaxLength(200).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.Scope, x.TargetId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.IsEnabled });
            entity.HasAlternateKey(x => new { x.Id, x.TenantId });
            entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StorageQuotaPolicyChangeAuditEntry>(entity =>
        {
            entity.ToTable("platform_storage_quota_policy_changes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value)).IsRequired();
            entity.Property(x => x.PolicyId).IsRequired();
            entity.Property(x => x.ActorSubjectId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ChangeType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.BeforeJson).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.AfterJson).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.OccurredAtUtc).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.OccurredAtUtc });
            entity.HasIndex(x => new { x.PolicyId, x.TenantId });
            entity.HasOne<StorageQuotaPolicy>().WithMany().HasForeignKey(x => new { x.PolicyId, x.TenantId }).HasPrincipalKey(x => new { x.Id, x.TenantId }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuthorizationAuditArchiveSchedule>(entity =>
        {
            entity.ToTable("platform_authorization_audit_archive_schedules");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).IsRequired();
            entity.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value)).IsRequired();
            entity.Property(x => x.IntervalHours).IsRequired();
            entity.Property(x => x.IsEnabled).IsRequired();
            entity.Property(x => x.NextRunAtUtc).IsRequired();
            entity.Property(x => x.LastRunAtUtc);
            entity.Property(x => x.LeaseId);
            entity.Property(x => x.LeaseExpiresAtUtc);
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.HasIndex(x => x.TenantId).IsUnique();
            entity.HasIndex(x => new { x.IsEnabled, x.NextRunAtUtc });
            entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        });

    }

    private static ValueConverter<TId, Guid> IdConverter<TId>(Func<Guid, TId> create, Func<TId, Guid> value) =>
        new(id => value(id), raw => create(raw));

    private static ValueConverter<TId?, Guid?> OptionalIdConverter<TId>(Func<Guid, TId> create, Func<TId, Guid> value)
        where TId : class =>
        new(id => id == null ? (Guid?)null : value(id), raw => raw == null ? null : create(raw.Value));
}

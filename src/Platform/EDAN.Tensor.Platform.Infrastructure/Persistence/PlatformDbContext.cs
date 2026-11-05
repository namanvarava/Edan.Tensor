using EDAN.Tensor.Platform.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EDAN.Tensor.Platform.Infrastructure.Persistence;

/// <summary>플랫폼 메타데이터 집합과 데이터베이스 매핑을 관리합니다.</summary>
public sealed class PlatformDbContext(DbContextOptions<PlatformDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<User> Users => Set<User>();
    public DbSet<TenantMembership> TenantMemberships => Set<TenantMembership>();
    public DbSet<OrganizationMembership> OrganizationMemberships => Set<OrganizationMembership>();
    public DbSet<ProjectMembership> ProjectMemberships => Set<ProjectMembership>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<AuthorizationPolicy> AuthorizationPolicies => Set<AuthorizationPolicy>();
    public DbSet<AuthorizationAuditEntry> AuthorizationAuditEntries => Set<AuthorizationAuditEntry>();
    public DbSet<AuthorizationPolicyChangeAuditEntry> AuthorizationPolicyChangeAuditEntries => Set<AuthorizationPolicyChangeAuditEntry>();
    public DbSet<AuthorizationAuditRetentionPolicy> AuthorizationAuditRetentionPolicies => Set<AuthorizationAuditRetentionPolicy>();
    public DbSet<AuthorizationAuditRetentionChange> AuthorizationAuditRetentionChanges => Set<AuthorizationAuditRetentionChange>();
    public DbSet<AuthorizationAuditArchiveJob> AuthorizationAuditArchiveJobs => Set<AuthorizationAuditArchiveJob>();
    public DbSet<AuthorizationAuditArchiveAttempt> AuthorizationAuditArchiveAttempts => Set<AuthorizationAuditArchiveAttempt>();

    /// <summary>테넌트 경계와 업무 코드의 고유 제약을 데이터 모델에 반영합니다.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureTenant(modelBuilder);
        ConfigureOrganization(modelBuilder);
        ConfigureProject(modelBuilder);
        ConfigureUser(modelBuilder);
        ConfigureTenantMembership(modelBuilder);
        ConfigureOrganizationMembership(modelBuilder);
        ConfigureProjectMembership(modelBuilder);
        ConfigureResource(modelBuilder);
        ConfigurePolicy(modelBuilder);
        ConfigureAuthorizationPolicy(modelBuilder);
        ConfigureAuthorizationAuditEntry(modelBuilder);
        ConfigureAuthorizationPolicyChangeAuditEntry(modelBuilder);
        ConfigureAuthorizationAuditRetention(modelBuilder);
        ConfigureAuthorizationAuditArchiveJobs(modelBuilder);
    }

    private static void ConfigureTenant(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Tenant>();
        entity.ToTable("platform_tenants");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value));
        entity.Property(x => x.Code).HasConversion(x => x.Value, x => new TenantCode(x)).HasMaxLength(64).IsRequired();
        entity.HasIndex(x => x.Code).IsUnique();
        entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
        entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        entity.Property(x => x.CreatedAtUtc).IsRequired();
    }

    private static void ConfigureOrganization(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Organization>();
        entity.ToTable("platform_organizations");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasConversion(IdConverter<OrganizationId>(value => new OrganizationId(value), id => id.Value));
        entity.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value)).IsRequired();
        entity.Property(x => x.Code).HasConversion(x => x.Value, x => new OrganizationCode(x)).HasMaxLength(64).IsRequired();
        entity.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
        entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        entity.HasIndex(x => x.TenantId);
        entity.HasAlternateKey(x => new { x.Id, x.TenantId });
        entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureProject(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Project>();
        entity.ToTable("platform_projects");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasConversion(IdConverter<ProjectId>(value => new ProjectId(value), id => id.Value));
        entity.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value));
        entity.Property(x => x.OrganizationId).HasConversion(IdConverter<OrganizationId>(value => new OrganizationId(value), id => id.Value));
        entity.Property(x => x.Code).HasConversion(x => x.Value, x => new ProjectCode(x)).HasMaxLength(64).IsRequired();
        entity.HasIndex(x => new { x.OrganizationId, x.Code }).IsUnique();
        entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
        entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        entity.HasIndex(x => new { x.TenantId, x.OrganizationId });
        entity.HasAlternateKey(x => new { x.Id, x.OrganizationId, x.TenantId });
        entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<Organization>().WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.TenantId })
            .HasPrincipalKey(x => new { x.Id, x.TenantId })
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<User>();
        entity.ToTable("platform_users");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasConversion(IdConverter<UserId>(value => new UserId(value), id => id.Value));
        entity.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value));
        entity.Property(x => x.TenantId).IsRequired();
        entity.Property(x => x.Email).HasConversion(x => x.Value, x => new EmailAddress(x)).HasMaxLength(320).IsRequired();
        entity.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
        entity.Property(x => x.DisplayName).HasMaxLength(120).IsRequired();
        entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
    }

    /// <summary>외부 인증 주체의 테넌트 소속과 역할을 고유하게 저장합니다.</summary>
    private static void ConfigureTenantMembership(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<TenantMembership>();
        entity.ToTable("platform_tenant_memberships");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasConversion(IdConverter<TenantMembershipId>(value => new TenantMembershipId(value), id => id.Value));
        entity.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value)).IsRequired();
        entity.Property(x => x.SubjectId).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(32).IsRequired();
        entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        entity.HasIndex(x => new { x.TenantId, x.SubjectId }).IsUnique();
        entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
    }

    /// <summary>조직 소속을 Tenant·Organization·Subject 조합으로 고유하게 저장합니다.</summary>
    private static void ConfigureOrganizationMembership(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<OrganizationMembership>();
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
    }

    /// <summary>프로젝트 소속을 Tenant·Organization·Project·Subject 조합으로 고유하게 저장합니다.</summary>
    private static void ConfigureProjectMembership(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ProjectMembership>();
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
    }

    private static void ConfigureResource(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Resource>();
        entity.ToTable("platform_resources", table => table.HasCheckConstraint("CK_platform_resources_project_requires_organization", "project_id IS NULL OR organization_id IS NOT NULL"));
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasConversion(IdConverter<ResourceId>(value => new ResourceId(value), id => id.Value));
        entity.Ignore(x => x.Scope);
        entity.Property(x => x.ScopeTenantId).HasColumnName("tenant_id").HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value)).IsRequired();
        entity.Property(x => x.ScopeOrganizationId).HasColumnName("organization_id").HasConversion(OptionalIdConverter<OrganizationId>(value => new OrganizationId(value), id => id.Value));
        entity.Property(x => x.ScopeProjectId).HasColumnName("project_id").HasConversion(OptionalIdConverter<ProjectId>(value => new ProjectId(value), id => id.Value));
        entity.HasIndex(x => x.ScopeTenantId);
        entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.ScopeTenantId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<Organization>().WithMany()
            .HasForeignKey(x => new { x.ScopeOrganizationId, x.ScopeTenantId })
            .HasPrincipalKey(x => new { x.Id, x.TenantId })
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<Project>().WithMany()
            .HasForeignKey(x => new { x.ScopeProjectId, x.ScopeOrganizationId, x.ScopeTenantId })
            .HasPrincipalKey(x => new { x.Id, x.OrganizationId, x.TenantId })
            .OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.ResourceType).HasMaxLength(80).IsRequired();
        entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
        entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
    }

    private static void ConfigurePolicy(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Policy>();
        entity.ToTable("platform_policies", table => table.HasCheckConstraint("CK_platform_policies_project_requires_organization", "project_id IS NULL OR organization_id IS NOT NULL"));
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasConversion(IdConverter<PolicyId>(value => new PolicyId(value), id => id.Value));
        entity.Ignore(x => x.Scope);
        entity.Property(x => x.ScopeTenantId).HasColumnName("tenant_id").HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value));
        entity.Property(x => x.ScopeOrganizationId).HasColumnName("organization_id").HasConversion(OptionalIdConverter<OrganizationId>(value => new OrganizationId(value), id => id.Value));
        entity.Property(x => x.ScopeProjectId).HasColumnName("project_id").HasConversion(OptionalIdConverter<ProjectId>(value => new ProjectId(value), id => id.Value));
        entity.Property(x => x.ScopeTenantId).IsRequired();
        entity.HasIndex(x => x.ScopeTenantId);
        entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.ScopeTenantId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<Organization>().WithMany()
            .HasForeignKey(x => new { x.ScopeOrganizationId, x.ScopeTenantId })
            .HasPrincipalKey(x => new { x.Id, x.TenantId })
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<Project>().WithMany()
            .HasForeignKey(x => new { x.ScopeProjectId, x.ScopeOrganizationId, x.ScopeTenantId })
            .HasPrincipalKey(x => new { x.Id, x.OrganizationId, x.TenantId })
            .OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.Key).HasMaxLength(120).IsRequired();
        entity.Property(x => x.Value).HasMaxLength(4000).IsRequired();
        entity.Property(x => x.Version).IsConcurrencyToken();
        entity.HasIndex(x => new { x.Key, x.Version });
    }

    /// <summary>접근 정책은 Tenant 소유권과 선택적 하위 범위를 함께 저장합니다.</summary>
    private static void ConfigureAuthorizationPolicy(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AuthorizationPolicy>();
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
    }

    /// <summary>권한 평가 감사 기록은 추가 전용 데이터로 저장합니다.</summary>
    private static void ConfigureAuthorizationAuditEntry(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AuthorizationAuditEntry>();
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
    }

    /// <summary>접근 정책 변경 이력은 Tenant별 추가 전용 감사 데이터로 저장합니다.</summary>
    private static void ConfigureAuthorizationPolicyChangeAuditEntry(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AuthorizationPolicyChangeAuditEntry>();
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
    }


    /// <summary>감사 보존 정책과 변경 이력을 Tenant 경계로 저장합니다.</summary>
    private static void ConfigureAuthorizationAuditRetention(ModelBuilder modelBuilder)
    {
        var policy = modelBuilder.Entity<AuthorizationAuditRetentionPolicy>();
        policy.ToTable("platform_authorization_audit_retention");
        policy.HasKey(x => x.Id);
        policy.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value)).IsRequired();
        policy.Property(x => x.RetentionDays).IsRequired();
        policy.Property(x => x.IsLegalHold).IsRequired();
        policy.Property(x => x.LegalHoldReason).HasMaxLength(500);
        policy.Property(x => x.UpdatedBy).HasMaxLength(200).IsRequired();
        policy.Property(x => x.UpdatedAtUtc).IsRequired();
        policy.Property(x => x.Version).IsConcurrencyToken();
        policy.HasIndex(x => x.TenantId).IsUnique();
        policy.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);

        var change = modelBuilder.Entity<AuthorizationAuditRetentionChange>();
        change.ToTable("platform_authorization_audit_retention_changes");
        change.HasKey(x => x.Id);
        change.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value)).IsRequired();
        change.Property(x => x.PolicyId).IsRequired();
        change.Property(x => x.ActorSubjectId).HasMaxLength(200).IsRequired();
        change.Property(x => x.ChangeType).HasMaxLength(32).IsRequired();
        change.Property(x => x.BeforeJson).HasMaxLength(4000).IsRequired();
        change.Property(x => x.AfterJson).HasMaxLength(4000).IsRequired();
        change.Property(x => x.OccurredAtUtc).IsRequired();
        change.HasIndex(x => new { x.TenantId, x.OccurredAtUtc });
        change.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
    }


    /// <summary>감사 아카이브 작업과 실행 시도 이력을 Tenant 경계로 저장합니다.</summary>
    private static void ConfigureAuthorizationAuditArchiveJobs(ModelBuilder modelBuilder)
    {
        var job = modelBuilder.Entity<AuthorizationAuditArchiveJob>();
        job.ToTable("platform_authorization_audit_archive_jobs");
        job.HasKey(x => x.Id);
        job.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value)).IsRequired();
        job.Property(x => x.FromUtc).IsRequired();
        job.Property(x => x.ToUtc).IsRequired();
        job.Property(x => x.RequestedBy).HasMaxLength(200).IsRequired();
        job.Property(x => x.CreatedAtUtc).IsRequired();
        job.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        job.Property(x => x.Sha256).HasMaxLength(64);
        job.Property(x => x.FailureReason).HasMaxLength(500);
        job.Property(x => x.Version).IsConcurrencyToken();
        job.HasAlternateKey(x => new { x.Id, x.TenantId });
        job.HasIndex(x => new { x.TenantId, x.CreatedAtUtc });
        job.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);

        var attempt = modelBuilder.Entity<AuthorizationAuditArchiveAttempt>();
        attempt.ToTable("platform_authorization_audit_archive_attempts");
        attempt.HasKey(x => x.Id);
        attempt.Property(x => x.TenantId).HasConversion(IdConverter<TenantId>(value => new TenantId(value), id => id.Value)).IsRequired();
        attempt.Property(x => x.Operation).HasMaxLength(32).IsRequired();
        attempt.Property(x => x.StartedAtUtc).IsRequired();
        attempt.Property(x => x.ResultCode).HasMaxLength(200);
        attempt.HasIndex(x => new { x.TenantId, x.JobId, x.StartedAtUtc });
        attempt.HasIndex(x => new { x.JobId, x.TenantId });
        attempt.HasOne<AuthorizationAuditArchiveJob>().WithMany()
            .HasForeignKey(x => new { x.JobId, x.TenantId })
            .HasPrincipalKey(x => new { x.Id, x.TenantId })
            .OnDelete(DeleteBehavior.Restrict);
        attempt.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
    }

    private static ValueConverter<TId, Guid> IdConverter<TId>(Func<Guid, TId> create, Func<TId, Guid> value) =>
        new(id => value(id), raw => create(raw));

    private static ValueConverter<TId?, Guid?> OptionalIdConverter<TId>(Func<Guid, TId> create, Func<TId, Guid> value)
        where TId : class =>
        new(id => id == null ? (Guid?)null : value(id), raw => raw == null ? null : create(raw.Value));
}

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
    }

    private static ValueConverter<TId, Guid> IdConverter<TId>(Func<Guid, TId> create, Func<TId, Guid> value) =>
        new(id => value(id), raw => create(raw));

    private static ValueConverter<TId?, Guid?> OptionalIdConverter<TId>(Func<Guid, TId> create, Func<TId, Guid> value)
        where TId : class =>
        new(id => id == null ? (Guid?)null : value(id), raw => raw == null ? null : create(raw.Value));
}

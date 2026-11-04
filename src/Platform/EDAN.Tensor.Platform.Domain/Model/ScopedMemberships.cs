using EDAN.Tensor.SharedKernel.Exceptions;
using EDAN.Tensor.SharedKernel.Primitives;

namespace EDAN.Tensor.Platform.Domain.Model;

/// <summary>조직 소속 식별자입니다.</summary>
public sealed record OrganizationMembershipId : StronglyTypedId<Guid>
{
    public OrganizationMembershipId(Guid value) : base(value == Guid.Empty ? throw new ArgumentException("조직 소속 식별자는 비어 있을 수 없습니다.") : value) { }
}

/// <summary>프로젝트 소속 식별자입니다.</summary>
public sealed record ProjectMembershipId : StronglyTypedId<Guid>
{
    public ProjectMembershipId(Guid value) : base(value == Guid.Empty ? throw new ArgumentException("프로젝트 소속 식별자는 비어 있을 수 없습니다.") : value) { }
}

public enum OrganizationRole { Reader, Contributor, Manager }
public enum ProjectRole { Reader, Contributor, Manager }
public enum ScopedMembershipStatus { Active, Revoked }

/// <summary>외부 인증 주체의 조직 범위 접근 역할과 상태를 관리합니다.</summary>
public sealed class OrganizationMembership : AggregateRoot<OrganizationMembershipId>
{
    private OrganizationMembership(OrganizationMembershipId id, TenantId tenantId, OrganizationId organizationId, string subjectId, OrganizationRole role) : base(id)
    {
        TenantId = tenantId;
        OrganizationId = organizationId;
        SubjectId = ValidateSubject(subjectId);
        Role = role;
        Status = ScopedMembershipStatus.Active;
    }

    public TenantId TenantId { get; }
    public OrganizationId OrganizationId { get; }
    public string SubjectId { get; }
    public OrganizationRole Role { get; private set; }
    public ScopedMembershipStatus Status { get; private set; }

    /// <summary>인증 주체를 조직의 구성원으로 등록합니다.</summary>
    public static OrganizationMembership Grant(OrganizationMembershipId id, TenantId tenantId, OrganizationId organizationId, string subjectId, OrganizationRole role)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(tenantId);
        ArgumentNullException.ThrowIfNull(organizationId);
        if (!Enum.IsDefined(role)) throw new ArgumentOutOfRangeException(nameof(role));
        return new(id, tenantId, organizationId, subjectId, role);
    }

    /// <summary>활성 조직 구성원의 역할을 변경합니다.</summary>
    public void ChangeRole(OrganizationRole role)
    {
        EnsureActive();
        if (!Enum.IsDefined(role)) throw new ArgumentOutOfRangeException(nameof(role));
        Role = role;
    }

    /// <summary>조직 범위의 접근 권한을 폐기합니다.</summary>
    public void Revoke() { EnsureActive(); Status = ScopedMembershipStatus.Revoked; }

    private void EnsureActive()
    {
        if (Status != ScopedMembershipStatus.Active)
            throw new BusinessRuleViolationException("platform.organization_membership.revoked", "폐기된 조직 소속은 변경할 수 없습니다.");
    }

    internal static string ValidateSubject(string subjectId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subjectId);
        var value = subjectId.Trim();
        if (value.Length > 200) throw new ArgumentException("외부 사용자 식별자는 200자를 초과할 수 없습니다.");
        return value;
    }
}

/// <summary>외부 인증 주체의 프로젝트 범위 접근 역할과 상태를 관리합니다.</summary>
public sealed class ProjectMembership : AggregateRoot<ProjectMembershipId>
{
    private ProjectMembership(ProjectMembershipId id, TenantId tenantId, OrganizationId organizationId, ProjectId projectId, string subjectId, ProjectRole role) : base(id)
    {
        TenantId = tenantId;
        OrganizationId = organizationId;
        ProjectId = projectId;
        SubjectId = OrganizationMembership.ValidateSubject(subjectId);
        Role = role;
        Status = ScopedMembershipStatus.Active;
    }

    public TenantId TenantId { get; }
    public OrganizationId OrganizationId { get; }
    public ProjectId ProjectId { get; }
    public string SubjectId { get; }
    public ProjectRole Role { get; private set; }
    public ScopedMembershipStatus Status { get; private set; }

    /// <summary>인증 주체를 프로젝트의 구성원으로 등록합니다.</summary>
    public static ProjectMembership Grant(ProjectMembershipId id, TenantId tenantId, OrganizationId organizationId, ProjectId projectId, string subjectId, ProjectRole role)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(tenantId);
        ArgumentNullException.ThrowIfNull(organizationId);
        ArgumentNullException.ThrowIfNull(projectId);
        if (!Enum.IsDefined(role)) throw new ArgumentOutOfRangeException(nameof(role));
        return new(id, tenantId, organizationId, projectId, subjectId, role);
    }

    /// <summary>활성 프로젝트 구성원의 역할을 변경합니다.</summary>
    public void ChangeRole(ProjectRole role)
    {
        EnsureActive();
        if (!Enum.IsDefined(role)) throw new ArgumentOutOfRangeException(nameof(role));
        Role = role;
    }

    /// <summary>프로젝트 범위의 접근 권한을 폐기합니다.</summary>
    public void Revoke() { EnsureActive(); Status = ScopedMembershipStatus.Revoked; }

    private void EnsureActive()
    {
        if (Status != ScopedMembershipStatus.Active)
            throw new BusinessRuleViolationException("platform.project_membership.revoked", "폐기된 프로젝트 소속은 변경할 수 없습니다.");
    }
}

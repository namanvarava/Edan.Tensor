using EDAN.Tensor.SharedKernel.Primitives;
using EDAN.Tensor.SharedKernel.Exceptions;

namespace EDAN.Tensor.Platform.Domain.Model;

/// <summary>외부 인증 주체와 테넌트 사이의 활성 소속 및 역할을 관리합니다.</summary>
public sealed class TenantMembership : AggregateRoot<TenantMembershipId>
{
    private TenantMembership(TenantMembershipId id, TenantId tenantId, string subjectId, TenantRole role)
        : base(id)
    {
        TenantId = tenantId;
        SubjectId = ValidateSubject(subjectId);
        Role = role;
        Status = TenantMembershipStatus.Active;
    }

    public TenantId TenantId { get; }
    public string SubjectId { get; }
    public TenantRole Role { get; private set; }
    public TenantMembershipStatus Status { get; private set; }

    /// <summary>외부 인증 주체를 하나의 테넌트 구성원으로 등록합니다.</summary>
    public static TenantMembership Grant(TenantMembershipId id, TenantId tenantId, string subjectId, TenantRole role)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(tenantId);
        if (!Enum.IsDefined(role))
            throw new ArgumentOutOfRangeException(nameof(role));
        return new TenantMembership(id, tenantId, subjectId, role);
    }

    /// <summary>활성 구성원의 역할을 변경합니다.</summary>
    public void ChangeRole(TenantRole role)
    {
        EnsureActive();
        if (!Enum.IsDefined(role))
            throw new ArgumentOutOfRangeException(nameof(role));
        Role = role;
    }

    /// <summary>테넌트 접근 권한을 폐기합니다.</summary>
    public void Revoke()
    {
        EnsureActive();
        Status = TenantMembershipStatus.Revoked;
    }

    private void EnsureActive()
    {
        if (Status != TenantMembershipStatus.Active)
            throw new BusinessRuleViolationException("platform.membership.revoked", "폐기된 테넌트 소속은 변경할 수 없습니다.");
    }

    private static string ValidateSubject(string subjectId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subjectId);
        var value = subjectId.Trim();
        if (value.Length > 200)
            throw new ArgumentException("외부 사용자 식별자는 200자를 초과할 수 없습니다.");
        return value;
    }
}

/// <summary>테넌트 소속 레코드의 고유 식별자입니다.</summary>
public sealed record TenantMembershipId : StronglyTypedId<Guid>
{
    public TenantMembershipId(Guid value) : base(value == Guid.Empty
        ? throw new ArgumentException("테넌트 소속 식별자는 비어 있을 수 없습니다.")
        : value) { }
}

/// <summary>테넌트 내부에서 부여할 수 있는 역할입니다.</summary>
public enum TenantRole { Member, Administrator }

/// <summary>테넌트 소속의 접근 가능 상태입니다.</summary>
public enum TenantMembershipStatus { Active, Revoked }

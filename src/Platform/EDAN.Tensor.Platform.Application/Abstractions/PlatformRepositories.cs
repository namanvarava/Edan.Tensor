using EDAN.Tensor.Platform.Domain.Model;

namespace EDAN.Tensor.Platform.Application.Abstractions;

/// <summary>테넌트 저장소는 식별자와 시스템 범위 코드로 테넌트를 조회합니다.</summary>
public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(TenantId id, CancellationToken cancellationToken = default);
    Task<bool> ExistsCodeAsync(TenantCode code, CancellationToken cancellationToken = default);
    void Add(Tenant tenant);
}

/// <summary>조직 저장소는 테넌트 범위에서 조직을 조회합니다.</summary>
public interface IOrganizationRepository
{
    Task<Organization?> GetByIdAsync(OrganizationId id, CancellationToken cancellationToken = default);
    Task<bool> ExistsCodeAsync(TenantId tenantId, OrganizationCode code, CancellationToken cancellationToken = default);
    void Add(Organization organization);
}

/// <summary>프로젝트 저장소는 조직 범위에서 프로젝트 코드를 확인합니다.</summary>
public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(ProjectId id, CancellationToken cancellationToken = default);
    Task<bool> ExistsCodeAsync(OrganizationId organizationId, ProjectCode code, CancellationToken cancellationToken = default);
    void Add(Project project);
}

/// <summary>사용자 저장소는 테넌트 범위에서 이메일 중복을 확인합니다.</summary>
public interface IUserRepository
{
    Task<bool> ExistsEmailAsync(TenantId tenantId, EmailAddress email, CancellationToken cancellationToken = default);
    void Add(User user);
}

/// <summary>자원 저장소는 새 자원을 등록합니다.</summary>
public interface IResourceRepository
{
    void Add(Resource resource);
}

/// <summary>정책 저장소는 새 정책을 등록합니다.</summary>
public interface IPolicyRepository
{
    void Add(Policy policy);
}

/// <summary>작업 단위는 변경된 도메인 객체를 하나의 트랜잭션으로 저장합니다.</summary>
public interface IPlatformUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

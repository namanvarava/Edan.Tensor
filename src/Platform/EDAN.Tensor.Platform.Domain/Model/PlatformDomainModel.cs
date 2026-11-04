using System.Net.Mail;
using EDAN.Tensor.SharedKernel.Exceptions;
using EDAN.Tensor.SharedKernel.Primitives;
namespace EDAN.Tensor.Platform.Domain.Model;

public sealed record TenantId(Guid Value):StronglyTypedId<Guid>(Value==Guid.Empty?throw new ArgumentException("테넌트 식별자는 비어 있을 수 없습니다."):Value);
public sealed record OrganizationId(Guid Value):StronglyTypedId<Guid>(Value==Guid.Empty?throw new ArgumentException("조직 식별자는 비어 있을 수 없습니다."):Value);
public sealed record ProjectId(Guid Value):StronglyTypedId<Guid>(Value==Guid.Empty?throw new ArgumentException("프로젝트 식별자는 비어 있을 수 없습니다."):Value);
public sealed record UserId(Guid Value):StronglyTypedId<Guid>(Value==Guid.Empty?throw new ArgumentException("사용자 식별자는 비어 있을 수 없습니다."):Value);
public sealed record ResourceId(Guid Value):StronglyTypedId<Guid>(Value==Guid.Empty?throw new ArgumentException("자원 식별자는 비어 있을 수 없습니다."):Value);
public sealed record PolicyId(Guid Value):StronglyTypedId<Guid>(Value==Guid.Empty?throw new ArgumentException("정책 식별자는 비어 있을 수 없습니다."):Value);
internal static class Rules
{
 public static string Code(string value){ArgumentException.ThrowIfNullOrWhiteSpace(value);var v=value.Trim().ToUpperInvariant();if(v.Length is <2 or >64||v[0]=='-'||v[^1]=='-'||v.Contains("--",StringComparison.Ordinal)||v.Any(c=>!(c is >= 'A' and <= 'Z' or >= '0' and <= '9' or '-')))throw new ArgumentException("코드는 영문 대문자, 숫자, 하이픈으로 구성된 2~64자여야 합니다.");return v;}
 public static string Text(string value,int max){ArgumentException.ThrowIfNullOrWhiteSpace(value);var v=value.Trim();if(v.Length>max)throw new ArgumentException($"문자 길이는 {max}자를 초과할 수 없습니다.");return v;}
}
public sealed record TenantCode(string Value){public string Value{get;init;}=Rules.Code(Value);}
public sealed record OrganizationCode(string Value){public string Value{get;init;}=Rules.Code(Value);}
public sealed record ProjectCode(string Value){public string Value{get;init;}=Rules.Code(Value);}
public enum TenantStatus{Provisioning,Active,Suspended,Closed}
public enum OrganizationStatus{Active,Suspended,Closed}
public enum ProjectStatus{Active,Suspended,Archived}
public enum UserStatus{Invited,Active,Suspended,Deactivated}
public enum ResourceStatus{Active,Suspended,Retired}

/// <summary>테넌트는 데이터 격리와 소유권의 최상위 경계입니다.</summary>
public sealed class Tenant:AggregateRoot<TenantId>
{
 private Tenant(TenantId id,TenantCode code,string name,DateTimeOffset created):base(id){Code=code;Name=Rules.Text(name,160);CreatedAtUtc=created.ToUniversalTime();Status=TenantStatus.Provisioning;}
 public TenantCode Code{get;} public string Name{get;private set;} public TenantStatus Status{get;private set;} public DateTimeOffset CreatedAtUtc{get;}
 public static Tenant Create(TenantId id,TenantCode code,string name,DateTimeOffset created){ArgumentNullException.ThrowIfNull(id);ArgumentNullException.ThrowIfNull(code);return new(id,code,name,created);}
 public void Rename(string name){Open();Name=Rules.Text(name,160);} public void Activate(){if(Status is not(TenantStatus.Provisioning or TenantStatus.Suspended))Invalid();Status=TenantStatus.Active;}
 public void Suspend(){if(Status!=TenantStatus.Active)Invalid();Status=TenantStatus.Suspended;} public void Close(){Open();Status=TenantStatus.Closed;}
 private void Open(){if(Status==TenantStatus.Closed)Invalid();} private static void Invalid()=>throw new BusinessRuleViolationException("platform.tenant.invalid_transition","현재 테넌트 상태에서는 요청한 상태 변경을 수행할 수 없습니다.");
}

/// <summary>조직은 하나의 테넌트에 귀속되는 소유권 단위입니다.</summary>
public sealed class Organization:AggregateRoot<OrganizationId>
{
 private Organization(OrganizationId id,TenantId tenant,OrganizationCode code,string name):base(id){TenantId=tenant;Code=code;Name=Rules.Text(name,160);Status=OrganizationStatus.Active;}
 public TenantId TenantId{get;} public OrganizationCode Code{get;} public string Name{get;private set;} public OrganizationStatus Status{get;private set;}
 public static Organization Create(OrganizationId id,TenantId tenant,OrganizationCode code,string name){ArgumentNullException.ThrowIfNull(id);ArgumentNullException.ThrowIfNull(tenant);ArgumentNullException.ThrowIfNull(code);return new(id,tenant,code,name);}
 public void Rename(string name){Open();Name=Rules.Text(name,160);} public void Suspend(){if(Status!=OrganizationStatus.Active)Invalid();Status=OrganizationStatus.Suspended;} public void Activate(){if(Status!=OrganizationStatus.Suspended)Invalid();Status=OrganizationStatus.Active;} public void Close(){Open();Status=OrganizationStatus.Closed;}
 private void Open(){if(Status==OrganizationStatus.Closed)Invalid();} private static void Invalid()=>throw new BusinessRuleViolationException("platform.organization.invalid_transition","현재 조직 상태에서는 요청한 상태 변경을 수행할 수 없습니다.");
}

/// <summary>프로젝트는 테넌트와 조직의 소속 식별자를 함께 보유합니다.</summary>
public sealed class Project:AggregateRoot<ProjectId>
{
 private Project(ProjectId id,TenantId tenant,OrganizationId org,ProjectCode code,string name):base(id){TenantId=tenant;OrganizationId=org;Code=code;Name=Rules.Text(name,160);Status=ProjectStatus.Active;}
 public TenantId TenantId{get;} public OrganizationId OrganizationId{get;} public ProjectCode Code{get;} public string Name{get;private set;} public ProjectStatus Status{get;private set;}
 public static Project Create(ProjectId id,TenantId tenant,OrganizationId org,ProjectCode code,string name){ArgumentNullException.ThrowIfNull(id);ArgumentNullException.ThrowIfNull(tenant);ArgumentNullException.ThrowIfNull(org);ArgumentNullException.ThrowIfNull(code);return new(id,tenant,org,code,name);}
 public void Rename(string name){Open();Name=Rules.Text(name,160);} public void Suspend(){if(Status!=ProjectStatus.Active)Invalid();Status=ProjectStatus.Suspended;} public void Activate(){if(Status!=ProjectStatus.Suspended)Invalid();Status=ProjectStatus.Active;} public void Archive(){Open();Status=ProjectStatus.Archived;}
 private void Open(){if(Status==ProjectStatus.Archived)Invalid();} private static void Invalid()=>throw new BusinessRuleViolationException("platform.project.invalid_transition","현재 프로젝트 상태에서는 요청한 상태 변경을 수행할 수 없습니다.");
}
public sealed record EmailAddress
{
 public EmailAddress(string value){ArgumentException.ThrowIfNullOrWhiteSpace(value);try{var p=new MailAddress(value.Trim());if(!string.Equals(p.Address,value.Trim(),StringComparison.OrdinalIgnoreCase))throw new FormatException();Value=p.Address.ToLowerInvariant();}catch(FormatException){throw new ArgumentException("올바른 이메일 주소를 입력해야 합니다.");}}
 public string Value{get;} public override string ToString()=>Value;
}
/// <summary>사용자는 테넌트에 귀속되며 이메일 중복은 테넌트 범위에서 관리합니다.</summary>
public sealed class User:AggregateRoot<UserId>
{
 private User(UserId id,TenantId tenant,EmailAddress email,string name):base(id){TenantId=tenant;Email=email;DisplayName=Rules.Text(name,120);Status=UserStatus.Invited;}
 public TenantId TenantId{get;} public EmailAddress Email{get;} public string DisplayName{get;private set;} public UserStatus Status{get;private set;}
 public static User Invite(UserId id,TenantId tenant,EmailAddress email,string name){ArgumentNullException.ThrowIfNull(id);ArgumentNullException.ThrowIfNull(tenant);ArgumentNullException.ThrowIfNull(email);return new(id,tenant,email,name);}
 public void Rename(string name){Open();DisplayName=Rules.Text(name,120);} public void Activate(){if(Status is not(UserStatus.Invited or UserStatus.Suspended))Invalid();Status=UserStatus.Active;} public void Suspend(){if(Status!=UserStatus.Active)Invalid();Status=UserStatus.Suspended;} public void Deactivate(){Open();Status=UserStatus.Deactivated;}
 private void Open(){if(Status==UserStatus.Deactivated)Invalid();} private static void Invalid()=>throw new BusinessRuleViolationException("platform.user.invalid_transition","현재 사용자 상태에서는 요청한 상태 변경을 수행할 수 없습니다.");
}
/// <summary>자원 범위는 테넌트와 선택적 조직·프로젝트 식별자로 지정합니다.</summary>
public sealed record ResourceScope
{
 public ResourceScope(TenantId tenant,OrganizationId? organization=null,ProjectId? project=null){ArgumentNullException.ThrowIfNull(tenant);if(project is not null&&organization is null)throw new ArgumentException("프로젝트 범위에는 조직 범위가 필요합니다.");TenantId=tenant;OrganizationId=organization;ProjectId=project;}
 public TenantId TenantId{get;} public OrganizationId? OrganizationId{get;} public ProjectId? ProjectId{get;}
}
/// <summary>자원은 명시적인 테넌트 범위에 귀속됩니다.</summary>
public sealed class Resource:AggregateRoot<ResourceId>
{
 private Resource(ResourceId id,ResourceScope scope,string type,string name):base(id){Scope=scope;ResourceType=Rules.Text(type,80);Name=Rules.Text(name,160);Status=ResourceStatus.Active;}
 public ResourceScope Scope{get;} public string ResourceType{get;} public string Name{get;private set;} public ResourceStatus Status{get;private set;}
 public static Resource Create(ResourceId id,ResourceScope scope,string type,string name){ArgumentNullException.ThrowIfNull(id);ArgumentNullException.ThrowIfNull(scope);return new(id,scope,type,name);}
 public void Rename(string name){Open();Name=Rules.Text(name,160);} public void Suspend(){if(Status!=ResourceStatus.Active)Invalid();Status=ResourceStatus.Suspended;} public void Activate(){if(Status!=ResourceStatus.Suspended)Invalid();Status=ResourceStatus.Active;} public void Retire(){Open();Status=ResourceStatus.Retired;}
 private void Open(){if(Status==ResourceStatus.Retired)Invalid();} private static void Invalid()=>throw new BusinessRuleViolationException("platform.resource.invalid_transition","폐기된 자원은 다시 변경할 수 없습니다.");
}
/// <summary>정책은 예상 버전이 현재 버전과 일치할 때만 변경됩니다.</summary>
public sealed class Policy:AggregateRoot<PolicyId>
{
 private Policy(PolicyId id,ResourceScope scope,string key,string value):base(id){Scope=scope;Key=Rules.Text(key,120);Value=Rules.Text(value,4000);Version=1;IsEnabled=true;}
 public ResourceScope Scope{get;} public string Key{get;} public string Value{get;private set;} public long Version{get;private set;} public bool IsEnabled{get;private set;}
 public static Policy Create(PolicyId id,ResourceScope scope,string key,string value){ArgumentNullException.ThrowIfNull(id);ArgumentNullException.ThrowIfNull(scope);return new(id,scope,key,value);}
 public void Update(string value,long expected){Check(expected);Value=Rules.Text(value,4000);Version++;} public void SetEnabled(bool enabled,long expected){Check(expected);if(IsEnabled==enabled)return;IsEnabled=enabled;Version++;}
 private void Check(long expected){if(expected!=Version)throw new BusinessRuleViolationException("platform.policy.version_conflict","정책 버전이 변경되었습니다. 최신 정책을 다시 조회해야 합니다.");}
}

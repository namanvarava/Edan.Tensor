using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.SharedKernel.Exceptions;
using Xunit;
namespace EDAN.Tensor.Platform.Domain.Tests;
public sealed class PlatformDomainTests
{
 private static TenantId T()=>new(Guid.NewGuid());
 [Fact] public void TenantLifecycleIsEnforced(){var t=Tenant.Create(T(),new("acme"),"에이씨미",DateTimeOffset.UtcNow);Assert.Equal(TenantStatus.Provisioning,t.Status);t.Activate();t.Suspend();t.Activate();t.Close();Assert.Throws<BusinessRuleViolationException>(()=>t.Activate());}
 [Fact] public void CodeIsNormalized()=>Assert.Equal("ACME-01",new TenantCode(" acme-01 ").Value);
 [Theory][InlineData("A")][InlineData("-AB")][InlineData("AB-")][InlineData("A--B")][InlineData("AB CD")] public void InvalidCodeIsRejected(string s)=>Assert.Throws<ArgumentException>(()=>new TenantCode(s));
 [Fact] public void OrganizationRetainsTenant(){var id=T();var o=Organization.Create(new(Guid.NewGuid()),id,new("OPS"),"운영");Assert.Equal(id,o.TenantId);}
 [Fact] public void ProjectRetainsOwnership(){var t=T();var org=new OrganizationId(Guid.NewGuid());var p=Project.Create(new(Guid.NewGuid()),t,org,new("CORE"),"핵심");Assert.Equal(t,p.TenantId);Assert.Equal(org,p.OrganizationId);}
 [Fact] public void ArchivedProjectCannotBeReactivated(){var p=Project.Create(new(Guid.NewGuid()),T(),new(Guid.NewGuid()),new("CORE"),"핵심");p.Archive();Assert.Throws<BusinessRuleViolationException>(()=>p.Activate());}
 [Fact] public void EmailIsNormalized()=>Assert.Equal("person@example.com",new EmailAddress(" Person@Example.com ").Value);
 [Fact] public void UserIsTenantScoped(){var t=T();var u=User.Invite(new(Guid.NewGuid()),t,new("person@example.com"),"사용자");Assert.Equal(t,u.TenantId);Assert.Equal(UserStatus.Invited,u.Status);}
 [Fact] public void ProjectResourceScopeRequiresOrganization()=>Assert.Throws<ArgumentException>(()=>new ResourceScope(T(),project:new ProjectId(Guid.NewGuid())));
 [Fact] public void RetiredResourceCannotBeReactivated(){var r=Resource.Create(new(Guid.NewGuid()),new(T()),"DataSet","기본 데이터");r.Retire();Assert.Throws<BusinessRuleViolationException>(()=>r.Activate());}
 [Fact] public void PolicyUsesOptimisticVersion(){var p=Policy.Create(new(Guid.NewGuid()),new(T()),"retention.days","30");p.Update("60",1);Assert.Equal(2,p.Version);Assert.Throws<BusinessRuleViolationException>(()=>p.Update("90",1));}
}
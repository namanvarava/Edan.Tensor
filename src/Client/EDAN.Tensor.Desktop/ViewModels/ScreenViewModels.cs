using System.IO;
using EDAN.Tensor.Client;
using EDAN.Tensor.Client.Contracts;
using EDAN.Tensor.Desktop.Infrastructure;

namespace EDAN.Tensor.Desktop.ViewModels;

public abstract class PlatformDirectoryScreenViewModel(IDesktopScreenService session, string screenId, string title, string description)
    : RegistryScreenViewModel(session, screenId, title, description)
{
    protected bool TryGetTenant(out Guid tenantId)
    {
        if (Session.Client is not null && Session.TenantId is Guid value)
        {
            tenantId = value;
            return true;
        }
        tenantId = Guid.Empty;
        AddField("조회 조건", "API 연결과 Tenant ID가 필요합니다.");
        StatusMessage = "Tenant 선택 전";
        return false;
    }

    protected void ShowError(TensorApiException ex)
    {
        AddField("오류 코드", ex.Code);
        AddField("요청 ID", ex.RequestId ?? "없음");
        StatusMessage = ex.Description;
    }
}

public sealed class TenantManagementViewModel(IDesktopScreenService session)
    : RegistryScreenViewModel(session, "PLT-TNT-001", "테넌트 관리", "테넌트 생성·활성화와 내 테넌트 소속을 관리합니다.")
{
    public override async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        Fields.Clear();
        if (Session.Client is null)
        {
            AddField("연결 상태", "API 연결 후 내 테넌트 소속을 조회합니다.");
            StatusMessage = "API 연결 전";
            return;
        }

        try
        {
            var memberships = await Session.Client.GetMyMembershipsAsync(cancellationToken);
            TargetOptions.Clear();
            AddField("활성 소속 수", memberships.Count.ToString());
            foreach (var membership in memberships)
            {
                AddField(membership.TenantId.ToString("D"), membership.Role);
                AddTargetOption(membership.TenantId, $"Tenant 선택: {membership.TenantId:D}");
            }
            StatusMessage = $"{memberships.Count}개의 활성 Tenant 소속을 조회했습니다.";
        }
        catch (TensorApiException ex)
        {
            AddField("오류 코드", ex.Code);
            AddField("요청 ID", ex.RequestId ?? "없음");
            StatusMessage = ex.Description;
        }
    }

    protected override async Task ExecuteCommandAsync()
    {
        Fields.Clear();
        if (Session.Client is null) { AddField("연결 상태", "API 연결이 필요합니다."); return; }
        try
        {
            if (TryGetTargetId(out var tenantId))
            {
                await Session.Client.ActivateTenantAsync(tenantId);
                StatusMessage = $"Tenant {tenantId:D} 활성화를 요청했습니다.";
            }
            else if (TryParsePayload<CreateTenantRequestContract>(out var request))
            {
                var result = await Session.Client.CreateTenantAsync(request!);
                LastResultIdText = result.Id.ToString("D");
                AddField("생성된 Tenant ID", result.Id.ToString("D"));
                StatusMessage = "Tenant 생성을 요청했습니다.";
            }
        }
        catch (Exception exception) { ShowException(exception); }
    }

    protected override async Task ExecuteSecondaryCommandAsync(string operation)
    {
        Fields.Clear();
        if (Session.Client is null)
        {
            AddField("연결 상태", "API 연결이 필요합니다.");
            StatusMessage = "API 연결 전";
            return;
        }
        try
        {
            if (operation == "tenant-directory")
            {
                if (!TryGetPage(out var pageNumber, out var pageSize)) return;
                var page = await Session.Client.GetTenantsAsync(SearchText, page: pageNumber, pageSize: pageSize);
                TargetOptions.Clear();
                AddField("페이지", $"{page.Page} / {page.PageSize}");
                AddField("전체 Tenant 수", page.TotalCount.ToString());
                foreach (var tenant in page.Items)
                {
                    AddField(tenant.Code, $"{tenant.Name} · {tenant.Status} · {tenant.CreatedAtUtc:O}");
                    AddTargetOption(tenant.Id, $"Tenant 선택: {tenant.Code}");
                }
                StatusMessage = $"{page.Items.Count}개의 Tenant directory 항목을 조회했습니다. PlatformAdministrator 권한이 필요합니다.";
                return;
            }

            if (operation == "tenant-update" && TryGetTargetId(out var tenantId)
                && TryParseExplicitPayload<UpdateDirectoryEntryRequestContract>(out var update))
            {
                await Session.Client.UpdateTenantAsync(tenantId, update!);
                StatusMessage = "Tenant 수정을 요청했습니다.";
            }
        }
        catch (Exception exception) { ShowException(exception); }
    }
}

public sealed class OrganizationManagementViewModel(IDesktopScreenService session)
    : PlatformDirectoryScreenViewModel(session, "PLT-ORG-001", "조직 관리", "선택된 테넌트의 조직과 lifecycle을 관리합니다.")
{
    public override async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId)) return;
        if (!TryGetPage(out var pageNumber, out var pageSize)) return;
        try
        {
            var page = await Session.Client!.GetOrganizationsAsync(tenantId, SearchText, page: pageNumber, pageSize: pageSize, cancellationToken: cancellationToken);
            TargetOptions.Clear();
            AddField("전체 조직 수", page.TotalCount.ToString());
            foreach (var item in page.Items)
            {
                AddField(item.Code, $"{item.Name} · {item.Status}");
                AddTargetOption(item.Id, $"조직 선택: {item.Code}");
            }
            StatusMessage = $"{page.Items.Count}개의 조직을 조회했습니다.";
        }
        catch (TensorApiException ex) { ShowError(ex); }
    }

    protected override async Task ExecuteCommandAsync()
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId) || !TryParsePayload<CreateOrganizationRequestContract>(out var request)) return;
        try
        {
            var result = await Session.Client!.CreateOrganizationAsync(tenantId, request!);
            LastResultIdText = result.Id.ToString("D");
            AddField("생성된 조직 ID", result.Id.ToString("D"));
            StatusMessage = "조직 생성을 요청했습니다.";
        }
        catch (Exception exception) { ShowException(exception); }
    }

    protected override async Task ExecuteSecondaryCommandAsync(string operation)
    {
        if (operation != "organization-update") return;
        Fields.Clear();
        if (!TryGetTenant(out var tenantId) || !TryGetTargetId(out var organizationId)
            || !TryParseExplicitPayload<UpdateDirectoryEntryRequestContract>(out var request)) return;
        try
        {
            await Session.Client!.UpdateOrganizationAsync(tenantId, organizationId, request!);
            StatusMessage = "조직 수정을 요청했습니다.";
        }
        catch (Exception exception) { ShowException(exception); }
    }
}

public sealed class ProjectManagementViewModel(IDesktopScreenService session)
    : PlatformDirectoryScreenViewModel(session, "PLT-PJT-001", "프로젝트 관리", "조직 범위의 프로젝트와 lifecycle을 관리합니다.")
{
    public override async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId)) return;
        if (!TryGetPage(out var pageNumber, out var pageSize)) return;
        try
        {
            var page = await Session.Client!.GetProjectsAsync(tenantId, search: SearchText, page: pageNumber, pageSize: pageSize, cancellationToken: cancellationToken);
            TargetOptions.Clear();
            AddField("전체 프로젝트 수", page.TotalCount.ToString());
            foreach (var item in page.Items)
            {
                AddField(item.Code, $"{item.Name} · {item.Status}");
                AddTargetOption(item.Id, $"프로젝트 선택: {item.Code}");
            }
            StatusMessage = $"{page.Items.Count}개의 프로젝트를 조회했습니다.";
        }
        catch (TensorApiException ex) { ShowError(ex); }
    }

    protected override async Task ExecuteCommandAsync()
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId) || !TryGetTargetId(out var organizationId)
            || !TryParsePayload<CreateProjectRequestContract>(out var request)) return;
        try
        {
            var result = await Session.Client!.CreateProjectAsync(tenantId, organizationId, request!);
            LastResultIdText = result.Id.ToString("D");
            AddField("생성된 프로젝트 ID", result.Id.ToString("D"));
            StatusMessage = "프로젝트 생성을 요청했습니다.";
        }
        catch (Exception exception) { ShowException(exception); }
    }

    protected override async Task ExecuteSecondaryCommandAsync(string operation)
    {
        if (operation != "project-update") return;
        Fields.Clear();
        if (!TryGetTenant(out var tenantId) || !TryGetTargetId(out var projectId)
            || !Guid.TryParse(GetInputText("OrganizationId"), out var organizationId)
            || organizationId == Guid.Empty
            || !TryParseExplicitPayload<UpdateDirectoryEntryRequestContract>(out var request))
        {
            AddField("프로젝트 수정 조건", "Tenant ID, Project ID, Organization ID와 Request DTO JSON이 필요합니다.");
            StatusMessage = "프로젝트 수정 조건 입력 전";
            return;
        }
        try
        {
            await Session.Client!.UpdateProjectAsync(tenantId, organizationId, projectId, request!);
            StatusMessage = "프로젝트 수정을 요청했습니다.";
        }
        catch (Exception exception) { ShowException(exception); }
    }
}

public sealed class UserInvitationManagementViewModel(IDesktopScreenService session)
    : PlatformDirectoryScreenViewModel(session, "PLT-USR-001", "사용자 초대 관리", "테넌트 사용자의 초대와 상태를 관리합니다.")
{
    public override async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId)) return;
        if (!TryGetPage(out var pageNumber, out var pageSize)) return;
        try
        {
            var page = await Session.Client!.GetUsersAsync(tenantId, SearchText, page: pageNumber, pageSize: pageSize, cancellationToken: cancellationToken);
            TargetOptions.Clear();
            AddField("전체 사용자 수", page.TotalCount.ToString());
            foreach (var item in page.Items)
            {
                AddField(item.Email, $"{item.DisplayName} · {item.Status}");
                AddTargetOption(item.Id, $"사용자 선택: {item.Email}");
            }
            StatusMessage = $"{page.Items.Count}명의 사용자를 조회했습니다.";
        }
        catch (TensorApiException ex) { ShowError(ex); }
    }

    protected override async Task ExecuteCommandAsync()
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId) || !TryParsePayload<InviteUserRequestContract>(out var request)) return;
        try
        {
            var result = await Session.Client!.InviteUserAsync(tenantId, request!);
            LastResultIdText = result.Id.ToString("D");
            AddField("생성된 사용자 ID", result.Id.ToString("D"));
            StatusMessage = "사용자 초대를 요청했습니다.";
        }
        catch (Exception exception) { ShowException(exception); }
    }

    protected override async Task ExecuteSecondaryCommandAsync(string operation)
    {
        if (operation != "user-update") return;
        Fields.Clear();
        if (!TryGetTenant(out var tenantId) || !TryGetTargetId(out var userId)
            || !TryParseExplicitPayload<UpdateDirectoryEntryRequestContract>(out var request)) return;
        try
        {
            await Session.Client!.UpdateUserAsync(tenantId, userId, request!);
            StatusMessage = "사용자 수정을 요청했습니다.";
        }
        catch (Exception exception) { ShowException(exception); }
    }
}

public sealed class MembershipManagementViewModel(IDesktopScreenService session)
    : PlatformDirectoryScreenViewModel(session, "PLT-MBR-001", "멤버십 관리", "테넌트·조직·프로젝트 멤버십을 관리합니다.")
{
    public override async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId)) return;
        try
        {
            var items = await Session.Client!.GetTenantMembershipsAsync(tenantId, cancellationToken);
            AddField("Tenant 멤버십 수", items.Count.ToString());
            foreach (var item in items)
                AddField(item.SubjectId, $"{item.Role} · {item.Status}");
            StatusMessage = $"{items.Count}개의 Tenant 멤버십을 조회했습니다.";
        }
        catch (TensorApiException ex) { ShowError(ex); }
    }

    protected override async Task ExecuteCommandAsync()
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId) || !TryParsePayload<GrantTenantMembershipRequestContract>(out var request)) return;
        try
        {
            var result = await Session.Client!.GrantTenantMembershipAsync(tenantId, request!);
            LastResultIdText = result.Id.ToString("D");
            AddField("생성된 멤버십 ID", result.Id.ToString("D"));
            StatusMessage = "Tenant 멤버십 부여를 요청했습니다.";
        }
        catch (Exception exception) { ShowException(exception); }
    }

    protected override async Task ExecuteSecondaryCommandAsync(string operation)
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId)) return;
        try
        {
            switch (operation)
            {
                case "organization-membership-grant":
                    if (!Guid.TryParse(GetInputText("OrganizationId"), out var organizationId)
                        || organizationId == Guid.Empty
                        || !TryParsePayload<GrantOrganizationMembershipRequestContract>(out var organizationRequest))
                    {
                        AddField("조직 멤버십 조건", "Organization ID와 SubjectId/Role 입력이 필요합니다.");
                        StatusMessage = "조직 멤버십 조건 입력 전";
                        return;
                    }
                    var organizationMembership = await Session.Client!.GrantOrganizationMembershipAsync(
                        tenantId, organizationId, organizationRequest!);
                    LastResultIdText = organizationMembership.Id.ToString("D");
                    AddField("생성된 멤버십 ID", organizationMembership.Id.ToString("D"));
                    StatusMessage = "조직 멤버십 부여를 요청했습니다.";
                    break;
                case "project-membership-grant":
                    if (!Guid.TryParse(GetInputText("OrganizationId"), out var projectOrganizationId)
                        || projectOrganizationId == Guid.Empty
                        || !Guid.TryParse(GetInputText("ProjectId"), out var projectId)
                        || projectId == Guid.Empty
                        || !TryParsePayload<GrantProjectMembershipRequestContract>(out var projectRequest))
                    {
                        AddField("프로젝트 멤버십 조건", "Organization ID, Project ID와 SubjectId/Role 입력이 필요합니다.");
                        StatusMessage = "프로젝트 멤버십 조건 입력 전";
                        return;
                    }
                    var projectMembership = await Session.Client!.GrantProjectMembershipAsync(
                        tenantId, projectOrganizationId, projectId, projectRequest!);
                    LastResultIdText = projectMembership.Id.ToString("D");
                    AddField("생성된 멤버십 ID", projectMembership.Id.ToString("D"));
                    StatusMessage = "프로젝트 멤버십 부여를 요청했습니다.";
                    break;
            }
        }
        catch (Exception exception) { ShowException(exception); }
    }
}

public sealed class AuthorizationPolicyManagementViewModel(IDesktopScreenService session)
    : PlatformDirectoryScreenViewModel(session, "PLT-AUTH-001", "권한 정책 관리", "테넌트 범위 권한 정책을 관리합니다.")
{
    public override async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId)) return;
        try
        {
            var policies = await Session.Client!.GetAuthorizationPoliciesAsync(tenantId, cancellationToken);
            TargetOptions.Clear();
            AddField("정책 수", policies.Count.ToString());
            foreach (var policy in policies)
            {
                AddField(policy.Action, $"{policy.SubjectId} · {policy.ResourceType} · {policy.Effect} · {(policy.IsEnabled ? "활성" : "비활성")}");
                AddTargetOption(policy.Id, $"정책 선택: {policy.Action}");
            }
            StatusMessage = $"{policies.Count}개의 권한 정책을 조회했습니다. Tenant 관리자 권한이 필요합니다.";
        }
        catch (TensorApiException ex) { ShowError(ex); }
    }

    protected override async Task ExecuteCommandAsync()
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId)) return;
        try
        {
            if (TryGetTargetId(out var policyId))
            {
                if (!TryParsePayload<UpdateAuthorizationPolicyRequestContract>(out var update)) return;
                await Session.Client!.UpdateAuthorizationPolicyAsync(tenantId, policyId, update!);
                StatusMessage = "권한 정책 수정을 요청했습니다.";
            }
            else
            {
                if (!TryParsePayload<CreateAuthorizationPolicyRequestContract>(out var create)) return;
                var result = await Session.Client!.CreateAuthorizationPolicyAsync(tenantId, create!);
                LastResultIdText = result.Id.ToString("D");
                AddField("생성된 정책 ID", result.Id.ToString("D"));
                StatusMessage = "권한 정책 생성을 요청했습니다.";
            }
        }
        catch (Exception exception) { ShowException(exception); }
    }

    protected override async Task ExecuteSecondaryCommandAsync(string operation)
    {
        Fields.Clear();
        if (operation != "policy-enabled" || !TryGetTenant(out var tenantId)
            || !TryGetTargetId(out var policyId)
            || !TryParseExplicitPayload<SetAuthorizationPolicyEnabledRequestContract>(out var request)) return;
        try
        {
            await Session.Client!.SetAuthorizationPolicyEnabledAsync(tenantId, policyId, request!);
            StatusMessage = "권한 정책 활성화 상태 변경을 요청했습니다.";
        }
        catch (Exception exception) { ShowException(exception); }
    }
}

public sealed class AuthorizationAuditManagementViewModel(IDesktopScreenService session)
    : PlatformDirectoryScreenViewModel(session, "PLT-AUD-001", "권한 감사 관리", "권한 감사 조회와 무결성·보존 운영을 관리합니다.")
{
    public override async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId)) return;
        if (!TryGetPage(out var pageNumber, out var pageSize)) return;
        var action = GetInputText("Action");
        bool? isAllowed = null;
        if (!string.IsNullOrWhiteSpace(GetInputText("IsAllowed")))
        {
            if (!bool.TryParse(GetInputText("IsAllowed"), out var parsedAllowed))
            {
                AddField("필터 형식", "Is allowed는 true 또는 false여야 합니다.");
                StatusMessage = "감사 필터 오류";
                return;
            }
            isAllowed = parsedAllowed;
        }
        DateTimeOffset? fromUtc = null;
        DateTimeOffset? toUtc = null;
        var fromText = GetInputText("FromUtc");
        var toText = GetInputText("ToUtc");
        if (!string.IsNullOrWhiteSpace(fromText) || !string.IsNullOrWhiteSpace(toText))
        {
            if (!DateTimeOffset.TryParse(fromText, out var parsedFrom)
                || !DateTimeOffset.TryParse(toText, out var parsedTo))
            {
                AddField("기간 형식", "From/To UTC는 함께 입력하는 ISO-8601 시각이어야 합니다.");
                StatusMessage = "감사 기간 필터 오류";
                return;
            }
            fromUtc = parsedFrom;
            toUtc = parsedTo;
        }
        try
        {
            var page = await Session.Client!.GetAuthorizationAuditAsync(tenantId, subjectId: SearchText,
                action: string.IsNullOrWhiteSpace(action) ? null : action,
                isAllowed: isAllowed, fromUtc: fromUtc, toUtc: toUtc,
                page: pageNumber, pageSize: pageSize, cancellationToken: cancellationToken);
            AddField("페이지", $"{page.Page} / {page.PageSize}");
            AddField("전체 감사 건수", page.TotalCount.ToString());
            foreach (var entry in page.Items)
                AddField(entry.Action, $"{entry.SubjectId} · {(entry.IsAllowed ? "허용" : "거부")} · {entry.ReasonCode} · {entry.OccurredAtUtc:O}");
            StatusMessage = $"{page.Items.Count}개의 감사 기록을 조회했습니다. 필터는 API 계약의 subjectId/action/isAllowed/fromUtc/toUtc를 사용합니다.";
        }
        catch (TensorApiException ex) { ShowError(ex); }
    }

    protected override async Task ExecuteCommandAsync()
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId) || !TryParsePayload<VerifyAuthorizationAuditIntegrityRequestContract>(out var request)) return;
        try
        {
            var result = await Session.Client!.VerifyAuthorizationAuditIntegrityAsync(tenantId, request!);
            AddField("검증 결과", result.IsValid ? "일치" : "불일치");
            AddField("실제 SHA-256", result.Current.Sha256);
            AddField("레코드 수", result.Current.RecordCount.ToString());
            StatusMessage = "감사 무결성 검증을 완료했습니다.";
        }
        catch (Exception exception) { ShowException(exception); }
    }

    protected override async Task ExecuteSecondaryCommandAsync(string operation)
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId)) return;
        try
        {
            switch (operation)
            {
                case "audit-archive":
                    if (!TryParsePayload<CreateAuthorizationAuditArchiveRequestContract>(out var archiveRequest)) return;
                    var archive = await Session.Client!.CreateAuthorizationAuditArchiveAsync(tenantId, archiveRequest!);
                    LastResultIdText = archive.ArchiveId.ToString("D");
                    AddField("Archive ID", archive.ArchiveId.ToString("D"));
                    AddField("SHA-256", archive.Sha256);
                    AddField("레코드 수", archive.RecordCount.ToString());
                    StatusMessage = "감사 아카이브 생성을 요청했습니다.";
                    break;
                case "audit-archive-job":
                    if (!TryParsePayload<CreateAuthorizationAuditArchiveRequestContract>(out var jobRequest)) return;
                    var job = await Session.Client!.CreateAuthorizationAuditArchiveJobAsync(tenantId, jobRequest!);
                    LastResultIdText = job.JobId.ToString("D");
                    AddField("Archive Job ID", job.JobId.ToString("D"));
                    AddField("상태", job.Status);
                    StatusMessage = "감사 아카이브 작업 생성을 요청했습니다.";
                    break;
                case "audit-archive-job-get":
                    if (!TryGetTargetId(out var jobId)) return;
                    var jobDetails = await Session.Client!.GetAuthorizationAuditArchiveJobAsync(tenantId, jobId);
                    if (jobDetails is null)
                    {
                        AddField("Archive Job", "해당 Tenant 범위의 작업을 찾지 못했습니다.");
                        StatusMessage = "감사 아카이브 작업 없음";
                        return;
                    }
                    AddField("Archive Job ID", jobDetails.Job.JobId.ToString("D"));
                    AddField("상태", jobDetails.Job.Status);
                    AddField("시도 수", jobDetails.Job.AttemptCount.ToString());
                    AddField("최근 오류", jobDetails.Job.FailureReason ?? "없음");
                    StatusMessage = "감사 아카이브 작업 상세를 조회했습니다.";
                    break;
                case "audit-archive-job-retry":
                    if (!TryGetTargetId(out var retryJobId)) return;
                    var retried = await Session.Client!.RetryAuthorizationAuditArchiveJobAsync(tenantId, retryJobId);
                    AddField("Archive Job ID", retried.JobId.ToString("D"));
                    AddField("상태", retried.Status);
                    StatusMessage = "감사 아카이브 작업 재시도를 요청했습니다.";
                    break;
                case "audit-archive-job-verify":
                    if (!TryGetTargetId(out var verifyJobId)
                        || !TryParseExplicitPayload<VerifyAuthorizationAuditArchiveRequestContract>(out var verifyRequest)) return;
                    var verified = await Session.Client!.VerifyAuthorizationAuditArchiveJobAsync(tenantId, verifyJobId, verifyRequest!);
                    AddField("검증 결과", verified.IsValid ? "유효" : "불일치");
                    AddField("Reason", verified.ReasonCode);
                    StatusMessage = "감사 아카이브 작업 검증을 완료했습니다.";
                    break;
                case "audit-schedule":
                    var schedule = string.IsNullOrWhiteSpace(CommandPayloadText)
                        ? await Session.Client!.GetAuthorizationAuditArchiveScheduleAsync(tenantId)
                        : await Session.Client!.UpdateAuthorizationAuditArchiveScheduleAsync(tenantId,
                            ParseRequiredPayload<UpdateAuthorizationAuditArchiveScheduleRequestContract>());
                    AddField("주기", $"{schedule.IntervalHours}시간");
                    AddField("활성 상태", schedule.IsEnabled ? "활성" : "비활성");
                    AddField("마지막 실행", schedule.LastRunAtUtc?.ToString("O") ?? "없음");
                    StatusMessage = string.IsNullOrWhiteSpace(CommandPayloadText)
                        ? "감사 아카이브 보관 스케줄을 조회했습니다."
                        : "감사 아카이브 보관 스케줄을 갱신했습니다.";
                    break;
                case "audit-retention":
                    var retention = string.IsNullOrWhiteSpace(CommandPayloadText)
                        ? await Session.Client!.GetAuthorizationAuditRetentionAsync(tenantId)
                        : await Session.Client!.UpdateAuthorizationAuditRetentionAsync(tenantId,
                            ParseRequiredPayload<UpdateAuthorizationAuditRetentionRequestContract>());
                    AddField("보존 기간", $"{retention.RetentionDays}일");
                    AddField("Legal hold", retention.IsLegalHold ? "활성" : "비활성");
                    AddField("변경 주체", retention.UpdatedBy ?? "없음");
                    StatusMessage = string.IsNullOrWhiteSpace(CommandPayloadText)
                        ? "감사 보존 정책을 조회했습니다."
                        : "감사 보존 정책을 갱신했습니다.";
                    break;
            }
        }
        catch (Exception exception) { ShowException(exception); }
    }

    private T ParseRequiredPayload<T>()
    {
        if (!TryParseExplicitPayload<T>(out var result) || result is null)
            throw new InvalidOperationException("Request DTO JSON이 필요합니다.");
        return result;
    }
}

public sealed class StorageQuotaManagementViewModel(IDesktopScreenService session)
    : PlatformDirectoryScreenViewModel(session, "PLT-QTA-001", "저장 용량 할당량 관리", "저장 용량 정책과 변경 이력을 관리합니다.")
{
    public override async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId)) return;
        if (!TryGetPage(out var pageNumber, out var pageSize)) return;
        try
        {
            var page = await Session.Client!.GetStorageQuotaPoliciesAsync(tenantId, pageNumber, pageSize, cancellationToken);
            TargetOptions.Clear();
            AddField("페이지", $"{page.Page} / {page.PageSize}");
            AddField("전체 정책 수", page.TotalCount.ToString());
            foreach (var policy in page.Items)
            {
                AddField(policy.Scope, $"{policy.TargetId:D} · {policy.PlanCode} · {(policy.IsEnabled ? "활성" : "비활성")} · v{policy.Version}");
                AddTargetOption(policy.Id, $"Quota 선택: {policy.Scope} {policy.TargetId:D}");
            }
            StatusMessage = $"{page.Items.Count}개의 Quota 정책을 조회했습니다. Tenant 관리자 권한이 필요합니다.";
        }
        catch (TensorApiException ex) { ShowError(ex); }
    }

    protected override async Task ExecuteCommandAsync()
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId) || !TryParsePayload<CreateStorageQuotaPolicyRequestContract>(out var request)) return;
        try
        {
            var result = await Session.Client!.CreateStorageQuotaPolicyAsync(tenantId, request!);
            LastResultIdText = result.Id.ToString("D");
            AddField("생성된 정책 ID", result.Id.ToString("D"));
            AddField("버전", result.Version.ToString());
            StatusMessage = "저장 용량 정책 생성을 요청했습니다.";
        }
        catch (Exception exception) { ShowException(exception); }
    }

    protected override async Task ExecuteSecondaryCommandAsync(string operation)
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId) || !TryGetTargetId(out var policyId)) return;
        try
        {
            switch (operation)
            {
                case "quota-update":
                    if (!TryParseExplicitPayload<UpdateStorageQuotaPolicyRequestContract>(out var update)) return;
                    var updated = await Session.Client!.UpdateStorageQuotaPolicyAsync(tenantId, policyId, update!);
                    LastResultIdText = updated.Id.ToString("D");
                    AddField("버전", updated.Version.ToString());
                    StatusMessage = "Quota 정책 수정을 요청했습니다.";
                    break;
                case "quota-enabled":
                    if (!TryParseExplicitPayload<SetStorageQuotaPolicyEnabledRequestContract>(out var enabled)) return;
                    var enabledPolicy = await Session.Client!.SetStorageQuotaPolicyEnabledAsync(tenantId, policyId, enabled!);
                    AddField("활성 상태", enabledPolicy.IsEnabled ? "활성" : "비활성");
                    AddField("버전", enabledPolicy.Version.ToString());
                    StatusMessage = "Quota 정책 활성화 상태 변경을 요청했습니다.";
                    break;
                case "quota-history":
                    if (!TryGetPage(out var pageNumber, out var pageSize)) return;
                    var history = await Session.Client!.GetStorageQuotaPolicyHistoryAsync(tenantId, policyId, pageNumber, pageSize);
                    AddField("페이지", $"{history.Page} / {history.PageSize}");
                    AddField("전체 이력 수", history.TotalCount.ToString());
                    foreach (var item in history.Items)
                        AddField(item.ChangeType, $"{item.ActorSubjectId} · {item.OccurredAtUtc:O}");
                    StatusMessage = $"{history.Items.Count}개의 Quota 변경 이력을 조회했습니다.";
                    break;
            }
        }
        catch (Exception exception) { ShowException(exception); }
    }
}

public sealed class DataSetManagementViewModel(IDesktopScreenService session)
    : PlatformDirectoryScreenViewModel(session, "DSET-DSM-001", "데이터셋 관리", "데이터셋 metadata와 content lifecycle을 관리합니다.")
{
    public override async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId) || !TryGetTargetId(out var dataSetId))
        {
            AddField("조회 조건", "Tenant ID와 대상 ID에 DataSet ID를 입력하면 실제 metadata를 조회합니다.");
            StatusMessage = "DataSet ID 입력 전";
            return;
        }
        try
        {
            var dataSet = await Session.Client!.GetDataSetAsync(tenantId, dataSetId, cancellationToken);
            if (dataSet is null)
            {
                AddField("결과", "선택한 Tenant에서 DataSet을 찾을 수 없습니다.");
                StatusMessage = "DataSet 없음";
                return;
            }
            AddField("이름", dataSet.Name);
            AddField("범위", $"Organization {dataSet.OrganizationId:D} / Project {dataSet.ProjectId:D}");
            AddField("상태", dataSet.Status);
            AddField("버전", dataSet.Version.ToString());
            AddField("스키마", dataSet.CurrentSchemaVersion?.ToString() ?? "미등록");
            AddField("저장 어댑터", $"{dataSet.StorageAdapter} / {dataSet.ContentFormat}");
            StatusMessage = "DataSet metadata를 조회했습니다.";
        }
        catch (TensorApiException ex) { ShowError(ex); }
    }

    protected override async Task ExecuteCommandAsync()
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId)) return;
        try
        {
            if (TryGetTargetId(out var dataSetId))
            {
                await Session.Client!.ActivateDataSetAsync(tenantId, dataSetId);
                StatusMessage = "DataSet 활성화를 요청했습니다.";
            }
            else if (TryParsePayload<CreateDataSetRequestContract>(out var request))
            {
                var result = await Session.Client!.CreateDataSetAsync(tenantId, request!);
                LastResultIdText = result.Id.ToString("D");
                AddField("생성된 DataSet ID", result.Id.ToString("D"));
                StatusMessage = "DataSet 생성을 요청했습니다.";
            }
        }
        catch (Exception exception) { ShowException(exception); }
    }
}

public sealed class DataSetSchemaManagementViewModel(IDesktopScreenService session)
    : PlatformDirectoryScreenViewModel(session, "DSET-SCHM-001", "데이터셋 스키마 관리", "데이터셋 schema revision을 관리합니다.")
{
    public override async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId) || !TryGetTargetId(out var dataSetId))
        {
            AddField("조회 조건", "Tenant ID와 DataSet ID를 입력하면 불변 schema revision 목록을 조회합니다.");
            StatusMessage = "DataSet ID 입력 전";
            return;
        }
        try
        {
            var dataSet = await Session.Client!.GetDataSetAsync(tenantId, dataSetId, cancellationToken);
            if (dataSet is null) { AddField("결과", "DataSet을 찾을 수 없습니다."); StatusMessage = "DataSet 없음"; return; }
            AddField("현재 schema version", dataSet.CurrentSchemaVersion?.ToString() ?? "미등록");
            foreach (var revision in dataSet.SchemaRevisions)
                AddField($"Revision {revision.Version}", $"{revision.CreatedAtUtc:O} · {revision.SchemaJson}");
            StatusMessage = $"{dataSet.SchemaRevisions.Count}개의 schema revision을 조회했습니다.";
        }
        catch (TensorApiException ex) { ShowError(ex); }
    }

    protected override async Task ExecuteCommandAsync()
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId) || !TryGetTargetId(out var dataSetId)
            || !TryParsePayload<AddDataSetSchemaRequestContract>(out var request)) return;
        try
        {
            var result = await Session.Client!.AddDataSetSchemaAsync(tenantId, dataSetId, request!);
            LastResultIdText = result.DataSetId.ToString("D");
            AddField("등록된 schema version", result.SchemaVersion.ToString());
            StatusMessage = "DataSet schema revision 등록을 요청했습니다.";
        }
        catch (Exception exception) { ShowException(exception); }
    }
}

public sealed class DataSetStorageManagementViewModel(IDesktopScreenService session)
    : PlatformDirectoryScreenViewModel(session, "DSET-STO-001", "데이터셋 저장소 관리", "콘텐츠와 storage usage를 관리합니다.")
{
    public override async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId)) return;
        try
        {
            var tenantUsage = await Session.Client!.GetTenantStorageUsageAsync(tenantId, cancellationToken);
            AddField("Tenant 사용량", $"{tenantUsage.TenantBytes:N0} bytes / DataSet {tenantUsage.TenantDataSetCount}");
            if (TryGetTargetId(out var dataSetId))
            {
                var usage = await Session.Client.GetDataSetStorageUsageAsync(tenantId, dataSetId, cancellationToken);
                AddField("DataSet 사용량", $"{usage.DataSetBytes:N0} bytes");
                AddField("조직·프로젝트 사용량", $"{usage.OrganizationBytes:N0} / {usage.ProjectBytes:N0} bytes");
                AddField("경고", usage.Warnings.Count == 0 ? "없음" : string.Join(", ", usage.Warnings));
            }
            var take = int.TryParse(PageSizeText, out var parsedTake) ? parsedTake : 10;
            if (take is < 1 or > 744)
            {
                AddField("이력 범위", "페이지 크기 입력은 1~744 범위여야 합니다.");
                StatusMessage = "저장량 이력 범위 오류";
                return;
            }
            var history = await Session.Client.GetTenantStorageUsageHistoryAsync(tenantId, take, cancellationToken);
            AddField("최근 스냅샷", history.Count.ToString());
            StatusMessage = "Tenant 및 선택 DataSet의 실제 저장소 사용량을 조회했습니다.";
        }
        catch (TensorApiException ex) { ShowError(ex); }
    }

    protected override async Task ExecuteSecondaryCommandAsync(string operation)
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId) || !TryGetTargetId(out var dataSetId))
        {
            AddField("콘텐츠 조건", "Tenant ID와 DataSet ID가 필요합니다.");
            StatusMessage = "DataSet ID 입력 전";
            return;
        }

        try
        {
            switch (operation)
            {
                case "dataset-content-upload":
                    var uploadPath = GetInputText("ContentPath");
                    if (string.IsNullOrWhiteSpace(uploadPath) || !File.Exists(uploadPath))
                    {
                        AddField("파일 경로", "업로드할 로컬 파일 경로가 필요합니다.");
                        StatusMessage = "업로드 파일 없음";
                        return;
                    }
                    await using (var input = File.OpenRead(uploadPath))
                        await Session.Client!.WriteDataSetContentAsync(tenantId, dataSetId, input);
                    AddField("업로드 파일", uploadPath);
                    StatusMessage = "DataSet 콘텐츠 업로드를 완료했습니다.";
                    break;
                case "dataset-content-download":
                    var downloadPath = GetInputText("ContentPath");
                    if (string.IsNullOrWhiteSpace(downloadPath))
                    {
                        AddField("파일 경로", "다운로드할 로컬 파일 경로가 필요합니다.");
                        StatusMessage = "다운로드 경로 입력 전";
                        return;
                    }
                    await using (var content = await Session.Client!.OpenDataSetContentAsync(tenantId, dataSetId))
                    await using (var output = File.Create(downloadPath))
                        await content.CopyToAsync(output);
                    AddField("다운로드 파일", downloadPath);
                    StatusMessage = "DataSet 콘텐츠 다운로드를 완료했습니다.";
                    break;
                case "quota-events":
                    var events = await Session.Client!.GetTenantQuotaEventHistoryAsync(tenantId, 100);
                    AddField("Quota 이벤트 수", events.Count.ToString());
                    foreach (var item in events)
                        AddField(item.EventType, $"{item.Scope} · {item.UsedBytes:N0}/{item.LimitBytes:N0} · {item.OccurredAtUtc:O}");
                    StatusMessage = "Quota 이벤트 이력을 조회했습니다.";
                    break;
            }
        }
        catch (Exception exception) { ShowException(exception); }
    }
}

public sealed class DataSetValidationManagementViewModel(IDesktopScreenService session)
    : PlatformDirectoryScreenViewModel(session, "DSET-VLD-001", "데이터셋 검증 관리", "검증 profile·revision·execution을 관리합니다.")
{
    public override async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId) || !TryGetTargetId(out var executionId))
        {
            AddField("조회 조건", "Tenant ID와 검증 실행 ID를 입력하면 실행 상태와 상세 결과를 조회합니다.");
            StatusMessage = "검증 실행 ID 입력 전";
            return;
        }
        try
        {
            var execution = await Session.Client!.GetDataSetValidationAsync(tenantId, executionId, cancellationToken);
            if (execution is null) { AddField("결과", "검증 실행을 찾을 수 없습니다."); StatusMessage = "검증 실행 없음"; return; }
            AddField("상태", execution.Status);
            AddField("진행", $"{execution.ProcessedRecords} / {execution.TotalRecords}");
            AddField("오류·경고", $"{execution.ErrorCount} / {execution.WarningCount}");
            AddField("DataSet·Schema", $"{execution.DataSetId:D} / v{execution.SchemaVersion}");
            var details = await Session.Client.GetDataSetValidationDetailsAsync(tenantId, executionId, take: 20, cancellationToken: cancellationToken);
            foreach (var detail in details.Items)
                AddField($"행 {detail.RowNumber}", $"{detail.Severity} · {detail.Code} · {detail.Message}");
            StatusMessage = $"검증 실행과 상세 결과 {details.Items.Count}건을 조회했습니다.";
        }
        catch (TensorApiException ex) { ShowError(ex); }
    }

    protected override async Task ExecuteCommandAsync()
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId) || !TryGetTargetId(out var dataSetId)
            || !TryParsePayload<CreateDataSetValidationRequestContract>(out var request)
            || !TryGetRequestKey(out var key)) return;
        try
        {
            var result = await Session.Client!.CreateDataSetValidationAsync(tenantId, dataSetId, request!, key);
            LastResultIdText = result.Execution.Id.ToString("D");
            AddField("검증 실행 ID", result.Execution.Id.ToString("D"));
            AddField("Worker Task ID", result.WorkerTaskId.ToString("D"));
            AddField("생성됨", result.Created ? "예" : "기존 실행 재사용");
            StatusMessage = "DataSet 검증 실행을 요청했습니다.";
        }
        catch (Exception exception) { ShowException(exception); }
    }

    protected override async Task ExecuteSecondaryCommandAsync(string operation)
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId)) return;
        try
        {
            switch (operation)
            {
                case "validation-profile":
                    if (!TryParseExplicitPayload<CreateDataSetValidationProfileRequestContract>(out var profileRequest)) return;
                    var profile = await Session.Client!.CreateDataSetValidationProfileAsync(tenantId, profileRequest!);
                    LastResultIdText = profile.Id.ToString("D");
                    AddField("Profile ID", profile.Id.ToString("D"));
                    AddField("Revision", profile.Revision.ToString());
                    StatusMessage = "검증 프로필을 생성했습니다.";
                    break;
                case "validation-profile-revision":
                    if (!TryGetTargetId(out var profileId)
                        || !TryParseExplicitPayload<CreateDataSetValidationRevisionRequestContract>(out var revisionRequest)) return;
                    var revision = await Session.Client!.AddDataSetValidationProfileRevisionAsync(tenantId, profileId, revisionRequest!);
                    LastResultIdText = revision.Id.ToString("D");
                    AddField("Profile ID", revision.Id.ToString("D"));
                    AddField("Revision", revision.Revision.ToString());
                    StatusMessage = "검증 프로필 revision을 추가했습니다.";
                    break;
                case "validation-delete":
                    if (!TryGetTargetId(out var executionId)) return;
                    await Session.Client!.DeleteDataSetValidationAsync(tenantId, executionId);
                    StatusMessage = "검증 실행과 보존 상세를 삭제했습니다.";
                    break;
            }
        }
        catch (Exception exception) { ShowException(exception); }
    }
}

public sealed class WorkerTaskMonitoringViewModel(IDesktopScreenService session)
    : PlatformDirectoryScreenViewModel(session, "WRK-TSK-001", "Worker 작업 모니터링", "비동기 작업 상태와 진행률을 모니터링합니다.")
{
    public override async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId) || !TryGetTargetId(out var taskId))
        {
            AddField("조회 조건", "Tenant ID와 Worker 작업 ID를 입력하면 Lease·진행 상태를 조회합니다.");
            StatusMessage = "Worker 작업 ID 입력 전";
            return;
        }
        try
        {
            var task = await Session.Client!.GetWorkerTaskAsync(tenantId, taskId, cancellationToken);
            if (task is null) { AddField("결과", "Tenant 범위에서 Worker 작업을 찾을 수 없습니다."); StatusMessage = "작업 없음"; return; }
            AddField("유형·상태", $"{task.TaskType} · {task.Status}");
            AddField("진행률", $"{task.ProgressPercent}% · {task.ProgressStage ?? "단계 없음"}");
            AddField("시도", $"{task.AttemptCount} / {task.MaxAttempts}");
            AddField("Fencing·Checkpoint", $"{task.FencingToken} / {task.CheckpointVersion}");
            AddField("메시지", task.ProgressMessage ?? "없음");
            StatusMessage = "Worker 작업 상태를 조회했습니다.";
        }
        catch (TensorApiException ex) { ShowError(ex); }
    }

    protected override async Task ExecuteCommandAsync()
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId) || !TryParsePayload<EnqueueWorkerTaskRequestContract>(out var request)
            || !TryGetRequestKey(out var key)) return;
        try
        {
            var result = await Session.Client!.EnqueueWorkerTaskAsync(tenantId, request!, key);
            LastResultIdText = result.Id.ToString("D");
            AddField("생성된 작업 ID", result.Id.ToString("D"));
            AddField("상태", result.Status);
            StatusMessage = "Worker 작업 등록을 요청했습니다.";
        }
        catch (Exception exception) { ShowException(exception); }
    }
}

public sealed class DeadLetterQueueManagementViewModel(IDesktopScreenService session)
    : PlatformDirectoryScreenViewModel(session, "WRK-DLQ-001", "실패 작업 관리", "Dead Letter Queue 작업을 조회하고 replay합니다.")
{
    public override async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId)) return;
        if (!TryGetPage(out var pageNumber, out var pageSize)) return;

        try
        {
            var page = await Session.Client!.GetWorkerDeadLettersAsync(tenantId, pageNumber, pageSize, cancellationToken);
            TargetOptions.Clear();
            AddField("페이지", $"{page.Page} / {page.PageSize}");
            AddField("DLQ 항목 수", page.Items.Count.ToString());
            foreach (var item in page.Items)
            {
                AddField(item.Id.ToString("D"), $"{item.TaskType} · {item.LastErrorCode ?? "오류 코드 없음"}");
                AddTargetOption(item.Id, $"DLQ 선택: {item.TaskType}");
            }
            StatusMessage = $"{page.Items.Count}개의 DLQ 작업을 조회했습니다.";
        }
        catch (TensorApiException ex) { ShowError(ex); }
    }

    protected override async Task ExecuteCommandAsync()
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId) || !TryGetTargetId(out var taskId)
            || !Guid.TryParse(IdempotencyKeyText.Trim(), out var replayRequestId)
            || replayRequestId == Guid.Empty)
        {
            AddField("Replay 입력", "대상 ID에는 DLQ Task ID, Replay 키에는 GUID를 입력하세요.");
            StatusMessage = "Replay 입력 오류";
            return;
        }
        try
        {
            var result = await Session.Client!.ReplayWorkerDeadLetterAsync(tenantId, taskId, replayRequestId);
            AddField("원본 작업 ID", result.OriginalTaskId.ToString("D"));
            AddField("Replay 작업 ID", result.ReplayTaskId.ToString("D"));
            StatusMessage = "DLQ replay를 요청했습니다.";
        }
        catch (Exception exception) { ShowException(exception); }
    }
}

public sealed class ExternalDeliveryMonitoringViewModel(IDesktopScreenService session)
    : PlatformDirectoryScreenViewModel(session, "WRK-XDL-001", "외부 전달 모니터링", "외부 전달 상태와 오류를 모니터링합니다.")
{
    public override async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId) || !TryGetTargetId(out var deliveryId))
        {
            AddField("조회 조건", "Tenant ID와 외부 전달 ID를 입력하면 전달 상태를 조회합니다.");
            StatusMessage = "외부 전달 ID 입력 전";
            return;
        }
        try
        {
            var delivery = await Session.Client!.GetExternalDeliveryAsync(tenantId, deliveryId, cancellationToken);
            if (delivery is null) { AddField("결과", "Tenant 범위에서 외부 전달을 찾을 수 없습니다."); StatusMessage = "전달 없음"; return; }
            AddField("대상 URI", delivery.TargetUri);
            AddField("상태", delivery.Status);
            AddField("시도", $"{delivery.AttemptCount} / {delivery.MaxAttempts}");
            AddField("오류 코드", delivery.LastErrorCode ?? "없음");
            AddField("생성 시각", delivery.CreatedAtUtc.ToString("O"));
            StatusMessage = "외부 전달 상태를 조회했습니다.";
        }
        catch (TensorApiException ex) { ShowError(ex); }
    }

    protected override async Task ExecuteCommandAsync()
    {
        Fields.Clear();
        if (!TryGetTenant(out var tenantId) || !TryParsePayload<CreateExternalDeliveryRequestContract>(out var request)
            || !TryGetRequestKey(out var key)) return;
        try
        {
            var result = await Session.Client!.CreateExternalDeliveryAsync(tenantId, request!, key);
            LastResultIdText = result.Delivery.Id.ToString("D");
            AddField("전달 ID", result.Delivery.Id.ToString("D"));
            AddField("Worker Task ID", result.WorkerTaskId.ToString("D"));
            AddField("생성됨", result.Created ? "예" : "기존 전달 재사용");
            StatusMessage = "외부 전달 생성을 요청했습니다.";
        }
        catch (Exception exception) { ShowException(exception); }
    }
}

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
            AddField("활성 소속 수", memberships.Count.ToString());
            foreach (var membership in memberships)
                AddField(membership.TenantId.ToString("D"), membership.Role);
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
                AddField("생성된 Tenant ID", result.Id.ToString("D"));
                StatusMessage = "Tenant 생성을 요청했습니다.";
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
            AddField("전체 조직 수", page.TotalCount.ToString());
            foreach (var item in page.Items)
                AddField(item.Code, $"{item.Name} · {item.Status}");
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
            AddField("생성된 조직 ID", result.Id.ToString("D"));
            StatusMessage = "조직 생성을 요청했습니다.";
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
            AddField("전체 프로젝트 수", page.TotalCount.ToString());
            foreach (var item in page.Items)
                AddField(item.Code, $"{item.Name} · {item.Status}");
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
            AddField("생성된 프로젝트 ID", result.Id.ToString("D"));
            StatusMessage = "프로젝트 생성을 요청했습니다.";
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
            AddField("전체 사용자 수", page.TotalCount.ToString());
            foreach (var item in page.Items)
                AddField(item.Email, $"{item.DisplayName} · {item.Status}");
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
            AddField("생성된 사용자 ID", result.Id.ToString("D"));
            StatusMessage = "사용자 초대를 요청했습니다.";
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
            AddField("생성된 멤버십 ID", result.Id.ToString("D"));
            StatusMessage = "Tenant 멤버십 부여를 요청했습니다.";
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
            AddField("정책 수", policies.Count.ToString());
            foreach (var policy in policies)
                AddField(policy.Action, $"{policy.SubjectId} · {policy.ResourceType} · {policy.Effect} · {(policy.IsEnabled ? "활성" : "비활성")}");
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
                AddField("생성된 정책 ID", result.Id.ToString("D"));
                StatusMessage = "권한 정책 생성을 요청했습니다.";
            }
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
        try
        {
            var page = await Session.Client!.GetAuthorizationAuditAsync(tenantId, subjectId: SearchText,
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
            AddField("페이지", $"{page.Page} / {page.PageSize}");
            AddField("전체 정책 수", page.TotalCount.ToString());
            foreach (var policy in page.Items)
                AddField(policy.Scope, $"{policy.TargetId:D} · {policy.PlanCode} · {(policy.IsEnabled ? "활성" : "비활성")} · v{policy.Version}");
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
            AddField("생성된 정책 ID", result.Id.ToString("D"));
            AddField("버전", result.Version.ToString());
            StatusMessage = "저장 용량 정책 생성을 요청했습니다.";
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
            var history = await Session.Client.GetTenantStorageUsageHistoryAsync(tenantId, take: 10, cancellationToken);
            AddField("최근 스냅샷", history.Count.ToString());
            StatusMessage = "Tenant 및 선택 DataSet의 실제 저장소 사용량을 조회했습니다.";
        }
        catch (TensorApiException ex) { ShowError(ex); }
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
            AddField("검증 실행 ID", result.Execution.Id.ToString("D"));
            AddField("Worker Task ID", result.WorkerTaskId.ToString("D"));
            AddField("생성됨", result.Created ? "예" : "기존 실행 재사용");
            StatusMessage = "DataSet 검증 실행을 요청했습니다.";
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
            AddField("페이지", $"{page.Page} / {page.PageSize}");
            AddField("DLQ 항목 수", page.Items.Count.ToString());
            foreach (var item in page.Items)
                AddField(item.Id.ToString("D"), $"{item.TaskType} · {item.LastErrorCode ?? "오류 코드 없음"}");
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
            AddField("전달 ID", result.Delivery.Id.ToString("D"));
            AddField("Worker Task ID", result.WorkerTaskId.ToString("D"));
            AddField("생성됨", result.Created ? "예" : "기존 전달 재사용");
            StatusMessage = "외부 전달 생성을 요청했습니다.";
        }
        catch (Exception exception) { ShowException(exception); }
    }
}

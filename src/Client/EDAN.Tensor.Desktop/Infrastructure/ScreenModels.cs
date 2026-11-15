using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using EDAN.Tensor.Client;

namespace EDAN.Tensor.Desktop.Infrastructure;

public sealed class ScreenFieldModel(string label, string value) : ObservableObject
{
    public string Label { get; } = label;
    public string Value { get; } = value;
}

public sealed class ScreenActionModel(string label, ICommand command) : ObservableObject
{
    public string Label { get; } = label;
    public ICommand Command { get; } = command;
}

public sealed class ScreenInputModel(string label, string placeholder = "") : ObservableObject
{
    private string _text = string.Empty;

    public string Label { get; } = label;
    public string Placeholder { get; } = placeholder;
    public string Key { get; init; } = label;
    public ScreenInputKind Kind { get; init; } = ScreenInputKind.String;
    public bool Required { get; init; } = true;
    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }
}

public enum ScreenInputKind
{
    String,
    Guid,
    Integer,
    Long,
    Boolean,
    DateTimeOffset,
    Json
}

public sealed class ScreenTargetOptionModel(string label, Guid id, ICommand command)
{
    public string Label { get; } = label;
    public Guid Id { get; } = id;
    public ICommand Command { get; } = command;
}

public abstract class RegistryScreenViewModel : ObservableObject
{
    private string _statusMessage = "연결 후 새로 고침을 실행하세요.";

    protected RegistryScreenViewModel(IDesktopScreenService session, string screenId, string title, string description)
    {
        Session = session;
        ScreenId = screenId;
        Title = title;
        Description = description;
        ConfigureInputs(screenId);
        RefreshCommand = new AsyncRelayCommand(() => RefreshAsync());
        Actions.Add(new ScreenActionModel("새로 고침", RefreshCommand));
        Actions.Add(new ScreenActionModel("API 명령 실행", new AsyncRelayCommand(ExecuteCommandAsync)));
        AddSecondaryActions(screenId);
    }

    public string ScreenId { get; }
    public string Title { get; }
    public string Description { get; }
    public ObservableCollection<ScreenFieldModel> Fields { get; } = [];
    public ObservableCollection<ScreenActionModel> Actions { get; } = [];
    public ObservableCollection<ScreenInputModel> Inputs { get; } = [];
    public ObservableCollection<ScreenTargetOptionModel> TargetOptions { get; } = [];
    protected IDesktopScreenService Session { get; }
    protected AsyncRelayCommand RefreshCommand { get; }
    public string TargetIdText
    {
        get => _targetIdText;
        set => SetProperty(ref _targetIdText, value);
    }
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }
    public string PageText
    {
        get => _pageText;
        set => SetProperty(ref _pageText, value);
    }
    public string PageSizeText
    {
        get => _pageSizeText;
        set => SetProperty(ref _pageSizeText, value);
    }
    public string CommandPayloadText
    {
        get => _commandPayloadText;
        set => SetProperty(ref _commandPayloadText, value);
    }
    public string IdempotencyKeyText
    {
        get => _idempotencyKeyText;
        set => SetProperty(ref _idempotencyKeyText, value);
    }
    public string LastResultIdText
    {
        get => _lastResultIdText;
        protected set => SetProperty(ref _lastResultIdText, value);
    }
    public string StatusMessage
    {
        get => _statusMessage;
        protected set => SetProperty(ref _statusMessage, value);
    }

    public virtual Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        StatusMessage = "이 화면의 API 계약이 연결되면 실제 데이터를 표시합니다.";
        return Task.CompletedTask;
    }

    protected virtual Task ExecuteCommandAsync()
    {
        StatusMessage = "이 화면의 API 명령 계약은 현재 조회 전용 범위입니다.";
        return Task.CompletedTask;
    }

    protected virtual Task ExecuteSecondaryCommandAsync(string operation)
    {
        StatusMessage = $"{operation} 명령은 이 화면의 실제 Client 계약에 연결되지 않았습니다.";
        return Task.CompletedTask;
    }

    private void AddSecondaryActions(string screenId)
    {
        void Add(string label, string operation) => Actions.Add(
            new ScreenActionModel(label, new AsyncRelayCommand(() => ExecuteSecondaryCommandAsync(operation))));

        switch (screenId)
        {
            case "PLT-TNT-001":
                Add("Tenant directory 조회", "tenant-directory");
                Add("Tenant 수정", "tenant-update");
                break;
            case "PLT-ORG-001": Add("조직 수정", "organization-update"); break;
            case "PLT-PJT-001": Add("프로젝트 수정", "project-update"); break;
            case "PLT-USR-001": Add("사용자 수정", "user-update"); break;
            case "PLT-MBR-001":
                Add("조직 멤버십 부여", "organization-membership-grant");
                Add("프로젝트 멤버십 부여", "project-membership-grant");
                break;
            case "PLT-AUTH-001": Add("정책 활성화 변경", "policy-enabled"); break;
            case "PLT-AUD-001":
                Add("감사 아카이브 생성", "audit-archive");
                Add("아카이브 작업 생성", "audit-archive-job");
                Add("아카이브 작업 조회", "audit-archive-job-get");
                Add("아카이브 작업 재시도", "audit-archive-job-retry");
                Add("아카이브 작업 검증", "audit-archive-job-verify");
                Add("보관 스케줄 조회·수정", "audit-schedule");
                Add("보존 정책 조회", "audit-retention");
                break;
            case "PLT-QTA-001":
                Add("Quota 정책 수정", "quota-update");
                Add("Quota 이력 조회", "quota-history");
                Add("Quota 활성화 변경", "quota-enabled");
                break;
            case "DSET-STO-001":
                Add("콘텐츠 업로드", "dataset-content-upload");
                Add("콘텐츠 다운로드", "dataset-content-download");
                Add("Quota 이벤트 조회", "quota-events");
                break;
            case "DSET-VLD-001":
                Add("검증 프로필 생성", "validation-profile");
                Add("프로필 revision 추가", "validation-profile-revision");
                Add("검증 실행 삭제", "validation-delete");
                break;
        }
    }

    protected void AddField(string label, string value) => Fields.Add(new ScreenFieldModel(label, value));

    protected void AddTargetOption(Guid id, string label)
    {
        if (id == Guid.Empty) return;
        TargetOptions.Add(new ScreenTargetOptionModel(label, id,
            new AsyncRelayCommand(() =>
            {
                TargetIdText = id.ToString("D");
                return Task.CompletedTask;
            })));
    }

    protected ScreenInputModel AddInput(string key, string label, string placeholder = "",
        ScreenInputKind kind = ScreenInputKind.String, bool required = true)
    {
        var input = new ScreenInputModel(label, placeholder)
        {
            Key = key,
            Kind = kind,
            Required = required
        };
        Inputs.Add(input);
        return input;
    }

    protected bool TryParsePayload<T>(out T? payload)
    {
        var json = CommandPayloadText;
        if (string.IsNullOrWhiteSpace(json) && !TryBuildStructuredPayload(out json))
        {
            AddField("명령 입력", "Request DTO의 구조화 입력 또는 고급 JSON 입력이 필요합니다.");
            StatusMessage = "명령 Payload 입력 전";
            payload = default;
            return false;
        }
        try
        {
            payload = JsonSerializer.Deserialize<T>(json,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
            if (payload is null) throw new JsonException("빈 JSON 객체입니다.");
            return true;
        }
        catch (JsonException exception)
        {
            AddField("JSON 오류", exception.Message);
            StatusMessage = "Request DTO JSON 오류";
            payload = default;
            return false;
        }
    }

    protected bool TryParseExplicitPayload<T>(out T? payload)
    {
        if (string.IsNullOrWhiteSpace(CommandPayloadText))
        {
            AddField("고급 JSON 입력", "이 명령은 실제 Request DTO JSON이 필요합니다.");
            StatusMessage = "Request DTO JSON 입력 전";
            payload = default;
            return false;
        }
        try
        {
            payload = JsonSerializer.Deserialize<T>(CommandPayloadText,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
            if (payload is null) throw new JsonException("빈 JSON 객체입니다.");
            return true;
        }
        catch (JsonException exception)
        {
            AddField("JSON 오류", exception.Message);
            StatusMessage = "Request DTO JSON 오류";
            payload = default;
            return false;
        }
    }

    private bool TryBuildStructuredPayload(out string json)
    {
        json = string.Empty;
        if (Inputs.Count == 0) return false;

        var values = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var input in Inputs)
        {
            var text = input.Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                if (input.Required)
                {
                    AddField("입력 누락", $"{input.Label} 입력이 필요합니다.");
                    StatusMessage = "구조화 입력 누락";
                    return false;
                }
                continue;
            }

            try
            {
                values[input.Key] = input.Kind switch
                {
                    ScreenInputKind.String => JsonSerializer.SerializeToElement(text),
                    ScreenInputKind.Guid when Guid.TryParse(text, out var value) && value != Guid.Empty
                        => JsonSerializer.SerializeToElement(value),
                    ScreenInputKind.Integer when int.TryParse(text, out var value)
                        => JsonSerializer.SerializeToElement(value),
                    ScreenInputKind.Long when long.TryParse(text, out var value)
                        => JsonSerializer.SerializeToElement(value),
                    ScreenInputKind.Boolean when bool.TryParse(text, out var value)
                        => JsonSerializer.SerializeToElement(value),
                    ScreenInputKind.DateTimeOffset when DateTimeOffset.TryParse(text, out var value)
                        => JsonSerializer.SerializeToElement(value),
                    ScreenInputKind.Json => JsonDocument.Parse(text).RootElement.Clone(),
                    _ => throw new FormatException($"{input.Label} 형식이 올바르지 않습니다.")
                };
            }
            catch (Exception exception) when (exception is FormatException or JsonException)
            {
                AddField("입력 형식 오류", $"{input.Label}: {exception.Message}");
                StatusMessage = "구조화 입력 형식 오류";
                return false;
            }
        }

        json = JsonSerializer.Serialize(values, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return true;
    }

    private void ConfigureInputs(string screenId)
    {
        switch (screenId)
        {
            case "PLT-TNT-001":
            case "PLT-ORG-001":
            case "PLT-PJT-001":
                AddInput("Code", "Code", "예: tenant-prod");
                AddInput("Name", "Name", "표시 이름");
                AddInput("Status", "Status", "선택", ScreenInputKind.String, false);
                if (screenId == "PLT-PJT-001")
                    AddInput("OrganizationId", "Organization ID", "프로젝트 수정 시 필요", ScreenInputKind.Guid, false);
                break;
            case "PLT-USR-001":
                AddInput("Email", "Email", "user@example.com");
                AddInput("DisplayName", "Display name", "표시 이름");
                AddInput("Status", "Status", "선택", ScreenInputKind.String, false);
                break;
            case "PLT-MBR-001":
                AddInput("SubjectId", "Subject ID", "인증 주체 식별자");
                AddInput("Role", "Role", "TenantMember");
                AddInput("OrganizationId", "Organization ID", "조직 멤버십에 필요", ScreenInputKind.Guid, false);
                AddInput("ProjectId", "Project ID", "프로젝트 멤버십에 필요", ScreenInputKind.Guid, false);
                break;
            case "PLT-AUTH-001":
                AddInput("OrganizationId", "Organization ID", "선택", ScreenInputKind.Guid, false);
                AddInput("ProjectId", "Project ID", "선택", ScreenInputKind.Guid, false);
                AddInput("SubjectId", "Subject ID");
                AddInput("Action", "Action");
                AddInput("ResourceType", "Resource type");
                AddInput("Effect", "Effect", "Allow 또는 Deny");
                AddInput("Priority", "Priority", "0", ScreenInputKind.Integer);
                break;
            case "PLT-AUD-001":
                AddInput("Action", "Action filter", "선택", ScreenInputKind.String, false);
                AddInput("IsAllowed", "Is allowed", "true 또는 false", ScreenInputKind.Boolean, false);
                AddInput("FromUtc", "From UTC", "무결성·archive 명령에 필요", ScreenInputKind.DateTimeOffset);
                AddInput("ToUtc", "To UTC", "무결성·archive 명령에 필요", ScreenInputKind.DateTimeOffset);
                AddInput("ExpectedSha256", "Expected SHA-256", "무결성 검증에 필요", ScreenInputKind.String);
                break;
            case "PLT-QTA-001":
                AddInput("Scope", "Scope", "Tenant/Organization/Project/DataSet");
                AddInput("TargetId", "Target ID", "GUID", ScreenInputKind.Guid);
                AddInput("TenantLimitBytes", "Tenant limit bytes", "선택", ScreenInputKind.Long, false);
                AddInput("OrganizationLimitBytes", "Organization limit bytes", "선택", ScreenInputKind.Long, false);
                AddInput("ProjectLimitBytes", "Project limit bytes", "선택", ScreenInputKind.Long, false);
                AddInput("DataSetLimitBytes", "DataSet limit bytes", "선택", ScreenInputKind.Long, false);
                AddInput("WarningThresholdPercent", "Warning threshold %", "80", ScreenInputKind.Integer);
                AddInput("PlanCode", "Plan code", "Community");
                break;
            case "DSET-DSM-001":
                AddInput("OrganizationId", "Organization ID", "GUID", ScreenInputKind.Guid);
                AddInput("ProjectId", "Project ID", "GUID", ScreenInputKind.Guid);
                AddInput("Name", "Name");
                AddInput("StorageAdapter", "Storage adapter", "sqlite");
                AddInput("ContentFormat", "Content format", "json-array");
                break;
            case "DSET-SCHM-001":
                AddInput("SchemaJson", "Schema JSON", "JSON schema 문서", ScreenInputKind.Json);
                break;
            case "DSET-VLD-001":
                AddInput("ExpectedDataSetVersion", "Expected DataSet version", "1", ScreenInputKind.Integer);
                AddInput("SchemaVersion", "Schema version", "1", ScreenInputKind.Integer);
                AddInput("ProfileId", "Profile ID", "GUID", ScreenInputKind.Guid);
                AddInput("ProfileRevision", "Profile revision", "1", ScreenInputKind.Integer);
                break;
            case "DSET-STO-001":
                AddInput("ContentPath", "Content file path", "업로드 또는 다운로드 경로", ScreenInputKind.String, false);
                break;
            case "WRK-TSK-001":
                AddInput("TaskType", "Task type", "등록된 Worker task type");
                AddInput("PayloadReference", "Payload reference", "보안 참조");
                AddInput("MaxAttempts", "Max attempts", "5", ScreenInputKind.Integer);
                break;
            case "WRK-XDL-001":
                AddInput("TargetUri", "Target URI", "https://receiver.example/api");
                AddInput("Payload", "Payload JSON", "JSON object", ScreenInputKind.Json);
                AddInput("MaxAttempts", "Max attempts", "5", ScreenInputKind.Integer);
                break;
        }
    }

    protected string GetInputText(string key) =>
        Inputs.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.Ordinal))?.Text.Trim() ?? string.Empty;

    protected bool TryGetRequestKey(out string key)
    {
        key = IdempotencyKeyText.Trim();
        if (!string.IsNullOrWhiteSpace(key) && key.Length <= 200)
            return true;
        AddField("멱등성 키", "1~200자의 키가 필요합니다.");
        StatusMessage = "멱등성 키 오류";
        return false;
    }

    protected void ShowException(Exception exception)
    {
        if (exception is TensorApiException apiException)
        {
            AddField("오류 코드", apiException.Code);
            AddField("요청 ID", apiException.RequestId ?? "없음");
            StatusMessage = apiException.Description;
            return;
        }
        AddField("오류", exception.Message);
        StatusMessage = "API 명령을 실행하지 못했습니다.";
    }

    protected bool TryGetTargetId(out Guid id)
    {
        if (Guid.TryParse(TargetIdText.Trim(), out id) && id != Guid.Empty)
            return true;
        id = Guid.Empty;
        return false;
    }

    protected bool TryGetPage(out int page, out int pageSize)
    {
        page = int.TryParse(PageText, out var parsedPage) ? parsedPage : 1;
        pageSize = int.TryParse(PageSizeText, out var parsedPageSize) ? parsedPageSize : 50;
        if (page is < 1 or > int.MaxValue || pageSize is < 1 or > 100)
        {
            AddField("페이지 조건", "페이지는 1 이상, 페이지 크기는 1~100 범위여야 합니다.");
            StatusMessage = "페이지 조건 오류";
            return false;
        }
        return true;
    }

    private string _targetIdText = string.Empty;
    private string _searchText = string.Empty;
    private string _pageText = "1";
    private string _pageSizeText = "50";
    private string _commandPayloadText = string.Empty;
    private string _idempotencyKeyText = Guid.NewGuid().ToString("N");
    private string _lastResultIdText = string.Empty;
}

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
    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }
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
        RefreshCommand = new AsyncRelayCommand(() => RefreshAsync());
        Actions.Add(new ScreenActionModel("새로 고침", RefreshCommand));
        Actions.Add(new ScreenActionModel("API 명령 실행", new AsyncRelayCommand(ExecuteCommandAsync)));
    }

    public string ScreenId { get; }
    public string Title { get; }
    public string Description { get; }
    public ObservableCollection<ScreenFieldModel> Fields { get; } = [];
    public ObservableCollection<ScreenActionModel> Actions { get; } = [];
    public ObservableCollection<ScreenInputModel> Inputs { get; } = [];
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

    protected void AddField(string label, string value) => Fields.Add(new ScreenFieldModel(label, value));

    protected bool TryParsePayload<T>(out T? payload)
    {
        if (string.IsNullOrWhiteSpace(CommandPayloadText))
        {
            AddField("명령 입력", "API Request DTO JSON이 필요합니다.");
            StatusMessage = "명령 Payload 입력 전";
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
}

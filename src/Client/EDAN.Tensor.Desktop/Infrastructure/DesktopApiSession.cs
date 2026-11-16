using EDAN.Tensor.Client;
using System.Net.Http;

namespace EDAN.Tensor.Desktop.Infrastructure;

public sealed class DesktopApiSession : ObservableObject, IDesktopScreenService, IDisposable
{
    private string _baseAddress = "https://localhost:5001";
    private string _tenantIdText = string.Empty;
    private string _accessToken = string.Empty;
    private string _statusMessage = "API 연결 전";
    private HttpClient? _httpClient;
    private readonly bool _ownsHttpClient;

    /// <summary>기본 설정으로 데스크톱 API 세션을 생성합니다.</summary>
    public DesktopApiSession()
    {
        _ownsHttpClient = true;
    }

    /// <summary>지정한 HTTP 클라이언트를 사용하는 데스크톱 API 세션을 생성합니다.</summary>
    /// <param name="httpClient">API 호출에 사용할 HTTP 클라이언트입니다.</param>
    /// <exception cref="ArgumentNullException">HTTP 클라이언트가 <see langword="null"/>이면 발생합니다.</exception>
    public DesktopApiSession(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _ownsHttpClient = false;
        if (httpClient.BaseAddress is not null)
            _baseAddress = httpClient.BaseAddress.ToString().TrimEnd('/');
    }

    public string BaseAddress
    {
        get => _baseAddress;
        set => SetProperty(ref _baseAddress, value);
    }

    public string TenantIdText
    {
        get => _tenantIdText;
        set => SetProperty(ref _tenantIdText, value);
    }

    public string AccessToken
    {
        get => _accessToken;
        set => SetProperty(ref _accessToken, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public TensorApiClient? Client { get; private set; }
    public Guid? TenantId { get; private set; }

    public Task ConnectAsync()
    {
        if (!Uri.TryCreate(BaseAddress.Trim(), UriKind.Absolute, out var baseUri) ||
            baseUri.Scheme is not ("http" or "https"))
        {
            StatusMessage = "API URL은 http 또는 https 주소여야 합니다.";
            return Task.CompletedTask;
        }

        if (!string.IsNullOrWhiteSpace(TenantIdText) &&
            (!Guid.TryParse(TenantIdText.Trim(), out var tenantId) || tenantId == Guid.Empty))
        {
            StatusMessage = "Tenant ID는 비어 있거나 유효한 GUID여야 합니다.";
            return Task.CompletedTask;
        }

        TenantId = string.IsNullOrWhiteSpace(TenantIdText) ? null : Guid.Parse(TenantIdText.Trim());
        if (_ownsHttpClient || _httpClient is null)
        {
            _httpClient?.Dispose();
            _httpClient = new HttpClient();
        }

        _httpClient.BaseAddress = baseUri;
        Client = new TensorApiClient(_httpClient, new TensorApiClientOptions
        {
            DefaultTenantId = TenantId,
            AccessTokenProvider = _ => ValueTask.FromResult<string?>(
                string.IsNullOrWhiteSpace(AccessToken) ? null : AccessToken.Trim())
        });
        StatusMessage = TenantId is null
            ? $"연결됨: {baseUri} (Tenant 미선택)"
            : $"연결됨: {baseUri} / Tenant {TenantId:D}";
        OnPropertyChanged(nameof(Client));
        OnPropertyChanged(nameof(TenantId));
        return Task.CompletedTask;
    }

    /// <summary>세션이 소유한 HTTP 클라이언트만 해제합니다.</summary>
    public void Dispose()
    {
        if (_ownsHttpClient)
            _httpClient?.Dispose();
    }
}

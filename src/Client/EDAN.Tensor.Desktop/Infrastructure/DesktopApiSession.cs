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
        _httpClient?.Dispose();
        _httpClient = new HttpClient { BaseAddress = baseUri };
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

    public void Dispose() => _httpClient?.Dispose();
}

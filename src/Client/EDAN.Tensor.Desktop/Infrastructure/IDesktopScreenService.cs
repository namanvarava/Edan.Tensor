using EDAN.Tensor.Client;

namespace EDAN.Tensor.Desktop.Infrastructure;

/// <summary>Registry 화면 ViewModel이 사용하는 공통 API 세션 계약입니다.</summary>
public interface IDesktopScreenService
{
    TensorApiClient? Client { get; }
    Guid? TenantId { get; }
}

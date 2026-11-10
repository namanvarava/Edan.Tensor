using EDAN_Tensor_Licensing_Contracts;
using EDAN.Tensor.SharedKernel.Results;

namespace EDAN_Tensor_Licensing_Application.Abstractions;

/// <summary>라이선스 공개 정보의 서명 토큰 생성과 검증 경계를 제공합니다.</summary>
public interface ILicenseTokenCodec
{
    /// <summary>현재 서명 키의 식별자를 반환합니다.</summary>
    string KeyId { get; }

    /// <summary>API가 검증에 사용할 공개키를 JWK 문자열로 반환합니다.</summary>
    string PublicJwk { get; }

    /// <summary>라이선스 정보를 ES256 서명 토큰으로 직렬화합니다.</summary>
    /// <param name="descriptor">서명할 라이선스 공개 정보입니다.</param>
    /// <returns>개인키를 노출하지 않는 compact JWS 토큰입니다.</returns>
    string CreateToken(LicenseDescriptor descriptor);

    /// <summary>서명·시간·발급자·Claim 구조를 검증하고 라이선스 정보를 반환합니다.</summary>
    /// <param name="token">검증할 compact JWS 토큰입니다.</param>
    /// <param name="nowUtc">검증 기준 UTC 시각입니다.</param>
    /// <returns>검증된 라이선스 정보 또는 안정적인 오류를 반환합니다.</returns>
    Result<LicenseDescriptor> ReadAndValidate(string token, DateTimeOffset nowUtc);
}


using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EDAN_Tensor_Licensing_Application.Abstractions;
using EDAN_Tensor_Licensing_Contracts;
using EDAN.Tensor.SharedKernel.Results;

namespace EDAN_Tensor_Licensing_Infrastructure.Cryptography;

/// <summary>ES256 compact JWS로 라이선스를 서명하고 검증합니다.</summary>
public sealed class Es256LicenseTokenCodec(
    FileEs256KeyMaterial keyMaterial,
    string issuer = "EDAN.Tensor.LicenseAuthority") : ILicenseTokenCodec
{
    /// <inheritdoc />
    public string KeyId => keyMaterial.KeyId;

    /// <inheritdoc />
    public string PublicJwk => keyMaterial.PublicJwk;

    /// <inheritdoc />
    public string CreateToken(LicenseDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (!string.Equals(descriptor.KeyId, KeyId, StringComparison.Ordinal))
            throw new InvalidOperationException("라이선스 KeyId가 서명 키와 일치하지 않습니다.");

        var header = JsonSerializer.Serialize(new
        {
            alg = "ES256",
            kid = KeyId,
            typ = "JWT"
        });
        var payload = JsonSerializer.Serialize(new
        {
            iss = issuer,
            license_id = descriptor.LicenseId,
            site_or_tenant = descriptor.SiteOrTenant,
            product = descriptor.Product,
            features = descriptor.Features,
            status = descriptor.Status.ToString(),
            iat = descriptor.IssuedAtUtc.ToUnixTimeSeconds(),
            nbf = descriptor.StartsAtUtc.ToUnixTimeSeconds(),
            exp = descriptor.ExpiresAtUtc.ToUnixTimeSeconds()
        });
        var encodedHeader = FileEs256KeyMaterial.Base64Url(Encoding.UTF8.GetBytes(header));
        var encodedPayload = FileEs256KeyMaterial.Base64Url(Encoding.UTF8.GetBytes(payload));
        var signingInput = $"{encodedHeader}.{encodedPayload}";
        var signature = keyMaterial.SigningKey.SignData(
            Encoding.UTF8.GetBytes(signingInput),
            HashAlgorithmName.SHA256,
            DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
        return $"{signingInput}.{FileEs256KeyMaterial.Base64Url(signature)}";
    }

    /// <inheritdoc />
    public Result<LicenseDescriptor> ReadAndValidate(string token, DateTimeOffset nowUtc)
    {
        try
        {
            var parts = token.Split('.', StringSplitOptions.None);
            if (parts.Length != 3 || parts.Any(string.IsNullOrWhiteSpace))
                return Failure("licensing.token.invalid", "라이선스 토큰 형식이 올바르지 않습니다.");

            using var header = JsonDocument.Parse(Encoding.UTF8.GetString(FileEs256KeyMaterial.FromBase64Url(parts[0])));
            var headerRoot = header.RootElement;
            if (!string.Equals(headerRoot.GetProperty("alg").GetString(), "ES256", StringComparison.Ordinal)
                || !string.Equals(headerRoot.GetProperty("kid").GetString(), KeyId, StringComparison.Ordinal))
                return Failure("licensing.token.key_invalid", "라이선스 서명 알고리즘 또는 KeyId가 올바르지 않습니다.");

            var signature = FileEs256KeyMaterial.FromBase64Url(parts[2]);
            var validSignature = keyMaterial.SigningKey.VerifyData(
                Encoding.UTF8.GetBytes($"{parts[0]}.{parts[1]}"),
                signature,
                HashAlgorithmName.SHA256,
                DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
            if (!validSignature)
                return Failure("licensing.token.signature_invalid", "라이선스 서명이 올바르지 않습니다.");

            using var payload = JsonDocument.Parse(Encoding.UTF8.GetString(FileEs256KeyMaterial.FromBase64Url(parts[1])));
            var root = payload.RootElement;
            if (!string.Equals(root.GetProperty("iss").GetString(), issuer, StringComparison.Ordinal))
                return Failure("licensing.issuer_invalid", "라이선스 발급자가 올바르지 않습니다.");

            var descriptor = new LicenseDescriptor(
                root.GetProperty("license_id").GetGuid(),
                root.GetProperty("site_or_tenant").GetString() ?? string.Empty,
                root.GetProperty("product").GetString() ?? string.Empty,
                root.GetProperty("features").EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToArray(),
                DateTimeOffset.FromUnixTimeSeconds(root.GetProperty("iat").GetInt64()),
                DateTimeOffset.FromUnixTimeSeconds(root.GetProperty("nbf").GetInt64()),
                DateTimeOffset.FromUnixTimeSeconds(root.GetProperty("exp").GetInt64()),
                headerRoot.GetProperty("kid").GetString() ?? string.Empty,
                Enum.Parse<LicenseStatus>(root.GetProperty("status").GetString() ?? string.Empty, ignoreCase: false));

            if (descriptor.StartsAtUtc > nowUtc)
                return Failure("licensing.not_yet_valid", "라이선스 사용 시작 시각이 아직 도래하지 않았습니다.", descriptor);
            if (descriptor.ExpiresAtUtc <= nowUtc)
                return Failure("licensing.expired", "라이선스 유효 기간이 만료되었습니다.", descriptor);
            if (descriptor.Status != LicenseStatus.Active)
                return Failure("licensing.status_invalid", "라이선스 상태가 사용 가능한 상태가 아닙니다.", descriptor);

            return Result<LicenseDescriptor>.Success(descriptor);
        }
        catch (Exception exception) when (exception is FormatException or JsonException or KeyNotFoundException or InvalidOperationException)
        {
            return Failure("licensing.token.invalid", "라이선스 토큰을 해석할 수 없습니다.");
        }
    }

    private static Result<LicenseDescriptor> Failure(
        string code,
        string description,
        LicenseDescriptor? descriptor = null) =>
        Result<LicenseDescriptor>.Failure(new Error(code, description));
}


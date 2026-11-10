using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EDAN_Tensor_Licensing_Infrastructure.Cryptography;

/// <summary>ES256 개인키를 파일에서 읽거나 개발 환경에서 최초 한 번 생성합니다.</summary>
public sealed class FileEs256KeyMaterial : IDisposable
{
    private readonly ECDsa signingKey;

    /// <summary>파일 키 자료를 준비합니다.</summary>
    /// <param name="privateKeyPath">개인키 PEM 파일 경로입니다.</param>
    /// <param name="configuredKeyId">설정으로 고정할 KeyId이며 비어 있으면 공개키에서 계산합니다.</param>
    /// <param name="allowGeneration">개인키가 없을 때 개발용 키를 새로 만들 수 있는지 나타냅니다.</param>
    public FileEs256KeyMaterial(
        string privateKeyPath,
        string? configuredKeyId = null,
        bool allowGeneration = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(privateKeyPath);
        var fullPath = Path.GetFullPath(privateKeyPath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        signingKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        if (File.Exists(fullPath))
        {
            signingKey.ImportFromPem(File.ReadAllText(fullPath));
        }
        else if (!allowGeneration)
        {
            signingKey.Dispose();
            throw new FileNotFoundException(
                "운영 환경에서는 사전 배포된 LicenseAuthority 개인키가 필요합니다.",
                fullPath);
        }
        else
        {
            var pem = signingKey.ExportPkcs8PrivateKeyPem();
            File.WriteAllText(fullPath, pem, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        KeyId = string.IsNullOrWhiteSpace(configuredKeyId)
            ? CreateKeyId(signingKey.ExportSubjectPublicKeyInfo())
            : configuredKeyId.Trim();
        PublicJwk = CreatePublicJwk(signingKey, KeyId);
    }

    /// <summary>서명 키의 식별자를 반환합니다.</summary>
    public string KeyId { get; }

    /// <summary>검증자가 사용할 공개키를 JWK로 반환합니다.</summary>
    public string PublicJwk { get; }

    /// <summary>토큰 서명에 사용할 내부 ECDSA 키를 반환합니다.</summary>
    public ECDsa SigningKey => signingKey;

    /// <summary>키 객체를 해제합니다.</summary>
    public void Dispose() => signingKey.Dispose();

    private static string CreateKeyId(byte[] publicKey)
    {
        var hash = Convert.ToHexString(SHA256.HashData(publicKey)).ToLowerInvariant();
        return $"es256-{hash[..16]}";
    }

    private static string CreatePublicJwk(ECDsa key, string keyId)
    {
        var parameters = key.ExportParameters(includePrivateParameters: false);
        return JsonSerializer.Serialize(new
        {
            kty = "EC",
            crv = "P-256",
            x = Base64Url(parameters.Q.X!),
            y = Base64Url(parameters.Q.Y!),
            use = "sig",
            alg = "ES256",
            kid = keyId
        });
    }

    /// <summary>바이트 배열을 JWK용 Base64Url 문자열로 변환합니다.</summary>
    public static string Base64Url(byte[] value) =>
        Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    /// <summary>Base64Url 문자열을 바이트 배열로 변환합니다.</summary>
    public static byte[] FromBase64Url(string value)
    {
        var normalized = value.Replace('-', '+').Replace('_', '/');
        normalized = normalized.PadRight(normalized.Length + ((4 - normalized.Length % 4) % 4), '=');
        return Convert.FromBase64String(normalized);
    }
}


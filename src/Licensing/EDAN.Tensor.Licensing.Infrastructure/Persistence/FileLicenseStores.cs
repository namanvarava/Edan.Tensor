using System.Text.Json;
using EDAN_Tensor_Licensing_Application.Abstractions;
using EDAN_Tensor_Licensing_Contracts;

namespace EDAN_Tensor_Licensing_Infrastructure.Persistence;

/// <summary>개발 라이선스 공개 토큰을 로컬 파일에 한 번 저장합니다.</summary>
public sealed class FileDevelopmentLicenseStore(string path) : IDevelopmentLicenseStore
{
    private readonly string path = Path.GetFullPath(path);
    private readonly SemaphoreSlim gate = new(1, 1);

    /// <inheritdoc />
    public async Task<IssuedLicense?> GetAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(path))
                return null;
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<IssuedLicense>(stream, cancellationToken: cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(IssuedLicense license, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(license);
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (File.Exists(path))
                throw new IOException("개발 라이선스는 이미 발급되어 있습니다.");
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);
            var temporaryPath = $"{path}.{Guid.NewGuid():N}.tmp";
            await using (var stream = File.Create(temporaryPath))
                await JsonSerializer.SerializeAsync(stream, license, cancellationToken: cancellationToken);
            File.Move(temporaryPath, path);
        }
        finally
        {
            gate.Release();
        }
    }
}

/// <summary>폐기된 라이선스 식별자를 로컬 JSON 파일에 보관합니다.</summary>
public sealed class FileLicenseStatusStore(string path) : ILicenseStatusStore
{
    private readonly string path = Path.GetFullPath(path);
    private readonly SemaphoreSlim gate = new(1, 1);

    /// <inheritdoc />
    public async Task<LicenseStatus?> GetAsync(Guid licenseId, CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            var revoked = await ReadRevokedAsync(cancellationToken);
            return revoked.Contains(licenseId) ? LicenseStatus.Revoked : null;
        }
        finally
        {
            gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task RevokeAsync(Guid licenseId, CancellationToken cancellationToken = default)
    {
        if (licenseId == Guid.Empty)
            throw new ArgumentException("라이선스 식별자는 비어 있을 수 없습니다.", nameof(licenseId));
        await gate.WaitAsync(cancellationToken);
        try
        {
            var revoked = await ReadRevokedAsync(cancellationToken);
            revoked.Add(licenseId);
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);
            var temporaryPath = $"{path}.{Guid.NewGuid():N}.tmp";
            await using (var stream = File.Create(temporaryPath))
                await JsonSerializer.SerializeAsync(stream, revoked.OrderBy(x => x), cancellationToken: cancellationToken);
            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<HashSet<Guid>> ReadRevokedAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
            return [];
        await using var stream = File.OpenRead(path);
        var values = await JsonSerializer.DeserializeAsync<Guid[]>(stream, cancellationToken: cancellationToken);
        return values?.ToHashSet() ?? [];
    }
}


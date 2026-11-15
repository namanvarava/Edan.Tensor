using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;

namespace EDAN.Tensor.Platform.Infrastructure.Invitations;

/// <summary>암호학적으로 안전한 URL-safe token을 생성하고 SHA-256 hash만 반환합니다.</summary>
public sealed class InvitationTokenService : IInvitationTokenService
{
    public string CreateToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
        .Replace('+', '-').Replace('/', '_').TrimEnd('=');

    public string HashToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token.Trim()))).ToLowerInvariant();
    }
}

/// <summary>운영에서는 이메일/메시지 Adapter로 교체할 수 있는 기본 no-op 전달 구현입니다.</summary>
public sealed class NoOpInvitationDelivery : IInvitationDelivery
{
    public Task DeliverAsync(Invitation invitation, string rawToken, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

/// <summary>Development와 통합 테스트에서만 token 전달 결과를 확인하기 위한 메모리 Adapter입니다.</summary>
public sealed class DevelopmentInvitationDelivery : IInvitationDelivery
{
    private readonly ConcurrentDictionary<Guid, string> tokens = new();
    public Task DeliverAsync(Invitation invitation, string rawToken, CancellationToken cancellationToken = default)
    {
        tokens[invitation.Id.Value] = rawToken;
        return Task.CompletedTask;
    }

    public bool TryGetToken(Guid invitationId, out string token) => tokens.TryGetValue(invitationId, out token!);
}

using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.SharedKernel.Results;
using EDAN.Tensor.SharedKernel.Time;

namespace EDAN.Tensor.Platform.Application.UseCases;

/// <summary>초대 생성·재전송·철회·수락을 조정하며 token 평문은 응답이나 저장소에 노출하지 않습니다.</summary>
public sealed class InvitationService(
    ITenantRepository tenants,
    IUserRepository users,
    IInvitationRepository invitations,
    IPlatformUnitOfWork unitOfWork,
    ISystemClock clock,
    IInvitationTokenService tokenService,
    IInvitationDelivery delivery)
{
    public async Task<Result<InvitationView>> CreateAsync(TenantId tenantId, string email, string displayName,
        TimeSpan? lifetime = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await tenants.GetByIdAsync(tenantId, cancellationToken);
            if (tenant is null || tenant.Status != TenantStatus.Active)
                return Fail<InvitationView>("platform.tenant.unavailable", "활성 상태의 테넌트에만 사용자를 초대할 수 있습니다.");
            var address = new EmailAddress(email);
            if (await users.ExistsEmailAsync(tenantId, address, cancellationToken))
                return Fail<InvitationView>("platform.user.email_conflict", "해당 테넌트에서 이미 사용 중인 이메일 주소입니다.");

            var now = clock.UtcNow;
            var rawToken = tokenService.CreateToken();
            var user = User.Invite(new UserId(Guid.NewGuid()), tenantId, address, displayName);
            var invitation = Invitation.Create(new InvitationId(Guid.NewGuid()), tenantId, user.Id, address,
                displayName, tokenService.HashToken(rawToken), now, now.Add(NormalizeLifetime(lifetime)));
            users.Add(user);
            invitations.Add(invitation);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await delivery.DeliverAsync(invitation, rawToken, cancellationToken);
            return Result<InvitationView>.Success(ToView(invitation));
        }
        catch (EDAN.Tensor.SharedKernel.Exceptions.DomainException ex) { return Fail<InvitationView>(ex.Code, ex.Message); }
        catch (ArgumentException ex) { return Fail<InvitationView>("platform.validation", ex.Message); }
    }

    public async Task<InvitationPage> GetPageAsync(TenantId tenantId, string? search, InvitationStatus? status,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = await invitations.GetPageAsync(tenantId, search, status, page, pageSize, cancellationToken);
        foreach (var item in result.Items) item.ExpireIfNeeded(clock.UtcNow);
        return new(result.TotalCount, page, pageSize, result.Items.Select(ToView).ToArray());
    }

    public async Task<Result<InvitationView>> ResendAsync(TenantId tenantId, InvitationId invitationId,
        TimeSpan? lifetime = null, CancellationToken cancellationToken = default)
    {
        var invitation = await invitations.GetByIdAsync(tenantId, invitationId, cancellationToken);
        if (invitation is null) return Fail<InvitationView>("platform.invitation.not_found", "초대를 찾을 수 없습니다.");
        try
        {
            var now = clock.UtcNow;
            var rawToken = tokenService.CreateToken();
            invitation.Resend(tokenService.HashToken(rawToken), now, now.Add(NormalizeLifetime(lifetime)));
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await delivery.DeliverAsync(invitation, rawToken, cancellationToken);
            return Result<InvitationView>.Success(ToView(invitation));
        }
        catch (EDAN.Tensor.SharedKernel.Exceptions.DomainException ex) { return Fail<InvitationView>(ex.Code, ex.Message); }
        catch (ArgumentException ex) { return Fail<InvitationView>("platform.validation", ex.Message); }
    }

    public async Task<Result> RevokeAsync(TenantId tenantId, InvitationId invitationId, CancellationToken cancellationToken = default)
    {
        var invitation = await invitations.GetByIdAsync(tenantId, invitationId, cancellationToken);
        if (invitation is null) return Result.Failure(new Error("platform.invitation.not_found", "초대를 찾을 수 없습니다."));
        try { invitation.Revoke(clock.UtcNow); await unitOfWork.SaveChangesAsync(cancellationToken); return Result.Success(); }
        catch (EDAN.Tensor.SharedKernel.Exceptions.DomainException ex) { return Result.Failure(new Error(ex.Code, ex.Message)); }
    }

    public async Task<Result<UserId>> AcceptAsync(string rawToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var invitation = await invitations.GetByTokenHashAsync(tokenService.HashToken(rawToken), cancellationToken);
            if (invitation is null) return Result<UserId>.Failure(new Error("platform.invitation.token_invalid", "유효하지 않거나 만료된 초대 token입니다."));
            invitation.Accept(tokenService.HashToken(rawToken), clock.UtcNow);
            var user = await users.GetByIdAsync(invitation.UserId, cancellationToken);
            if (user is null) return Result<UserId>.Failure(new Error("platform.user.not_found", "초대된 사용자를 찾을 수 없습니다."));
            user.Activate();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<UserId>.Success(user.Id);
        }
        catch (EDAN.Tensor.SharedKernel.Exceptions.DomainException ex) { return Result<UserId>.Failure(new Error(ex.Code, ex.Message)); }
        catch (ArgumentException ex) { return Result<UserId>.Failure(new Error("platform.validation", ex.Message)); }
    }

    private static TimeSpan NormalizeLifetime(TimeSpan? lifetime)
    {
        if (lifetime is null) return TimeSpan.FromDays(7);
        if (lifetime.Value <= TimeSpan.Zero || lifetime.Value > TimeSpan.FromDays(30))
            throw new ArgumentOutOfRangeException(nameof(lifetime), "초대 유효 기간은 1~30일이어야 합니다.");
        return lifetime.Value;
    }
    private static InvitationView ToView(Invitation item) => new(item.Id.Value, item.TenantId.Value, item.UserId.Value, item.Email.Value,
        item.DisplayName, item.Status.ToString(), item.CreatedAtUtc, item.ExpiresAtUtc, item.AcceptedAtUtc, item.RevokedAtUtc, item.ResendCount);
    private static Result<T> Fail<T>(string code, string description) => Result<T>.Failure(new Error(code, description));
}

public sealed record InvitationView(Guid Id, Guid TenantId, Guid UserId, string Email, string DisplayName, string Status,
    DateTimeOffset CreatedAtUtc, DateTimeOffset ExpiresAtUtc, DateTimeOffset? AcceptedAtUtc, DateTimeOffset? RevokedAtUtc, int ResendCount);
public sealed record InvitationPage(int TotalCount, int Page, int PageSize, IReadOnlyList<InvitationView> Items);

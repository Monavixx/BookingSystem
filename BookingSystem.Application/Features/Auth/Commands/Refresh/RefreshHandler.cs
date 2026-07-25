using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Common.DTOs;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Users.Errors;
using BookingSystem.Domain.Users.ValueObjects;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Features.Auth.Commands.Refresh;

public class RefreshHandler(
    AppDbContext dbContext,
    ILogger<RefreshHandler> logger,
    IRefreshTokenService refreshTokenService,
    IJwtTokenService jwtTokenService) : IRequestHandler<RefreshCommand, Result<AuthTokens>>
{
    public async Task<Result<AuthTokens>> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Trying refresh auth tokens");
        var refreshTokenRes = RefreshToken.FromString(request.RefreshToken);
        if(refreshTokenRes.IsFailed) return refreshTokenRes.ToResult<AuthTokens>();
        byte[] refreshToken = refreshTokenRes.Value;
        var session =
            await dbContext.Sessions.Include(session => session.User)
                .FirstOrDefaultAsync(s => s.RefreshToken.Token == refreshToken, cancellationToken);
        if (session is null)
        {
            logger.LogWarning("Refresh failed: invalid refresh token");
            return Result.Fail<AuthTokens>(SessionErrors.NotFound);
        }

        if (session.User is null)
        {
            logger.LogError("Refresh failed: invalid user");
            return Result.Fail<AuthTokens>(UserErrors.NotFound);
        }

        session.UpdateRefreshToken(refreshTokenService.GenerateRefreshToken());
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Refresh successful for user {UserId}", session.UserId);
        return new AuthTokens(
            AccessToken: jwtTokenService.GenerateJwtToken(session.User),
            RefreshToken: session.RefreshToken
        );
    }
}
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Users.Errors;
using BookingSystem.Domain.Users.ValueObjects;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Users.Commands.Block;

public class BlockUserHandler(AppDbContext dbContext, TimeProvider timeProvider)
    : IRequestHandler<BlockUserCommand, Result>
{
    public async Task<Result> Handle(BlockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FindAsync([new UserId(request.UserId)], cancellationToken);
        if (user is null) return UserErrors.NotFound;
        
        var res = user.Block(timeProvider, request.Duration);
        if(res.IsFailed) return res;
        
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Users;
using BookingSystem.Domain.Users.Errors;
using BookingSystem.Domain.Users.ValueObjects;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Users.Commands.MakeManager;

public class MakeManagerHandler(AppDbContext dbContext) : IRequestHandler<MakeManagerCommand, Result>
{
    public async Task<Result> Handle(MakeManagerCommand request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FindAsync([new UserId(request.UserId)], cancellationToken);
        if (user == null) return Result.Fail(UserErrors.NotFound);
        user.MakeManager();
        var manager = Manager.Create(user.Id);
        dbContext.Managers.Add(manager);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
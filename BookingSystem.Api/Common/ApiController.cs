using BookingSystem.Domain.Common.Errors;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Api.Common;

[ApiController]
public abstract class ApiController(IMediator mediator) : ControllerBase
{
    protected readonly IMediator Mediator = mediator;

    protected IActionResult HandleResultOk<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);
        return HandleErrors(result);
    }

    protected IActionResult HandleResultNoContent(Result result)
    {
        if (result.IsSuccess)
            return NoContent();
        return HandleErrors(result);
    }

    protected IActionResult HandleErrors(IResultBase result)
    {
        var errorTypes = result.Errors.DistinctBy(e => e.GetType()).ToArray();
        var typesCount = errorTypes.Length;
        switch (typesCount)
        {
            case 0:
                throw new ArgumentException("Result must contain at least one error");
            case 1:
                return Problem(
                    title: GetTitle(result.Errors[0]),
                    statusCode: ErrorToHttpCode(result.Errors[0]),
                    detail: result.Errors.Count == 1 ? result.Errors[0].Message : "More than one error occurred",
                    extensions: new Dictionary<string, object?>
                    {
                        ["errors"] = result.Errors.Select(e => new
                                { message = e.Message, code = (e as DomainError)?.Code, metadata = e.Metadata })
                            .ToArray()
                    }
                );
            default:
                return Problem(
                    title: "Multiple errors",
                    statusCode: StatusCodes.Status400BadRequest,
                    detail: string.Join("; ", errorTypes.Select(GetTitle)),
                    extensions: new Dictionary<string, object?>
                    {
                        ["errors"] = result.Errors.Select(e => new
                            {
                                title = GetTitle(e), statusCode = ErrorToHttpCode(e), message = e.Message,
                                code = (e as DomainError)?.Code, metadata = e.Metadata
                            })
                            .ToArray()
                    }
                );
        }
    }

    private int ErrorToHttpCode(IError error)
        => error switch
        {
            ValidationError => StatusCodes.Status400BadRequest,
            NotFoundError => StatusCodes.Status404NotFound,
            ConflictError => StatusCodes.Status409Conflict,
            UnauthorizedError => StatusCodes.Status401Unauthorized,
            ReferenceError => StatusCodes.Status400BadRequest,
            ForbiddenError => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

    private string GetTitle(IError e)
        => e is DomainError de2 ? de2.Title : e.GetType().Name;
}
using Microsoft.AspNetCore.Mvc;
using QuizKit.Common.Results;

namespace QuizKit.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        return result switch
        {
            { IsSuccess: true } => new OkObjectResult(result.Data),
            _ => ToActionResult(result as Result),
        };
    }

    public static IActionResult ToActionResult(this Result result)
    {
        return result switch
        {
            { Status: ResultStatus.Unauthorized} => new UnauthorizedResult(),
            { Status: ResultStatus.Forbidden } => new ForbidResult(),
            { Status: ResultStatus.NotFound } => new NotFoundResult(),
            { IsSuccess: true } => new NoContentResult(),
            { IsSuccess: false } => new BadRequestObjectResult(result),
            _ => new ConflictObjectResult(result),
        };
    }
}

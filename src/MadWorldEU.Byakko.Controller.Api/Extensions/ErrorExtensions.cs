using MadWorldEU.Byakko.Common;
using MadWorldEU.Byakko.Functional;

namespace MadWorldEU.Byakko.Extensions;

public static class ErrorExtensions
{
    public static IResult ToBadRequest(this Error error) => 
        Results.BadRequest(error.ToFailureResponse(StatusCodes.Status400BadRequest));

    public static IResult ToConflict(this Error error) => 
        Results.Conflict(error.ToFailureResponse(StatusCodes.Status409Conflict));
    
    public static IResult ToNotFound(this Error error) => 
        Results.NotFound(error.ToFailureResponse(StatusCodes.Status404NotFound));

    private static FailureResponse ToFailureResponse(this Error error, int statusCode)
    {
        return new FailureResponse()
        {
            Code = error.Code,
            StatusCode = statusCode,
            Description = error.Description
        };
    }
}
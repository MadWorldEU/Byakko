using System.Diagnostics.CodeAnalysis;
using MadWorldEU.Byakko.Common;

namespace MadWorldEU.Byakko.Responses;

public sealed class ResultResponse<TResponse>
{
    public TResponse? Response { get; set; }
    public FailureResponse? Failure { get; set; }

    private ResultResponse(TResponse response)
    {
        Response = response;
    }
    
    private ResultResponse(FailureResponse failure)
    {
        Failure = failure;
    }
    
    public bool IsSuccess => Failure == null;
    
    public static implicit operator ResultResponse<TResponse>(TResponse response) => new(response);
    public static implicit operator ResultResponse<TResponse>(FailureResponse failure) => new(failure);
}
using System.Net;

namespace Partner_Integration_BFF.Contracts.Responses;

public sealed record ApiResponse<T>(
    bool IsSuccess,
    string Message,
    HttpStatusCode StatusCode,
    T Data);
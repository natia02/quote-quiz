using System.Security.Claims;

namespace QuoteQuiz.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(claim!.Value);
    }
}
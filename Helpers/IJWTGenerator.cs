using System.Security.Claims;
using cinema.Models;
using FluentResults;

namespace cinema.Helpers;

public interface IJWTGenerator
{
    public IResult<string> Generate(Player caller);
    public IResult<ClaimsPrincipal> ValidateAndDecrypt(string token);
}
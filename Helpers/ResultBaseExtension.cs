using FluentResults;

namespace cinema.Helpers;


public static class ResultBaseExtension
{
    public static List<string> ToResultErrorList(this List<IError> errors)
    {
        List<string> list = [];
        errors.ForEach(err => list.Add(err.Message));
        return list;
    }
}
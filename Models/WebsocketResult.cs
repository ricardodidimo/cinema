namespace cinema.Models;


public class WebsocketResult
{
    public object? Data { get; set; }
    public bool Successful { get; set; }
    public List<string>? Errors { get; set; }


    public static WebsocketResult Fail(List<string> errors)
    {
        return new WebsocketResult
        {
            Successful = false,
            Errors = errors
        };
    }

    public static WebsocketResult Ok(Object? data)
    {
        return new WebsocketResult
        {
            Successful = true,
            Data = data
        };
    }
}
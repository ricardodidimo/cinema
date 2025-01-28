using cinema.Models;
using FluentResults;

namespace cinema.Repositories.RoomRepository;

public interface IRoomRepository
{
    public Task<IResult<MoviePickRoom?>> FindByIdentifier(string id);
    public Task<IResult<MoviePickRoom>> Add(MoviePickRoom room);
    public Task<IResult<MoviePickRoom>> Update(MoviePickRoom room);
    public Task<IResult<MoviePickRoom>> Delete(MoviePickRoom room);
    public Task<IResult<MoviePickRoom>> AddPlayer(MoviePickRoom room, Player player);
    public Task<IResult<MoviePickRoom>> RemovePlayer(MoviePickRoom room, Player player);
    public Task<IResult<MoviePickRoom>> ConfirmRound(MoviePickRoom room, Player player);
}
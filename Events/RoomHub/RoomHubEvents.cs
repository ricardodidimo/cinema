namespace cinema.Events.RoomHub;


public class RoomHubEvents
{
    /// <summary>
    /// Default event for an failed request that requires a retry call.
    /// </summary>
    public static readonly string RETRY = "RETRY";

    /// <summary>
    /// Event that indicates that a new identity was created for a connection and a new token issued.
    /// </summary>
    public static readonly string SUBSCRIBED = "SUBSCRIBED";

    /// <summary>
    /// Event that indicates that the caller successfully created a 'room'/connection group.
    /// </summary>
    public static readonly string CREATED_ROOM = "CREATED_ROOM";

    /// <summary>
    /// Event that indicates that the caller successfully joined a 'room'/connection group.
    /// </summary>
    public static readonly string JOINED = "JOINED";

    /// <summary>
    /// Event that communicates to other 'players' in the connection group that a new connection joined.
    /// </summary>
    public static readonly string PLAYER_JOINED = "PLAYER_JOINED";

    /// <summary>
    /// Event that indicates that the caller successfully left a 'room'/connection group.
    /// </summary>
    public static readonly string LEFT = "LEFT";

    /// <summary>
    /// Event that communicates to other 'players' in the group that a connection disconnected.
    /// </summary>
    public static readonly string PLAYER_LEFT = "PLAYER_LEFT";

    /// <summary>
    /// Event that indicates to the connection group that a player confirmed the next round.
    /// </summary>
    public static readonly string CONFIRMED_ROUND = "CONFIRMED_ROUND";

    /// <summary>
    /// Event that indicates to the connection group that a new round can begin.
    /// </summary>
    public static readonly string ROUND_READY = "ROUND_READY";

    /// <summary>
    /// Event that indicates to the connection group that the round ended and to be aware of the receiving suggestions.
    /// </summary>
    public static readonly string ROUND_FINISHED = "ROUND_FINISHED";

    /// <summary>
    /// Event that indicates that a approval vote for a movie suggested was casted
    /// </summary>
    public static readonly string VOTE_CASTED = "VOTE_CASTED";

    /// <summary>
    /// Event that indicates that all confirmed member of the room voted in approval of a certain movie suggestion
    /// </summary>
    public static readonly string MOVIE_PICKED = "MOVIE_PICKED";
}
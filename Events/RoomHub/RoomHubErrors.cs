namespace cinema.Events.RoomHub;


public class RoomHubErrors
{
    public static readonly string FAILED_GENERATE_TOKEN = "UNABLE TO GENERATE IDENTITY TOKEN";
    public static readonly string UNAUTHORIZED_ACTION = "UNAUTHORIZED";
    public static readonly string CONNECT_NOT_IN_GROUP = "PLAYER IS NOT A PARTICIPANT IN ROOM";
    public static readonly string IDENTITY_INFO_MISSING = "IDENTITY MISSING. RE-ENTRY REQUIRED";
    public static readonly string ACTIVE_ROOM_NOT_FOUND = "NO ACTIVE ROOM WAS FOUND";
    public static readonly string ACTIVE_SUGGESTION_NOT_FOUND = "NO ACTIVE SUGGESTION WAS FOUND";
    public static readonly string PLAYER_ALREADY_CONFIRMED = "PLAYER ALREADY CONFIRMED ROUND";
    public static readonly string PLAYER_ALREADY_VOTED = "PLAYER ALREADY CASTED A VOTE THIS ROUND";
    public static readonly string ROOM_IS_FULL = "ROOM REACHED MAXIMUM CAPACITY";
    public static readonly string UNABLE_CREATE_ROOM = "UNABLE TO CREATE ROOM";
    public static readonly string UNABLE_UPDATE_ROOM = "UNABLE TO UPDATE ROOM";
    public static readonly string ROOM_CONFIRMATIONS_PENDING = "ROOM HAS CONFIRMATIONS LEFT PENDING";
    public static readonly string SUGGESTIONS_API_ERROR = "UNABLE TO GATHER SUGGESTIONS";
}
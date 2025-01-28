using System.Security.Claims;
using System.Text.Json;
using cinema.Events.RoomHub;
using cinema.Events.RoomHub.CastVote;
using cinema.Events.RoomHub.ChangeOwnership;
using cinema.Events.RoomHub.ConfirmRound;
using cinema.Events.RoomHub.CreateRoom;
using cinema.Events.RoomHub.JoinRoom;
using cinema.Events.RoomHub.LeaveRoom;
using cinema.Events.RoomHub.PlayRound;
using cinema.Events.RoomHub.Subscribe;
using cinema.Helpers;
using cinema.Models;
using cinema.Repositories.RoomRepository;
using FluentResults;
using Microsoft.AspNetCore.SignalR;

namespace cinema
{
    public class RoomHub(IJWTGenerator _JWTGenerator, ICreateRoomEvent createRoomEvent,
    IJoinRoomEvent joinRoomEvent, ILeaveRoomEvent leaveRoomEvent, IConfirmRoundEvent confirmRoundEvent,
    IPlayRoundEvent playRoundEvent, ICastVoteEvent castVoteEvent, IChangeOwnershipEvent changeOwnershipEvent) : Hub
    {
        static readonly string CURRENT_ROOM_INFO_KEY = "CURRENT_ROOM";
        static readonly string CURRENT_PLAYER_INFO_KEY = "CURRENT_PLAYER";
        public static readonly string API_ENDPOINT_ENV_KEY = "api_get_movies_endpoint";
        public static readonly string API_AUTHKEY_ENV_KEY = "api_key";

        private void StoreConnectionInfo(string key, Object item)
        {
            Context.Items.Add(key, item);
        }

        private void RemoveConnectionInfo(string key)
        {
            Context.Items.Remove(key);
        }

        private Result<Player> GetPlayer()
        {
            object? playerId = Context.Items.FirstOrDefault(infos => (string)infos.Key == CURRENT_PLAYER_INFO_KEY).Value;
            if (playerId is null)
            {
                return Result.Fail<Player>("No Identity");
            }

            return (Player)playerId;
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            object? connectedRoomId = Context.Items.FirstOrDefault(items => (string)items.Key == CURRENT_ROOM_INFO_KEY).Value;
            object? connectedPlayer = Context.Items.FirstOrDefault(items => (string)items.Key == CURRENT_PLAYER_INFO_KEY).Value;

            if (connectedRoomId is not null && connectedPlayer is not null)
            {
                LeaveRoomRequest request = new() { RoomCode = (string)connectedRoomId };
                IResult<MoviePickRoom> room = await leaveRoomEvent.Exec(request, (Player)connectedPlayer!);
                await Clients.Group((string)connectedRoomId).SendAsync(RoomHubEvents.PLAYER_LEFT, WebsocketResult.Ok(connectedPlayer));

                if (room.Value.PlayersConnectedTotal > 0 && room.Value.OwnerId == ((Player)connectedPlayer).Identifier)
                {
                    ChangeOwnershipRequest passRoomOwnershipRequest = new() { RoomCode = (string)connectedRoomId };
                    IResult<MoviePickRoom> updatedRoom = await changeOwnershipEvent.Exec(passRoomOwnershipRequest, null);
                    await Clients.Group((string)connectedRoomId).SendAsync(RoomHubEvents.OWNERSHIP_CHANGED, WebsocketResult.Ok(updatedRoom.Value));
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task Subscribe(SubscribeRequest request)
        {
            Player identity = new()
            {
                Identifier = Context.ConnectionId,
                Avatar = request.Avatar,
                Name = request.Name,
                Preferences = request.Preferences
            };

            this.StoreConnectionInfo(CURRENT_PLAYER_INFO_KEY, identity);
            await Clients.Caller.SendAsync(RoomHubEvents.SUBSCRIBED, WebsocketResult.Ok(identity));
        }

        public async Task CreateRoom(CreateRoomRequest request)
        {
            var playerDecode = GetPlayer();
            if (playerDecode.IsFailed)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.CREATED_ROOM, WebsocketResult.Fail(playerDecode.Errors.ToResultErrorList()));
                return;
            }

            object? connectedRoomId = Context.Items.FirstOrDefault(infos => (string)infos.Key == CURRENT_ROOM_INFO_KEY).Value;
            var isAlreadyConnectedToRoom = connectedRoomId is not null;
            if (isAlreadyConnectedToRoom)
            {
                LeaveRoomRequest leaveRequest = new() { RoomCode = (string)connectedRoomId! };
                var removalOperation = await leaveRoomEvent.Exec(leaveRequest, playerDecode.Value);
                if (removalOperation.IsFailed)
                {
                    await Clients.Caller.SendAsync(RoomHubEvents.CREATED_ROOM, WebsocketResult.Fail(removalOperation.Errors.ToResultErrorList()));
                    return;
                }

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, (string)connectedRoomId!);
                this.RemoveConnectionInfo(CURRENT_ROOM_INFO_KEY);
            }

            var createRoomOp = await createRoomEvent.Exec(request, playerDecode.Value);
            if (createRoomOp.IsFailed)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.CREATED_ROOM, WebsocketResult.Fail(createRoomOp.Errors.ToResultErrorList()));
                return;
            }

            MoviePickRoom createdRoom = createRoomOp.Value;
            await Groups.AddToGroupAsync(Context.ConnectionId, createdRoom.Identifier);
            this.StoreConnectionInfo(CURRENT_ROOM_INFO_KEY, createdRoom.Identifier);
            await Clients.Caller.SendAsync(RoomHubEvents.CREATED_ROOM, WebsocketResult.Ok(CreateRoomResponse.ToResponse(createdRoom)));
        }

        public async Task JoinRoom(JoinRoomRequest request)
        {
            var playerDecode = GetPlayer();
            if (playerDecode.IsFailed)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.JOINED, WebsocketResult.Fail(playerDecode.Errors.ToResultErrorList()));
                return;
            }

            var connectedRoomId = Context.Items.FirstOrDefault(infos => (string)infos.Key == CURRENT_ROOM_INFO_KEY).Value;
            if (connectedRoomId != null)
            {
                var removalOperation = await leaveRoomEvent.Exec(new() { RoomCode = (string)connectedRoomId }, playerDecode.Value);
                if (removalOperation.IsFailed)
                {
                    await Clients.Caller.SendAsync(RoomHubEvents.JOINED, WebsocketResult.Fail(removalOperation.Errors.ToResultErrorList()));
                    return;
                }

                this.RemoveConnectionInfo(CURRENT_ROOM_INFO_KEY);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, (string)connectedRoomId);
            }

            var joinOpertion = await joinRoomEvent.Exec(request, playerDecode.Value);
            if (joinOpertion.IsFailed)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.JOINED, WebsocketResult.Fail(joinOpertion.Errors.ToResultErrorList()));
                return;
            }

            MoviePickRoom roomEntry = joinOpertion.Value;
            this.StoreConnectionInfo(CURRENT_ROOM_INFO_KEY, roomEntry.Identifier);
            await Groups.AddToGroupAsync(Context.ConnectionId, request.RoomCode);

            await Clients.Group(roomEntry.Identifier).SendAsync(RoomHubEvents.PLAYER_JOINED, WebsocketResult.Ok(playerDecode.Value));
            await Clients.Caller.SendAsync(RoomHubEvents.JOINED, WebsocketResult.Ok(JoinRoomResponse.ToResponse(roomEntry)));
        }

        public async Task LeaveRoom(LeaveRoomRequest request)
        {
            var playerDecode = GetPlayer();
            if (playerDecode.IsFailed)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.LEFT, WebsocketResult.Fail(playerDecode.Errors.ToResultErrorList()));
                return;
            }

            var connectedRoomId = Context.Items.FirstOrDefault(infos => (string)infos.Key == CURRENT_ROOM_INFO_KEY).Value;
            if (connectedRoomId is null || (string)connectedRoomId != request.RoomCode)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.LEFT, WebsocketResult.Fail([RoomHubErrors.CONNECT_NOT_IN_GROUP]));
                return;
            }

            var removalOperation = await leaveRoomEvent.Exec(request, playerDecode.Value);
            if (removalOperation.IsFailed)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.LEFT, WebsocketResult.Fail(removalOperation.Errors.ToResultErrorList()));
                return;
            }

            this.RemoveConnectionInfo(CURRENT_ROOM_INFO_KEY);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, request.RoomCode);

            await Clients.Group((string)connectedRoomId).SendAsync(RoomHubEvents.PLAYER_LEFT, WebsocketResult.Ok(playerDecode));
            await Clients.Caller.SendAsync(RoomHubEvents.LEFT, WebsocketResult.Ok(LeaveRoomResponse.ToResponse(removalOperation.Value)));

            MoviePickRoom room = removalOperation.Value;
            if (room.PlayersConnectedTotal > 0 && room.OwnerId == playerDecode.Value.Identifier)
            {
                ChangeOwnershipRequest passRoomOwnershipRequest = new() { RoomCode = (string)connectedRoomId };
                IResult<MoviePickRoom> updatedRoom = await changeOwnershipEvent.Exec(passRoomOwnershipRequest, null);
                await Clients.Group((string)connectedRoomId).SendAsync(RoomHubEvents.OWNERSHIP_CHANGED, WebsocketResult.Ok(updatedRoom.Value));
            }
        }

        public async Task ConfirmRound(ConfirmRoundRequest request)
        {
            var playerDecodeOp = GetPlayer();
            var playerDecode = playerDecodeOp.Value;
            if (playerDecodeOp.IsFailed)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.CONFIRMED_ROUND, WebsocketResult.Fail(playerDecodeOp.Errors.ToResultErrorList()));
                return;
            }

            var connectedRoomId = Context.Items.FirstOrDefault(infos => (string)infos.Key == CURRENT_ROOM_INFO_KEY).Value;
            if (connectedRoomId is null || (string)connectedRoomId != request.RoomCode)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.CONFIRMED_ROUND, WebsocketResult.Fail([RoomHubErrors.CONNECT_NOT_IN_GROUP]));
                return;
            }

            var confirmOperation = await confirmRoundEvent.Exec(request, playerDecode);
            if (confirmOperation.IsFailed)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.CONFIRMED_ROUND, WebsocketResult.Fail(confirmOperation.Errors.ToResultErrorList()));
                return;
            }

            await Clients.Group((string)connectedRoomId).SendAsync(RoomHubEvents.CONFIRMED_ROUND,
                WebsocketResult.Ok(confirmOperation.Value));

            if (confirmOperation.Value.CanBeginRound)
            {
                await Clients.Group((string)connectedRoomId).SendAsync(RoomHubEvents.ROUND_READY, WebsocketResult.Ok(null));
            }
        }

        public async Task PlayRound(PlayRoundRequest request)
        {
            var playerDecodeOp = GetPlayer();
            var playerDecode = playerDecodeOp.Value;
            if (playerDecodeOp.IsFailed)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.ROUND_FINISHED, WebsocketResult.Fail(playerDecodeOp.Errors.ToResultErrorList()));
                return;
            }

            var playRoundOperation = await playRoundEvent.Exec(request, playerDecode);
            if (playRoundOperation.IsFailed)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.ROUND_FINISHED, WebsocketResult.Fail(playRoundOperation.Errors.ToResultErrorList()));
                return;
            }

            await Clients.Group(request.RoomCode).SendAsync(RoomHubEvents.ROUND_FINISHED, WebsocketResult.Ok(playRoundOperation.Value));
        }

        public async Task CastVote(CastVoteRequest request)
        {
            var playerDecodeOp = GetPlayer();
            var playerDecode = playerDecodeOp.Value;
            if (playerDecodeOp.IsFailed)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.VOTE_CASTED, WebsocketResult.Fail(playerDecodeOp.Errors.ToResultErrorList()));
                return;
            }

            var castVoteOperation = await castVoteEvent.Exec(request, playerDecode);
            if (castVoteOperation.IsFailed)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.VOTE_CASTED, WebsocketResult.Fail(castVoteOperation.Errors.ToResultErrorList()));
                return;
            }

            await Clients.Caller.SendAsync(RoomHubEvents.VOTE_CASTED, WebsocketResult.Ok(null));
            if (castVoteOperation.Value.RoomReachedConsensus)
            {
                await Clients.Group(request.RoomCode).SendAsync(RoomHubEvents.MOVIE_PICKED, WebsocketResult.Ok(castVoteOperation.Value.Movie));
            }
        }
    }
}

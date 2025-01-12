using System.Security.Claims;
using System.Text.Json;
using cinema.Events.RoomHub;
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
    public class RoomHub(IJWTGenerator _JWTGenerator, IRoomRepository _roomRepository,
    ICreateRoomEvent createRoomEvent, IJoinRoomEvent joinRoomEvent, ILeaveRoomEvent leaveRoomEvent,
    IConfirmRoundEvent confirmRoundEvent, IPlayRoundEvent playRoundEvent) : Hub
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

        private Result<Player> GetPlayerFromToken(string token)
        {
            var tokenValidation = _JWTGenerator.ValidateAndDecrypt(token);
            if (tokenValidation.IsFailed)
            {
                return Result.Fail<Player>(tokenValidation.Errors);
            }

            Claim? identityObj = tokenValidation.Value.Claims.FirstOrDefault(claims => claims.Type == JWTGenerator.WS_IDENTITY_CLAIM);
            if (identityObj is null)
            {
                return Result.Fail<Player>([RoomHubErrors.IDENTITY_INFO_MISSING]);
            }

            var identity = JsonSerializer.Deserialize<Player>(identityObj.Value);
            if (identity is null)
            {
                return Result.Fail<Player>([RoomHubErrors.IDENTITY_INFO_MISSING]);
            }

            return Result.Ok(identity);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            object? connectedRoomId = Context.Items.FirstOrDefault(items => (string)items.Key == CURRENT_ROOM_INFO_KEY).Value;
            object? connectedPlayer = Context.Items.FirstOrDefault(items => (string)items.Key == CURRENT_PLAYER_INFO_KEY).Value;

            if (connectedRoomId is not null && connectedPlayer is not null)
            {
                Player? player = JsonSerializer.Deserialize<Player>((string)connectedPlayer);
                LeaveRoomRequest request = new() { RoomCode = (string)connectedRoomId };
                await leaveRoomEvent.Exec(request, player!);
                await Clients.Group((string)connectedRoomId).SendAsync(RoomHubEvents.PLAYER_LEFT, WebsocketResult.Ok(connectedPlayer));
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task Subscribe(SubscribeRequest request)
        {
            Player identity = new()
            {
                Identifier = Guid.NewGuid().ToString(),
                Avatar = request.Avatar,
                Name = request.Name,
                Preferences = request.Preferences
            };

            IResult<string> tokenGeneration = _JWTGenerator.Generate(identity);
            if (tokenGeneration.IsFailed)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.RETRY, WebsocketResult.Fail([RoomHubErrors.FAILED_GENERATE_TOKEN]));
                return;
            }

            this.StoreConnectionInfo(CURRENT_PLAYER_INFO_KEY, identity);
            await Clients.Caller.SendAsync(RoomHubEvents.SUBSCRIBED, WebsocketResult.Ok(new
            {
                token = tokenGeneration.Value
            }));
        }

        public async Task CreateRoom(string token, CreateRoomRequest request)
        {
            var playerDecode = GetPlayerFromToken(token);
            if (playerDecode.IsFailed)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.RETRY, WebsocketResult.Fail(playerDecode.Errors.ToResultErrorList()));
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
                    await Clients.Caller.SendAsync(RoomHubEvents.RETRY, WebsocketResult.Fail(removalOperation.Errors.ToResultErrorList()));
                    return;
                }

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, (string)connectedRoomId!);
                this.RemoveConnectionInfo(CURRENT_ROOM_INFO_KEY);
            }

            var createRoomOp = await createRoomEvent.Exec(request, playerDecode.Value);
            if (createRoomOp.IsFailed)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.RETRY, WebsocketResult.Fail(createRoomOp.Errors.ToResultErrorList()));
                return;
            }

            MoviePickRoom createdRoom = createRoomOp.Value;
            await Groups.AddToGroupAsync(Context.ConnectionId, createdRoom.Identifier);
            this.StoreConnectionInfo(CURRENT_ROOM_INFO_KEY, createdRoom.Identifier);
            await Clients.Caller.SendAsync(RoomHubEvents.CREATED_ROOM, WebsocketResult.Ok(CreateRoomResponse.ToResponse(createdRoom)));
        }

        public async Task JoinRoom(string token, JoinRoomRequest request)
        {
            var playerDecode = GetPlayerFromToken(token);
            if (playerDecode.IsFailed)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.RETRY, WebsocketResult.Fail(playerDecode.Errors.ToResultErrorList()));
                return;
            }

            var connectedRoomId = Context.Items.FirstOrDefault(infos => (string)infos.Key == CURRENT_ROOM_INFO_KEY).Value;
            if (connectedRoomId != null)
            {
                var removalOperation = await leaveRoomEvent.Exec(new() { RoomCode = (string)connectedRoomId }, playerDecode.Value);
                if (removalOperation.IsFailed)
                {
                    await Clients.Caller.SendAsync(RoomHubEvents.RETRY, WebsocketResult.Fail(removalOperation.Errors.ToResultErrorList()));
                    return;
                }

                this.RemoveConnectionInfo(CURRENT_ROOM_INFO_KEY);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, (string)connectedRoomId);
            }

            var joinOpertion = await joinRoomEvent.Exec(request, playerDecode.Value);
            if (joinOpertion.IsFailed)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.RETRY, WebsocketResult.Fail(joinOpertion.Errors.ToResultErrorList()));
                return;
            }

            MoviePickRoom roomEntry = joinOpertion.Value;
            this.StoreConnectionInfo(CURRENT_ROOM_INFO_KEY, roomEntry.Identifier);
            await Groups.AddToGroupAsync(Context.ConnectionId, request.RoomCode);

            await Clients.Group(roomEntry.Identifier).SendAsync(RoomHubEvents.PLAYER_JOINED, WebsocketResult.Ok(playerDecode.Value));
            await Clients.Caller.SendAsync(RoomHubEvents.JOINED, WebsocketResult.Ok(JoinRoomResponse.ToResponse(roomEntry)));
        }

        public async Task LeaveRoom(string token, LeaveRoomRequest request)
        {
            var playerDecode = GetPlayerFromToken(token);
            if (playerDecode.IsFailed)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.RETRY, WebsocketResult.Fail(playerDecode.Errors.ToResultErrorList()));
                return;
            }

            var connectedRoomId = Context.Items.FirstOrDefault(infos => (string)infos.Key == CURRENT_ROOM_INFO_KEY).Value;
            if (connectedRoomId is null || (string)connectedRoomId != request.RoomCode)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.RETRY, WebsocketResult.Fail([RoomHubErrors.CONNECT_NOT_IN_GROUP]));
                return;
            }

            var removalOperation = await leaveRoomEvent.Exec(request, playerDecode.Value);
            if (removalOperation.IsFailed)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.RETRY, WebsocketResult.Fail(removalOperation.Errors.ToResultErrorList()));
                return;
            }

            this.RemoveConnectionInfo(CURRENT_ROOM_INFO_KEY);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, request.RoomCode);

            await Clients.Group((string)connectedRoomId).SendAsync(RoomHubEvents.PLAYER_LEFT, WebsocketResult.Ok(playerDecode));
            await Clients.Caller.SendAsync(RoomHubEvents.LEFT, WebsocketResult.Ok(LeaveRoomResponse.ToResponse(removalOperation.Value)));
        }

        public async Task ConfirmRound(string token, ConfirmRoundRequest request)
        {
            var playerDecodeOp = GetPlayerFromToken(token);
            var playerDecode = playerDecodeOp.Value;
            if (playerDecodeOp.IsFailed)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.RETRY, WebsocketResult.Fail(playerDecodeOp.Errors.ToResultErrorList()));
                return;
            }

            var connectedRoomId = Context.Items.FirstOrDefault(infos => (string)infos.Key == CURRENT_ROOM_INFO_KEY).Value;
            if (connectedRoomId is null || (string)connectedRoomId != request.RoomCode)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.RETRY, WebsocketResult.Fail([RoomHubErrors.CONNECT_NOT_IN_GROUP]));
                return;
            }

            var confirmOperation = await confirmRoundEvent.Exec(request, playerDecode);
            if (confirmOperation.IsFailed)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.RETRY, WebsocketResult.Fail(confirmOperation.Errors.ToResultErrorList()));
                return;
            }

            await Clients.Group((string)connectedRoomId).SendAsync(RoomHubEvents.CONFIRMED_ROUND,
                WebsocketResult.Ok(ConfirmRoundResponse.ToResponse(confirmOperation.Value, playerDecode)));

            if (confirmOperation.Value.CanBeginRound)
            {
                await Clients.Group((string)connectedRoomId).SendAsync(RoomHubEvents.ROUND_READY, WebsocketResult.Ok(null));
            }
        }

        public async Task PlayRound(string token, PlayRoundRequest request)
        {
            var playerDecodeOp = GetPlayerFromToken(token);
            var playerDecode = playerDecodeOp.Value;
            if (playerDecodeOp.IsFailed)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.RETRY, WebsocketResult.Fail(playerDecodeOp.Errors.ToResultErrorList()));
                return;
            }

            var playRoundOperation = await playRoundEvent.Exec(request, playerDecode);
            if (playRoundOperation.IsFailed)
            {
                await Clients.Caller.SendAsync(RoomHubEvents.RETRY, WebsocketResult.Fail(playRoundOperation.Errors.ToResultErrorList()));
                return;
            }

            await Clients.Group(request.RoomCode).SendAsync(RoomHubEvents.CONFIRMED_ROUND, WebsocketResult.Ok(playRoundOperation.Value));
        }
    }
}

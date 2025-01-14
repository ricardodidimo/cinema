using System.Text;
using cinema;
using cinema.Events.RoomHub.CastVote;
using cinema.Events.RoomHub.ConfirmRound;
using cinema.Events.RoomHub.CreateRoom;
using cinema.Events.RoomHub.JoinRoom;
using cinema.Events.RoomHub.LeaveRoom;
using cinema.Events.RoomHub.PlayRound;
using cinema.Helpers;
using cinema.Repositories.RoomRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IJWTGenerator, JWTGenerator>();
builder.Services.AddTransient<ICreateRoomEvent, CreateRoom>();
builder.Services.AddTransient<IJoinRoomEvent, JoinRoom>();
builder.Services.AddTransient<ILeaveRoomEvent, LeaveRoom>();
builder.Services.AddTransient<IConfirmRoundEvent, ConfirmRound>();
builder.Services.AddTransient<IPlayRoundEvent, PlayRound>();
builder.Services.AddTransient<ICastVoteEvent, CastVote>();
builder.Services.AddTransient<IRoomRepository, RoomRepositoryMem>();

builder.Services.AddHttpClient();
builder.Services.AddSignalR(h => h.EnableDetailedErrors = true);
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(x =>
    {
        var key = builder.Configuration[JWTGenerator.JWT_ENCRYPTION_KEY_ID] ?? throw new ArgumentNullException(JWTGenerator.JWT_ENCRYPTION_KEY_ID);
        x.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
            ValidateIssuerSigningKey = true,
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateLifetime = true,
        };
    });

var app = builder.Build();
app.MapHub<RoomHub>("/room");
app.UseAuthentication();
app.UseAuthorization();
app.Run();
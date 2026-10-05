using LogiTrack.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LogiTrackContext>();
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
using ChatAppServer.WebAPI;
using ChatAppServer.WebAPI.Services;
using ChatAppServer.WebAPI.SignalR;

var builder = WebApplication.CreateBuilder(args);
var origins = "*";
//builder.Services.AddDefaultCors();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.AllowAnyHeader();
        builder.AllowAnyMethod();
        if (origins is not { Length: > 0 }) return;

        if (origins.Contains("*"))
        {
            builder.AllowAnyHeader();
            builder.AllowAnyMethod();
            builder.SetIsOriginAllowed(host => true);
            builder.AllowCredentials();
        }
        else
        {
            builder.WithOrigins(origins);
        }
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ConnectionTracker>();
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("MessengerApplicationDatabase"));

builder.Services.AddScoped<DatabaseProviderService>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IMessagesService, MessagesService>();

builder.Services.AddSignalR((e => { e.EnableDetailedErrors = true; }));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/chat-hub");
app.Run();

using VoiceRecog.WebAPI;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddTransient<AudioService>();

var app = builder.Build();


app.MapGet("/", () => "Hello audio world!");
app.MapPost("/audio", StreamAudioAsync);


await app.RunAsync();

static async Task StreamAudioAsync(HttpRequest req,
                                   AudioService service,
                                   CancellationToken cancellationToken = default)
{
    await using var stream = req.Body;
    await service.ReadAudioStreamAsync(stream, cancellationToken);
}
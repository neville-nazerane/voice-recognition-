using VoiceRecog.Background;
using VoiceRecog.Background.Workers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>()
                .AddHostedService<ReadVoiceWorker>();



var host = builder.Build();

await host.RunAsync();

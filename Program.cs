using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using SpanishWordLearner.Components;
using SpanishWordLearner.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();
builder.Services.AddSingleton<WordService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProgressService>();

var host = builder.Build();

// Eagerly trigger vocabulary dataset load and auth check
var wordService = host.Services.GetRequiredService<WordService>();
_ = wordService.InitializeAsync();

var authService = host.Services.GetRequiredService<AuthService>();
_ = authService.InitializeAsync();

await host.RunAsync();

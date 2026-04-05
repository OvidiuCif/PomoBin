using Microsoft.AspNetCore.Mvc;
using PomoApp.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Settings>();
builder.Services.AddSingleton<PomodoroTimerService>();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/timer/status", (PomodoroTimerService timer) => new
{
    isRunning         = timer.IsRunning,
    phase             = timer.CurrentPhase.ToString(),
    remaining         = timer.Remaining.ToString(@"mm\:ss"),
    completedSessions = timer.CompletedSessions
});

app.MapPost("/timer/start", (PomodoroTimerService timer) =>
{
    timer.Start();
    return Results.Ok(new { status = "started" });
});

app.MapPost("/timer/pause", (PomodoroTimerService timer) =>
{
    timer.Pause();
    return Results.Ok(new { status = "paused" });
});

app.MapPost("/timer/reset", (PomodoroTimerService timer) =>
{
    timer.Reset();
    return Results.Ok(new { status = "reset" });
});

app.MapPost("/timer/skip", (PomodoroTimerService timer) =>
{
    timer.Skip();
    return Results.Ok(new { status = "skipped", phase = timer.CurrentPhase.ToString() });
});

app.MapGet("/settings", (Settings settings) => settings);

app.MapPut("/settings", ([FromBody] Settings incoming, [FromServices] Settings current) =>
{
    current.WorkMinutes              = incoming.WorkMinutes;
    current.BreakMinutes             = incoming.BreakMinutes;
    current.LongBreakMinutes         = incoming.LongBreakMinutes;
    current.SessionsBeforeLongBreak  = incoming.SessionsBeforeLongBreak;
    return Results.Ok(current);
});

app.Run();

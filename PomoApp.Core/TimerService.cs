namespace PomoApp.Core;

public enum PomodoroPhase { Work, Break, LongBreak }

public class PomodoroTimerService
{
    private readonly Settings _settings;
    private CancellationTokenSource? _cts;
    private int _completedSessions;

    public PomodoroPhase CurrentPhase { get; private set; } = PomodoroPhase.Work;
    public bool IsRunning { get; private set; }
    public TimeSpan Remaining { get; private set; }
    public int CompletedSessions => _completedSessions;

    public event Action<TimeSpan>? Tick;
    public event Action<PomodoroPhase>? PhaseChanged;
    public event Action? SessionCompleted;

    public PomodoroTimerService(Settings settings)
    {
        _settings = settings;
        Remaining = TimeSpan.FromMinutes(settings.WorkMinutes);
    }

    public void Start()
    {
        if (IsRunning) return;
        _cts = new CancellationTokenSource();
        IsRunning = true;
        _ = RunAsync(_cts.Token);
    }

    public void Pause()
    {
        _cts?.Cancel();
        IsRunning = false;
    }

    public void Reset()
    {
        Pause();
        _completedSessions = 0;
        CurrentPhase = PomodoroPhase.Work;
        Remaining = TimeSpan.FromMinutes(_settings.WorkMinutes);
        Tick?.Invoke(Remaining);
    }

    public void Skip()
    {
        Pause();
        AdvancePhase();
    }

    private async Task RunAsync(CancellationToken ct)
    {
        while (Remaining > TimeSpan.Zero && !ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(1000, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            Remaining = Remaining.Subtract(TimeSpan.FromSeconds(1));
            Tick?.Invoke(Remaining);
        }

        if (!ct.IsCancellationRequested && Remaining <= TimeSpan.Zero)
        {
            IsRunning = false;
            if (CurrentPhase == PomodoroPhase.Work)
            {
                _completedSessions++;
                SessionCompleted?.Invoke();
            }
            AdvancePhase();
        }
    }

    private void AdvancePhase()
    {
        if (CurrentPhase == PomodoroPhase.Work)
        {
            CurrentPhase = _completedSessions > 0 && _completedSessions % _settings.SessionsBeforeLongBreak == 0
                ? PomodoroPhase.LongBreak
                : PomodoroPhase.Break;
        }
        else
        {
            CurrentPhase = PomodoroPhase.Work;
        }

        Remaining = CurrentPhase switch
        {
            PomodoroPhase.Work     => TimeSpan.FromMinutes(_settings.WorkMinutes),
            PomodoroPhase.Break    => TimeSpan.FromMinutes(_settings.BreakMinutes),
            PomodoroPhase.LongBreak => TimeSpan.FromMinutes(_settings.LongBreakMinutes),
            _                      => TimeSpan.FromMinutes(_settings.WorkMinutes)
        };

        PhaseChanged?.Invoke(CurrentPhase);
        Tick?.Invoke(Remaining);
    }
}

using System.Windows;
using System.Windows.Media;
using PomoApp.Core;

namespace PomoApp.Desktop;

public partial class MainWindow : Window
{
    private readonly PomodoroTimerService _timer;

    private static readonly SolidColorBrush _workBg = new(Color.FromRgb(175, 73, 73));
    private static readonly SolidColorBrush _breakBg = new(Color.FromRgb(41, 116, 121));
    private static readonly SolidColorBrush _longBreakBg = new(Color.FromRgb(47, 106, 149));
    private static readonly SolidColorBrush _workLabel = new(Color.FromRgb(255, 209, 209));
    private static readonly SolidColorBrush _brkLabel = new(Color.FromRgb(178, 223, 220));
    private static readonly SolidColorBrush _longBrkLabel = new(Color.FromRgb(178, 210, 235));

    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        _timer = new PomodoroTimerService(new Settings
        {
            WorkMinutes             = 0.1,  // ~6 seconds
            BreakMinutes            = 0.05, // ~3 seconds
            LongBreakMinutes        = 0.1,  // ~6 seconds
            SessionsBeforeLongBreak = 4
        });
#else
        _timer = new PomodoroTimerService(new Settings());
#endif
        _timer.Tick += OnTick;
        _timer.PhaseChanged += OnPhaseChanged;
        _timer.SessionCompleted += OnSessionCompleted;
    }

    private void OnTick(TimeSpan remaining) =>
        Dispatcher.Invoke(() => TimerDisplay.Text = remaining.ToString(@"mm\:ss"));

    private void OnPhaseChanged(PomodoroPhase phase)
    {
        Dispatcher.Invoke(() =>
        {
            Background = phase switch
            {
                PomodoroPhase.Work => _workBg,
                PomodoroPhase.Break => _breakBg,
                PomodoroPhase.LongBreak => _longBreakBg,
                _ => _workBg
            };

            SessionLabel.Foreground = phase switch
            {
                PomodoroPhase.Work => _workLabel,
                PomodoroPhase.Break => _brkLabel,
                PomodoroPhase.LongBreak => _longBrkLabel,
                _ => _workLabel
            };

            PhaseLabel.Text = phase switch
            {
                PomodoroPhase.Work => "WORK FOCUS",
                PomodoroPhase.Break => "SHORT BREAK",
                PomodoroPhase.LongBreak => "LONG BREAK",
                _ => "WORK FOCUS"
            };

            StartPauseButton.Content = "START";
            SoundService.PlayPhase(phase);
        });
    }

    private void OnSessionCompleted() =>
        Dispatcher.Invoke(() =>
        {
            SoundService.PlayNotification();
            SessionLabel.Text = $"Sessions completed: {_timer.CompletedSessions}";
        });

    private void StartPause_Click(object sender, RoutedEventArgs e)
    {
        SoundService.PlayButtonClick();
        if (_timer.IsRunning)
        {
            _timer.Pause();
            StartPauseButton.Content = "START";
        }
        else
        {
            _timer.Start();
            StartPauseButton.Content = "PAUSE";
        }
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        _timer.Reset();
        Background = _workBg;
        StartPauseButton.Content = "START";
        PhaseLabel.Text = "WORK FOCUS";
        SessionLabel.Foreground = _workLabel;
        SessionLabel.Text = "Sessions completed: 0";
    }

    private void Skip_Click(object sender, RoutedEventArgs e)
    {
        _timer.Skip();
        StartPauseButton.Content = "START";
    }
}
using System.Windows;
using System.Windows.Media;
using PomoApp.Core;

namespace PomoApp.Desktop;

public partial class MainWindow : Window
{
    private readonly PomodoroTimerService _timer;

    public MainWindow()
    {
        InitializeComponent();
        _timer = new PomodoroTimerService(new Settings());
        _timer.Tick             += OnTick;
        _timer.PhaseChanged     += OnPhaseChanged;
        _timer.SessionCompleted += OnSessionCompleted;
    }

    private void OnTick(TimeSpan remaining) =>
        Dispatcher.Invoke(() => TimerDisplay.Text = remaining.ToString(@"mm\:ss"));

    private void OnPhaseChanged(PomodoroPhase phase)
    {
        Dispatcher.Invoke(() =>
        {
            PhaseLabel.Text = phase switch
            {
                PomodoroPhase.Work      => "Work",
                PomodoroPhase.Break     => "Short Break",
                PomodoroPhase.LongBreak => "Long Break",
                _                       => "Work"
            };
            PhaseLabel.Foreground    = phase == PomodoroPhase.Work ? Brushes.Crimson : Brushes.SteelBlue;
            StartPauseButton.Content = "Start";
        });
    }

    private void OnSessionCompleted() =>
        Dispatcher.Invoke(() =>
            SessionLabel.Text = $"Sessions completed: {_timer.CompletedSessions}");

    private void StartPause_Click(object sender, RoutedEventArgs e)
    {
        if (_timer.IsRunning)
        {
            _timer.Pause();
            StartPauseButton.Content = "Start";
        }
        else
        {
            _timer.Start();
            StartPauseButton.Content = "Pause";
        }
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        _timer.Reset();
        StartPauseButton.Content = "Start";
        PhaseLabel.Text           = "Work";
        PhaseLabel.Foreground     = Brushes.Crimson;
        SessionLabel.Text         = "Sessions completed: 0";
    }

    private void Skip_Click(object sender, RoutedEventArgs e)
    {
        _timer.Skip();
        StartPauseButton.Content = "Start";
    }
}
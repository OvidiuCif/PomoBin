using System.IO;
using System.Media;
using System.Windows;
using PomoApp.Core;

namespace PomoApp.Desktop;

internal static class SoundService
{
    public static void PlayPhase(PomodoroPhase phase)
    {
        var file = phase switch
        {
            PomodoroPhase.Work      => "work_start.wav",
            PomodoroPhase.Break     => "break_start.wav",
            PomodoroPhase.LongBreak => "long_break_start.wav",
            _                       => null
        };

        if (file is not null)
            Play($"Resources/Sounds/{file}");
    }

    public static void PlayButtonClick() => Play("Resources/Sounds/button_sound.wav");

    public static void PlayNotification() => Play("Resources/Sounds/notification_message.wav");

    private static void Play(string resourcePath)
    {
        try
        {
            var uri = new Uri($"pack://application:,,,/{resourcePath}");
            var info = Application.GetResourceStream(uri);
            if (info is null) return;

            using var ms = new MemoryStream();
            info.Stream.CopyTo(ms);
            ms.Position = 0;

            var player = new SoundPlayer(ms);
            player.Load();
            player.Play();
        }
        catch { }
    }
}

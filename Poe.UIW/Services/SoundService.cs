using System;
using System.Windows.Media;
using Poe.UIW.Properties;
using Serilog;

namespace Poe.UIW.Services;

public class SoundService
{
    private readonly MediaPlayer _player;
    private readonly Uri _defaultSoundUri;

    public const string DefaultNotificationPath = "defaultNotification.wav";

    public SoundService()
    {
        _player = new MediaPlayer();
        _defaultSoundUri = new Uri($"Resources/Sounds/{DefaultNotificationPath}", UriKind.Relative);
        _player.Open(new Uri($"Resources/Sounds/{UserSettings.Default.NotificationSoundPath}", UriKind.Relative));
        _player.MediaFailed += PlayerOnMediaFailed;
        _player.MediaEnded += PlayerOnMediaEnded;
    }

    public void Reset()
    {
        _player.Open(_defaultSoundUri);
    }

    public void Load(string path)
    {
        _player.Open(new Uri($"Resources/Sounds/{path}", UriKind.Relative));
    }

    private void PlayerOnMediaEnded(object sender, EventArgs e)
    {
        _player.Stop();
    }

    private void PlayerOnMediaFailed(object sender, ExceptionEventArgs e)
    {
        Log.Error("Error with file {File}. {Error}", _player.Source, e.ErrorException.Message);
        _player.Open(_defaultSoundUri);
    }

    public void Play()
    {
        if (UserSettings.Default.PlayNotificationSound)
        {
            _player.Play();
        }
    }
}
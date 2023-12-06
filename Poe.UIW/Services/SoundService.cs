using System;
using System.IO;
using System.Windows.Media;
using Poe.UIW.Properties;
using Serilog;
using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Contracts;

namespace Poe.UIW.Services;

public class SoundService
{
    private readonly MediaPlayer _player;
    private readonly MediaPlayer _testPlayer;
    private readonly Uri _defaultSoundUri;
    private string _currentFileName;
    private readonly ISnackbarService _snackbarService;

    public SoundService(ISnackbarService snackbarService)
    {
        _snackbarService = snackbarService;
        _player = new MediaPlayer();
        _testPlayer = new MediaPlayer();
        _defaultSoundUri = new Uri("Resources/Sounds/Simple ping.mp3", UriKind.Relative);
        _player.Open(new Uri($"Resources/Sounds/{UserSettings.Default.NotificationSoundPath}", UriKind.Relative));
        _player.MediaFailed += PlayerOnMediaFailed;
        _player.MediaEnded += PlayerOnMediaEnded;

        _testPlayer.MediaFailed += TestPlayerOnMediaFailed;
        _testPlayer.MediaEnded += PlayerOnMediaEnded;
    }

    public void TestPlay(string path)
    {
        var fileInAppFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/Sounds/", path);
        _testPlayer.Open(new Uri(fileInAppFolder, UriKind.Absolute));
        _testPlayer.Play();
    }

    public void TestPlay(Uri absolutPath)
    {
        if (absolutPath.IsAbsoluteUri)
        {
            _testPlayer.Open(absolutPath);
            _testPlayer.Play();
        }
    }

    public void Load(string path)
    {
        _currentFileName = path;
        _player.Open(new Uri($"Resources/Sounds/{path}", UriKind.Relative));
    }

    private void PlayerOnMediaEnded(object sender, EventArgs e)
    {
        _player.Stop();
    }

    private void TestPlayerOnMediaFailed(object sender, ExceptionEventArgs e)
    {
        Log.Error("Error with file {File}. {Error}", _player.Source, e.ErrorException.Message);
        _player.Open(_defaultSoundUri);
    }

    private void PlayerOnMediaFailed(object sender, ExceptionEventArgs e)
    {
        _snackbarService.Show(
            $"Error during notification {_currentFileName} playback.",
            "The sound will be changed to Simple ping.mp3",
            SymbolRegular.Alert24,
            ControlAppearance.Danger
        );

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
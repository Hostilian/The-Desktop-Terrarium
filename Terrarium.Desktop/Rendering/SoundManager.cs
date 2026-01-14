using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Windows.Media;
using System.Windows.Threading;
using Terrarium.Logic.Simulation;

namespace Terrarium.Desktop.Rendering;

/// <summary>
/// Manages sound effects and ambient audio for the terrarium.
/// </summary>
public class SoundManager : IDisposable
{
    private readonly MediaPlayer _backgroundPlayer;
    private readonly Dictionary<string, SoundPlayer> _soundEffects;
    private readonly Random _random;
    private bool _isMuted;
    private double _volume;
    private TerrariumType _currentTheme;
    private readonly DispatcherTimer _themeSongTimer;

    // Constants
    private const double DefaultVolume = 0.5;
    private const int ThemeSongIntervalMinutes = 5;

    public bool IsEnabled { get; set; } = true;
    public double MasterVolume { get => _volume; set => SetVolume(value); }

    public SoundManager()
    {
        _backgroundPlayer = new MediaPlayer();
        _soundEffects = new Dictionary<string, SoundPlayer>();
        _random = new Random();
        _isMuted = false;
        _volume = DefaultVolume;
        _currentTheme = TerrariumType.Forest;

        // Timer for theme song changes
        _themeSongTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(ThemeSongIntervalMinutes)
        };
        _themeSongTimer.Tick += ThemeSongTimer_Tick;

        // Initialize sound effects
        InitializeSoundEffects();

        // Start theme song
        PlayThemeSong();
        _themeSongTimer.Start();
    }

    private void InitializeSoundEffects()
    {
        // Use system sounds for effects since we don't have audio files
        _soundEffects["eat"] = new SoundPlayer(); // Will use beep
        _soundEffects["birth"] = new SoundPlayer();
        _soundEffects["death"] = new SoundPlayer();
        _soundEffects["click"] = new SoundPlayer();
        _soundEffects["ambient"] = new SoundPlayer();
    }

    public void SetTheme(TerrariumType theme)
    {
        _currentTheme = theme;
        PlayThemeSong();
    }

    private void PlayThemeSong()
    {
        if (_isMuted)
        {
            return;
        }

        _backgroundPlayer.Stop();

        // Simulate different theme songs with different frequencies
        // In a real implementation, you'd load actual audio files
        switch (_currentTheme)
        {
            case TerrariumType.Forest:
                // Forest theme - nature sounds
                _backgroundPlayer.Volume = _volume * 0.3;
                break;
            case TerrariumType.Desert:
                // Desert theme - wind sounds
                _backgroundPlayer.Volume = _volume * 0.2;
                break;
            case TerrariumType.Aquatic:
                // Aquatic theme - water sounds
                _backgroundPlayer.Volume = _volume * 0.4;
                break;
            case TerrariumType.GodSimulator:
                // God Simulator theme - mystical/ambient sounds
                _backgroundPlayer.Volume = _volume * 0.5;
                break;
        }

        // For demo, we'll just set volume - real audio would play here
    }

    private void ThemeSongTimer_Tick(object? sender, EventArgs e)
    {
        // Occasionally change to a variation of the theme song
        PlayThemeSong();
    }

    public void PlayEffect(string effectName)
    {
        if (_isMuted || !_soundEffects.ContainsKey(effectName))
        {
            return;
        }

        try
        {
            // Use system beeps for different effects
            switch (effectName)
            {
                case "eat":
                    SystemSounds.Beep.Play();
                    break;
                case "birth":
                    SystemSounds.Asterisk.Play();
                    break;
                case "death":
                    SystemSounds.Hand.Play();
                    break;
                case "click":
                    SystemSounds.Exclamation.Play();
                    break;
                case "ambient":
                    // Random ambient sound
                    if (_random.Next(10) < 3) // 30% chance
                    {
                        SystemSounds.Question.Play();
                    }
                    break;
            }
        }
        catch
        {
            // Silently fail if sound fails
        }
    }

    public void PlayAnimationSound()
    {
        PlayEffect("ambient");
    }

    public void SetMuted(bool muted)
    {
        _isMuted = muted;
        if (muted)
        {
            _backgroundPlayer.Stop();
            _themeSongTimer.Stop();
        }
        else
        {
            PlayThemeSong();
            _themeSongTimer.Start();
        }
    }

    public void SetVolume(double volume)
    {
        _volume = Math.Clamp(volume, 0.0, 1.0);
        _backgroundPlayer.Volume = _volume;
    }

    public void Dispose()
    {
        _themeSongTimer.Stop();
        _backgroundPlayer.Stop();
        foreach (var effect in _soundEffects.Values)
        {
            effect.Dispose();
        }
        _soundEffects.Clear();
    }
}

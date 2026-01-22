namespace Terrarium.Desktop.Rendering;

using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Windows.Media;
using System.Windows.Threading;
using Terrarium.Logic.Simulation;

/// <summary>
/// Manages sound effects and ambient audio for the terrarium.
/// </summary>
public class SoundManager : IDisposable
{
    private readonly MediaPlayer backgroundPlayer;
    private readonly Dictionary<string, SoundPlayer> soundEffects;
    private readonly Random random;
    private bool isMuted;
    private double volume;
    private TerrariumType currentTheme;
    private readonly DispatcherTimer themeSongTimer;

    // Constants
    private const double DefaultVolume = 0.5;
    private const int ThemeSongIntervalMinutes = 5;

    public bool IsEnabled { get; set; } = true;

    public double MasterVolume { get => volume; set => SetVolume(value); }

    public SoundManager()
    {
        backgroundPlayer = new MediaPlayer();
        soundEffects = new Dictionary<string, SoundPlayer>();
        random = new Random();
        isMuted = false;
        volume = DefaultVolume;
        currentTheme = TerrariumType.Forest;

        // Timer for theme song changes
        themeSongTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(ThemeSongIntervalMinutes)
        };
        themeSongTimer.Tick += ThemeSongTimer_Tick;

        // Initialize sound effects
        InitializeSoundEffects();

        // Start theme song
        PlayThemeSong();
        themeSongTimer.Start();
    }

    private void InitializeSoundEffects()
    {
        // Use system sounds for effects since we don't have audio files
        soundEffects["eat"] = new SoundPlayer(); // Will use beep
        soundEffects["birth"] = new SoundPlayer();
        soundEffects["death"] = new SoundPlayer();
        soundEffects["click"] = new SoundPlayer();
        soundEffects["ambient"] = new SoundPlayer();
    }

    public void SetTheme(TerrariumType theme)
    {
        currentTheme = theme;
        PlayThemeSong();
    }

    private void PlayThemeSong()
    {
        if (isMuted)
        {
            return;
        }

        backgroundPlayer.Stop();

        // Simulate different theme songs with different frequencies
        // In a real implementation, you'd load actual audio files
        switch (currentTheme)
        {
            case TerrariumType.Forest:
                // Forest theme - nature sounds
                backgroundPlayer.Volume = volume * 0.3;
                break;
            case TerrariumType.Desert:
                // Desert theme - wind sounds
                backgroundPlayer.Volume = volume * 0.2;
                break;
            case TerrariumType.Aquatic:
                // Aquatic theme - water sounds
                backgroundPlayer.Volume = volume * 0.4;
                break;
            case TerrariumType.GodSimulator:
                // God Simulator theme - mystical/ambient sounds
                backgroundPlayer.Volume = volume * 0.5;
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
        if (isMuted || !soundEffects.ContainsKey(effectName))
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
                    // Random ambient sound - 30% chance
                    if (random.Next(10) < 3)
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
        isMuted = muted;
        if (muted)
        {
            backgroundPlayer.Stop();
            themeSongTimer.Stop();
        }
        else
        {
            PlayThemeSong();
            themeSongTimer.Start();
        }
    }

    public void SetVolume(double volume)
    {
        this.volume = Math.Clamp(volume, 0.0, 1.0);
        backgroundPlayer.Volume = this.volume;
    }

    public void Dispose()
    {
        themeSongTimer.Stop();
        backgroundPlayer.Stop();
        foreach (var effect in soundEffects.Values)
        {
            effect.Dispose();
        }
        soundEffects.Clear();
    }
}

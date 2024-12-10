using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;

namespace Superorganism.Screens
{
    public class OptionsMenuScreen : MenuScreen
    {
        private readonly MenuEntry _backgroundMusicVolumeEntry;
        private readonly MenuEntry _soundEffectVolumeEntry;
        private readonly MenuEntry _fullscreenEntry;
        private readonly MenuEntry _resolutionEntry;

        public GraphicsDeviceManager GraphicsDeviceManager;
        private int _currentResolutionIndex;

        public static float BackgroundMusicVolume { get; private set; }
        public static float SoundEffectVolume { get; private set; } = 0.5f;

        // Available resolutions
        private readonly Point[] _availableResolutions =
        [
            new(800, 480), // default
            new(1280, 720),   // 720p
            new(1920, 1080),  // 1080p
            new(2560, 1440) // 1440p
        ];

        public OptionsMenuScreen() : base("Options")
        {
            _backgroundMusicVolumeEntry = new MenuEntry(string.Empty);
            _soundEffectVolumeEntry = new MenuEntry(string.Empty);
            _fullscreenEntry = new MenuEntry(string.Empty);
            _resolutionEntry = new MenuEntry(string.Empty);
            MenuEntry back = new("Back");

            // Just set up the normal handlers
            _backgroundMusicVolumeEntry.Selected += BackgroundMusicVolumeEntrySelected;
            _soundEffectVolumeEntry.Selected += SoundEffectVolumeEntrySelected;
            _fullscreenEntry.Selected += FullscreenEntrySelected;
            _resolutionEntry.Selected += ResolutionEntrySelected;
            back.Selected += OnCancel;

            // Set up adjust value handlers
            _backgroundMusicVolumeEntry.AdjustValue += BackgroundMusicVolumeEntrySelected;
            _soundEffectVolumeEntry.AdjustValue += SoundEffectVolumeEntrySelected;
            _resolutionEntry.AdjustValue += ResolutionEntrySelected;

            MenuEntries.Add(_backgroundMusicVolumeEntry);
            MenuEntries.Add(_soundEffectVolumeEntry);
            MenuEntries.Add(_fullscreenEntry);
            MenuEntries.Add(_resolutionEntry);
            MenuEntries.Add(back);

            SetMenuEntryText();
        }

        // Override OnSelectEntry to handle Enter/Space as increase for non-back buttons
        protected override void OnSelectEntry(int entryIndex, PlayerIndex playerIndex)
        {
            MenuEntry entry = MenuEntries[entryIndex];

            if (entry.Text.Contains("Back"))
            {
                // For back button, perform normal selection
                base.OnSelectEntry(entryIndex, playerIndex);
            }
            else if (entry.Text.Contains("Fullscreen"))
            {
                // Toggle fullscreen and apply changes
                GraphicsDeviceManager.IsFullScreen = !GraphicsDeviceManager.IsFullScreen;
                GraphicsDeviceManager.HardwareModeSwitch = false;
                GraphicsDeviceManager.ApplyChanges();
                SetMenuEntryText();
            }
            else
            {
                // Treat selection as a positive adjustment and apply changes
                OnAdjustValue(entryIndex, 1, playerIndex);
            }
        }

        public override void Activate()
        {
            base.Activate();

            // Get GraphicsDeviceManager from ScreenManager
            GraphicsDeviceManager = ScreenManager.GraphicsDeviceManager;

            // Now we can safely initialize the current resolution index
            Point currentRes = new(
                GraphicsDeviceManager.PreferredBackBufferWidth,
                GraphicsDeviceManager.PreferredBackBufferHeight
            );

            _currentResolutionIndex = Array.FindIndex(_availableResolutions, r => r.Equals(currentRes));
            if (_currentResolutionIndex == -1) _currentResolutionIndex = 0;

            SetMenuEntryText();
        }

        private void SetMenuEntryText()
        {
            int backgroundMusicVolumePercent = (int)Math.Round(BackgroundMusicVolume * 100);
            int soundEffectVolumePercent = (int)Math.Round(SoundEffectVolume * 100);

            _backgroundMusicVolumeEntry.Text = $"Music Volume: {backgroundMusicVolumePercent}";
            _soundEffectVolumeEntry.Text = $"Sound Effect Volume: {soundEffectVolumePercent}";

            // Only set graphics-related text if GraphicsDeviceManager is available
            if (GraphicsDeviceManager != null)
            {
                _fullscreenEntry.Text = $"Fullscreen: {(GraphicsDeviceManager.IsFullScreen ? "On" : "Off")}";

                if (_currentResolutionIndex >= 0 && _currentResolutionIndex < _availableResolutions.Length)
                {
                    Point res = _availableResolutions[_currentResolutionIndex];
                    _resolutionEntry.Text = $"Resolution: {res.X}x{res.Y}";
                }
            }
        }

        private void FullscreenEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            GraphicsDeviceManager.IsFullScreen = !GraphicsDeviceManager.IsFullScreen;
            GraphicsDeviceManager.HardwareModeSwitch = false;
            GraphicsDeviceManager.ApplyChanges();
            SetMenuEntryText();
        }

        private void ResolutionEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            if (e.Direction != 0)
            {
                _currentResolutionIndex = (_currentResolutionIndex + (e.Direction > 0 ? 1 : -1) + _availableResolutions.Length) % _availableResolutions.Length;

                Point newResolution = _availableResolutions[_currentResolutionIndex];
                GraphicsDeviceManager.PreferredBackBufferWidth = newResolution.X;
                GraphicsDeviceManager.PreferredBackBufferHeight = newResolution.Y;
                GraphicsDeviceManager.ApplyChanges();

                SetMenuEntryText();
            }
        }

        private void BackgroundMusicVolumeEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            BackgroundMusicVolume = e.Direction switch
            {
                > 0 => Math.Min(BackgroundMusicVolume + 0.05f, 1.0f),
                < 0 => Math.Max(BackgroundMusicVolume - 0.05f, 0f),
                _ => BackgroundMusicVolume
            };
            MediaPlayer.Volume = BackgroundMusicVolume;
            SetMenuEntryText();
        }

        private void SoundEffectVolumeEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            SoundEffectVolume = e.Direction switch
            {
                > 0 => Math.Min(SoundEffectVolume + 0.05f, 1.0f),
                < 0 => Math.Max(SoundEffectVolume - 0.05f, 0f),
                _ => SoundEffectVolume
            };
            SoundEffect.MasterVolume = SoundEffectVolume;
            SetMenuEntryText();
        }
    }
}
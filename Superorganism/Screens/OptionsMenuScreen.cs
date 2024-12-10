

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;

namespace Superorganism.Screens
{
    // The options screen is brought up over the top of the main menu
    // screen, and gives the user a chance to configure the game
    // in various hopefully useful ways.
    public class OptionsMenuScreen : MenuScreen
    {
        private readonly MenuEntry _backgroundMusicVolumeEntry;
        private readonly MenuEntry _soundEffectVolumeEntry;

        public static float BackgroundMusicVolume { get; private set; } = 0.00f;

        public static float SoundEffectVolume { get; private set; } = 0.5f;

        public OptionsMenuScreen() : base("Options")
        {
            _backgroundMusicVolumeEntry = new MenuEntry(string.Empty);
            _soundEffectVolumeEntry = new MenuEntry(string.Empty);

            SetMenuEntryText();

            MenuEntry back = new("Back");

            _backgroundMusicVolumeEntry.Selected += BackgroundMusicVolumeEntrySelected;
            _soundEffectVolumeEntry.Selected += SoundEffectVolumeEntrySelected;
            _backgroundMusicVolumeEntry.AdjustValue += BackgroundMusicVolumeEntrySelected;
            _soundEffectVolumeEntry.AdjustValue += SoundEffectVolumeEntrySelected;
            back.Selected += OnCancel;

            MenuEntries.Add(_backgroundMusicVolumeEntry);
            MenuEntries.Add(_soundEffectVolumeEntry);
            MenuEntries.Add(back);
        }

        private void SetMenuEntryText()
        {
            int backgroundMusicVolumePercent = (int)Math.Round(BackgroundMusicVolume * 100);
            int soundEffectVolumePercent = (int)Math.Round(SoundEffectVolume * 100);

            _backgroundMusicVolumeEntry.Text = $"Music Volume: {backgroundMusicVolumePercent}";
            _soundEffectVolumeEntry.Text = $"Sound Effect Volume: {soundEffectVolumePercent}";
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

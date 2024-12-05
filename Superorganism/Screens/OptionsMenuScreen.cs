

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
        private enum Ungulate
        {
            BactrianCamel,
            Dromedary,
            Llama,
        }

        private MenuEntry _backgroundMusicVolumeEntry;
        private MenuEntry _soundEffectVolumeEntry;

        public static float BackgroundMusicVolume { get; private set; } = 0.05f;

        public static float SoundEffectVolume { get; private set; } = 0.5f;

        public OptionsMenuScreen() : base("Options")
        {
            _backgroundMusicVolumeEntry = new MenuEntry(string.Empty);
            _soundEffectVolumeEntry = new MenuEntry(string.Empty);

            SetMenuEntryText();

            MenuEntry back = new MenuEntry("Back");

            _backgroundMusicVolumeEntry.Selected += BackgroundMusicVolumeEntrySelected;
            _soundEffectVolumeEntry.Selected += SoundEffectVolumeEntrySelected;
            _backgroundMusicVolumeEntry.AdjustValue += BackgroundMusicVolumeEntrySelected;
            _soundEffectVolumeEntry.AdjustValue += SoundEffectVolumeEntrySelected;
            back.Selected += OnCancel;

            MenuEntries.Add(_backgroundMusicVolumeEntry);
            MenuEntries.Add(_soundEffectVolumeEntry);
            MenuEntries.Add(back);
        }

        // Fills in the latest values for the options screen menu text.
        private void SetMenuEntryText()
        {
            _backgroundMusicVolumeEntry.Text = $"Background Music Volume: {BackgroundMusicVolume:F2}";
            _soundEffectVolumeEntry.Text = $"Sound Effect Volume: {SoundEffectVolume:F2}";
        }

        private void BackgroundMusicVolumeEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            if (e.Direction > 0)
                BackgroundMusicVolume = Math.Min(BackgroundMusicVolume + 0.05f, 1.0f);
            else if (e.Direction < 0)
                BackgroundMusicVolume = Math.Max(BackgroundMusicVolume - 0.05f, 0f);

            MediaPlayer.Volume = BackgroundMusicVolume;
            SetMenuEntryText();
        }

        private void SoundEffectVolumeEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            if (e.Direction > 0)
                SoundEffectVolume = Math.Min(SoundEffectVolume + 0.05f, 1.0f);
            else if (e.Direction < 0)
                SoundEffectVolume = Math.Max(SoundEffectVolume - 0.05f, 0f);

            SoundEffect.MasterVolume = SoundEffectVolume;
            SetMenuEntryText();
        }

    }
}

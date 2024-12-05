

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

        private readonly MenuEntry _ungulateMenuEntry;
        private readonly MenuEntry _languageMenuEntry;
        private readonly MenuEntry _frobnicateMenuEntry;
        private readonly MenuEntry _elfMenuEntry;
        private MenuEntry _backgroundMusicVolumeEntry;
        private MenuEntry _soundEffectVolumeEntry;

        private static Ungulate _currentUngulate = Ungulate.Dromedary;
        private static readonly string[] Languages = { "C#", "French", "Deoxyribonucleic acid" };
        private static int _currentLanguage;
        private static bool _frobnicate = true;
        private static int _elf = 23;


        public static float BackgroundMusicVolume { get; private set; } = 0.05f;

        public static float SoundEffectVolume { get; private set; } = 0.5f;

        public OptionsMenuScreen() : base("Options")
        {
            _ungulateMenuEntry = new MenuEntry(string.Empty);
            _languageMenuEntry = new MenuEntry(string.Empty);
            _frobnicateMenuEntry = new MenuEntry(string.Empty);
            _elfMenuEntry = new MenuEntry(string.Empty);
            _backgroundMusicVolumeEntry = new MenuEntry(string.Empty);
            _soundEffectVolumeEntry = new MenuEntry(string.Empty);

            SetMenuEntryText();

            MenuEntry back = new MenuEntry("Back");

            _ungulateMenuEntry.Selected += UngulateMenuEntrySelected;
            _languageMenuEntry.Selected += LanguageMenuEntrySelected;
            _frobnicateMenuEntry.Selected += FrobnicateMenuEntrySelected;
            _elfMenuEntry.Selected += ElfMenuEntrySelected;
            _backgroundMusicVolumeEntry.Selected += BackgroundMusicVolumeEntrySelected;
            _soundEffectVolumeEntry.Selected += SoundEffectVolumeEntrySelected;
            _backgroundMusicVolumeEntry.AdjustValue += BackgroundMusicVolumeEntrySelected;
            _soundEffectVolumeEntry.AdjustValue += SoundEffectVolumeEntrySelected;
            back.Selected += OnCancel;

            MenuEntries.Add(_ungulateMenuEntry);
            MenuEntries.Add(_languageMenuEntry);
            MenuEntries.Add(_frobnicateMenuEntry);
            MenuEntries.Add(_elfMenuEntry);
            MenuEntries.Add(_backgroundMusicVolumeEntry);
            MenuEntries.Add(_soundEffectVolumeEntry);
            MenuEntries.Add(back);
        }

        // Fills in the latest values for the options screen menu text.
        private void SetMenuEntryText()
        {
            _ungulateMenuEntry.Text = $"Preferred ungulate: {_currentUngulate}";
            _languageMenuEntry.Text = $"Language: {Languages[_currentLanguage]}";
            _frobnicateMenuEntry.Text = $"Frobnicate: {(_frobnicate ? "on" : "off")}";
            _elfMenuEntry.Text = $"elf: {_elf.ToString()}";
            _backgroundMusicVolumeEntry.Text = $"Background Music Volume: {BackgroundMusicVolume:F2}";
            _soundEffectVolumeEntry.Text = $"Sound Effect Volume: {SoundEffectVolume:F2}";
        }

        private void UngulateMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            _currentUngulate++;

            if (_currentUngulate > Ungulate.Llama)
                _currentUngulate = 0;

            SetMenuEntryText();
        }

        private void LanguageMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            _currentLanguage = (_currentLanguage + 1) % Languages.Length;
            SetMenuEntryText();
        }

        private void FrobnicateMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            _frobnicate = !_frobnicate;
            SetMenuEntryText();
        }

        private void ElfMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            _elf++;
            SetMenuEntryText();
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

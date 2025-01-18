using Microsoft.Xna.Framework;
using Superorganism.Graphics;
using System;
using System.IO;
using System.Linq;
using Superorganism.ScreenManagement;

namespace Superorganism.Screens
{
    public class MainMenuScreen : MenuScreen
    {
        public MainMenuScreen() : base("Superorganism")
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.0);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            string savePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Superorganism",
                "Saves");

            bool hasSaveFiles = Directory.Exists(savePath) &&
                Directory.GetFiles(savePath, "*.sav").Any();

            // Add Continue option if save files exist
            if (hasSaveFiles)
            {
                MenuEntry continueGameMenuEntry = new("Continue");
                continueGameMenuEntry.Selected += ContinueGameMenuEntrySelected;
                MenuEntries.Add(continueGameMenuEntry);
            }

            // Changed "Play Game" to "New Game"
            MenuEntry newGameMenuEntry = new("New Game");
            MenuEntry loadMenuEntry = new("Load Game");
            MenuEntry instructionsMenuEntry = new("Instructions");
            MenuEntry optionsMenuEntry = new("Options");
            MenuEntry exitMenuEntry = new("Exit");

            newGameMenuEntry.Selected += PlayGameMenuEntrySelected;
            loadMenuEntry.Selected += LoadMenuEntrySelected;
            instructionsMenuEntry.Selected += InstructionsMenuEntrySelected;
            optionsMenuEntry.Selected += OptionsMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            MenuEntries.Add(newGameMenuEntry);
            MenuEntries.Add(loadMenuEntry);
            MenuEntries.Add(instructionsMenuEntry);
            MenuEntries.Add(optionsMenuEntry);
            MenuEntries.Add(exitMenuEntry);
        }

        private void ContinueGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            string savePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Superorganism",
                "Saves");

            string mostRecentSave = Directory
                .GetFiles(savePath, "*.sav")
                .OrderByDescending(f => File.GetLastWriteTime(f))
                .FirstOrDefault();

            if (mostRecentSave != null)
            {
                GameplayScreen newGameplayScreen = new()
                {
                    SaveFileToLoad = Path.GetFileName(mostRecentSave)
                };
                LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, newGameplayScreen);
            }
        }

        private void PlayGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, new GameplayScreen(), null);
        }

        private void LoadMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new SaveFileMenuScreen(true), e.PlayerIndex);
        }

        private void InstructionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new InstructionsScreen(), e.PlayerIndex);
        }

        private void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new OptionsMenuScreen(), e.PlayerIndex);
        }

        protected override void OnCancel(PlayerIndex playerIndex)
        {
            const string message = "Are you sure you want to exit the game?";
            MessageBoxScreen confirmExitMessageBox = new(message);
            confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;
            ScreenManager.AddScreen(confirmExitMessageBox, playerIndex);
        }

        private void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.Game.Exit();
        }
    }
}
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Linq;
using Superorganism.ScreenManagement;

namespace Superorganism.Screens
{
    public class MainMenuScreen : MenuScreen
    {
        // Flag to track if we have a child screen open
        private bool _hasOpenChildScreen;

        /// <summary>
        /// 
        /// </summary>
        public MainMenuScreen() : base("Superorganism")
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.0);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            string savePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Superorganism",
                "Saves");

            // Add Continue option if save files exist
            if (HasSaveFiles())
            {
                MenuEntry continueGameMenuEntry = new("Continue");
                continueGameMenuEntry.Selected += ContinueGameMenuEntrySelected;
                MenuEntries.Add(continueGameMenuEntry);
            }

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

        /// <summary>
        /// Override the base Update method to prevent auto-transitioning to Active when child screens are open
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // Check if any of our child screens are active
            if (ScreenManager != null)
            {
                _hasOpenChildScreen = HasActiveChildScreen();
            }

            // If we have an open child screen, we should stay hidden regardless of covered state
            if (_hasOpenChildScreen)
            {
                // Call base update but force it to think it's covered
                base.Update(gameTime, otherScreenHasFocus, true);

                // Ensure we stay hidden even if base would transition us off
                if (ScreenState != ScreenState.Hidden)
                {
                    ScreenState = ScreenState.Hidden;
                }

                return;
            }

            // Normal update behavior when no child screens
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// Checks if any child screens are currently active
        /// </summary>
        private bool HasActiveChildScreen()
        {
            GameScreen[] screens = ScreenManager.GetScreens();

            foreach (GameScreen screen in screens)
            {
                // Skip inactive or hidden screens
                if (screen.ScreenState == ScreenState.Hidden)
                    continue;

                // Check for specific child screen types with a reference back to this
                if (screen is OptionsMenuScreen optScreen && optScreen.SourceMainMenu == this)
                    return true;

                if (screen is SaveFileMenuScreen saveScreen && saveScreen.SourceMainMenu == this)
                    return true;

                if (screen is InstructionsScreen instructionsScreen && instructionsScreen.SourceMainMenu == this)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Draw the menu screen
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // Don't draw if we're hidden or have an open child screen
            if (ScreenState == ScreenState.Hidden || _hasOpenChildScreen)
            {
                return;
            }

            // Draw the menu entries
            base.Draw(gameTime);
        }

        public bool HasSaveFiles()
        {
            string savePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Superorganism",
                "Saves");

            return Directory.Exists(savePath) &&
                                Directory.GetFiles(savePath, "*.sav").Any();
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
            // Hide this main menu screen
            ScreenState = ScreenState.Hidden;

            // Create and add the save file menu screen
            SaveFileMenuScreen loadScreen = new(true)
            {
                SourceMainMenu = this
            };
            ScreenManager.AddScreen(loadScreen, e.PlayerIndex);
        }

        private void InstructionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            // Hide this main menu screen
            ScreenState = ScreenState.Hidden;

            // Create and add the instructions screen
            InstructionsScreen instructionsScreen = new()
            {
                SourceMainMenu = this
            };
            ScreenManager.AddScreen(instructionsScreen, e.PlayerIndex);
        }

        private void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            // Hide this main menu screen
            ScreenState = ScreenState.Hidden;

            // Create and add the options menu screen
            OptionsMenuScreen optionsScreen = new()
            {
                SourceMainMenu = this
            };
            ScreenManager.AddScreen(optionsScreen, e.PlayerIndex);
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
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Core.Timing;
using Superorganism.ScreenManagement;

namespace Superorganism.Screens
{
    public class PauseMenuScreen : MenuScreen
    {
        public bool ShouldPauseGame { get; } = true;
        private bool _hasOpenChildScreen;
        private bool _isInitialPauseTransition = true;

        public PauseMenuScreen() : base("Paused")
        {
            // Set this as a popup screen so the gameplay screen remains visible underneath
            IsPopup = true;

            MenuEntry resumeGameMenuEntry = new("Resume Game");
            MenuEntry optionGameMenuEntry = new("Options");
            MenuEntry saveMenuEntry = new("Save Game");
            MenuEntry loadMenuEntry = new("Load Game");
            MenuEntry quitGameMenuEntry = new("Quit Game");
            resumeGameMenuEntry.Selected += OnCancel;
            optionGameMenuEntry.Selected += OptionsMenuEntrySelected;
            saveMenuEntry.Selected += SaveMenuEntrySelected;
            loadMenuEntry.Selected += LoadMenuEntrySelected;
            quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;
            MenuEntries.Add(resumeGameMenuEntry);
            MenuEntries.Add(saveMenuEntry);
            MenuEntries.Add(loadMenuEntry);
            MenuEntries.Add(optionGameMenuEntry);
            MenuEntries.Add(quitGameMenuEntry);
        }

        /// <summary>
        /// Override Update to handle transitions and child screens
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // Check if any child screens are active
            if (ScreenManager != null)
            {
                _hasOpenChildScreen = HasActiveChildScreen();
            }

            // If we have an open child screen, stay hidden
            if (_hasOpenChildScreen)
            {
                base.Update(gameTime, otherScreenHasFocus, true);

                if (ScreenState != ScreenState.Hidden)
                {
                    ScreenState = ScreenState.Hidden;
                }

                // No longer in initial transition when we have children
                _isInitialPauseTransition = false;

                return;
            }

            // Normal update behavior
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Handle resuming the game when transitioning off
            if (ScreenState == ScreenState.TransitionOff && !IsExiting)
            {
                GameTimer.Resume();
            }

            // Reset initial transition flag when we're fully visible
            if (ScreenState == ScreenState.Active)
            {
                _isInitialPauseTransition = false;
            }
        }

        /// <summary>
        /// Checks if any child screens (OptionsMenu, SaveFileMenu) are currently active
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
                if (screen is OptionsMenuScreen optScreen && optScreen.SourcePauseMenu == this)
                    return true;

                if (screen is SaveFileMenuScreen saveScreen && saveScreen.SourcePauseMenu == this)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Draw with fading effect only during initial pause transition
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // Skip drawing if hidden or covered by child screen
            if (ScreenState == ScreenState.Hidden || _hasOpenChildScreen)
            {
                return;
            }

            // Apply fading effect only during initial transition or when exiting
            if (_isInitialPauseTransition || IsExiting)
            {
                // Fade in when first pausing, fade out when exiting
                ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 0.6f);
            }
            else
            {
                // Constant darkness when showing menu
                ScreenManager.FadeBackBufferToBlack(0.6f);
            }

            // Draw menu entries
            base.Draw(gameTime);
        }

        /// <summary>
        /// When going back to gameplay, reset the initial transition flag
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            // Reset for next time we pause
            _isInitialPauseTransition = true;

            // Resume the gameplay timer
            GameTimer.Resume();

            base.OnCancel(playerIndex);
        }

        private void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            // Hide this pause menu screen
            ScreenState = ScreenState.Hidden;

            // Create and add the options menu screen
            OptionsMenuScreen optionsScreen = new()
            {
                SourcePauseMenu = this
            };
            ScreenManager.AddScreen(optionsScreen, e.PlayerIndex);
        }

        private void SaveMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            // Hide this pause menu screen
            ScreenState = ScreenState.Hidden;

            // Create and add the save file menu screen
            SaveFileMenuScreen saveScreen = new(false)
            {
                SourcePauseMenu = this
            };
            ScreenManager.AddScreen(saveScreen, e.PlayerIndex);
        }

        private void LoadMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            // Hide this pause menu screen
            ScreenState = ScreenState.Hidden;

            // Create and add the save file menu screen for loading
            SaveFileMenuScreen loadScreen = new(true)
            {
                SourcePauseMenu = this
            };
            ScreenManager.AddScreen(loadScreen, e.PlayerIndex);
        }

        private void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            const string message = "Are you sure you want to quit this game?\nUnsaved progress will be lost.";
            MessageBoxScreen confirmQuitMessageBox = new(message);
            confirmQuitMessageBox.Accepted += ConfirmQuitMessageBoxAccepted;
            ScreenManager.AddScreen(confirmQuitMessageBox, ControllingPlayer);
        }

        private void ConfirmQuitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            // Reset the timer when quitting the game
            GameTimer.Reset();
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(), new MainMenuScreen());
        }
    }
}
using System.Linq;
using Microsoft.Xna.Framework;
using Superorganism.Core.Timing;

namespace Superorganism.Screens
{
    public class PauseMenuScreen : MenuScreen
    {
        public PauseMenuScreen() : base("Paused")
        {
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
        /// Overrides OnCancel to resume the gameplay timer when unpausing the game.
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            // Resume the gameplay timer when canceling/resuming from pause
            GameTimer.Resume();
            base.OnCancel(playerIndex);
        }

        /// <summary>
        /// Override Update to handle timer states based on screen transitions.
        /// When the screen is transitioning off, we'll resume the timer.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // If we're in the middle of transitioning off (removing the pause screen),
            // resume the timer
            if (ScreenState == ScreenManagement.ScreenState.TransitionOff && !IsExiting)
            {
                GameTimer.Resume();
            }
        }

        private void SaveMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new SaveFileMenuScreen(false), e.PlayerIndex);
        }

        private void LoadMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new SaveFileMenuScreen(true), e.PlayerIndex);
        }

        private void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new OptionsMenuScreen(), e.PlayerIndex);
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
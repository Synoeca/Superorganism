using Microsoft.Xna.Framework;

namespace Superorganism.Screens
{
    // The main menu screen is the first thing displayed when the game starts up.
    public class MainMenuScreen : MenuScreen
    {
        public MainMenuScreen() : base("")
        {
            MenuEntry playGameMenuEntry = new("Play Game");
            MenuEntry loadMenuEntry = new("Load Game");
            MenuEntry instructionsMenuEntry = new("Instructions");
            MenuEntry optionsMenuEntry = new("Options");
            MenuEntry exitMenuEntry = new("Exit");

            playGameMenuEntry.Selected += PlayGameMenuEntrySelected;
            loadMenuEntry.Selected += LoadMenuEntrySelected;
            instructionsMenuEntry.Selected += InstructionsMenuEntrySelected;
            optionsMenuEntry.Selected += OptionsMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            MenuEntries.Add(playGameMenuEntry);
            MenuEntries.Add(loadMenuEntry);
            MenuEntries.Add(instructionsMenuEntry);
            MenuEntries.Add(optionsMenuEntry);
            MenuEntries.Add(exitMenuEntry);
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

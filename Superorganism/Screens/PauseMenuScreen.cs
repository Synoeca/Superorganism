using Superorganism.Core.SaveLoadSystem;
using System.Linq;

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

        private void SaveMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new SaveFileMenuScreen(false), e.PlayerIndex);
        }

        private void LoadMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new SaveFileMenuScreen(true), e.PlayerIndex);
        }

        private void ConfirmLoadMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            // Get the current GameplayScreen
            GameplayScreen gameplayScreen = GetGameplayScreen();
            if (gameplayScreen == null) return;

            // Create a new GameplayScreen (this will load the latest save)
            GameplayScreen newGameplayScreen = new();

            // Load the new screen
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, newGameplayScreen);
        }

        private GameplayScreen GetGameplayScreen()
        {
            return ScreenManager.GetScreens().OfType<GameplayScreen>().FirstOrDefault();
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
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(), new MainMenuScreen());
        }
    }
}
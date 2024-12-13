using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using ContentPipeline;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static System.TimeZoneInfo;
using Superorganism.Core.SaveLoadSystem;
using Superorganism.ScreenManagement;

namespace Superorganism.Screens
{
    public class SaveFileMenuScreen : GameScreen
    {
        private const int EntriesPerPage = 6;
        private readonly bool _isLoadingMode;
        private readonly string _savePath;
        private readonly List<List<SaveFileEntry>> _pages = [];
        private SaveFileEntry _backButton;
        private int _currentPage;
        private Vector2 _pageIndicatorPosition;
        private readonly JsonSerializerOptions _serializerOptions;

        private readonly InputAction _menuLeft;
        private readonly InputAction _menuRight;
        private readonly InputAction _menuUp;
        private readonly InputAction _menuDown;
        private readonly InputAction _menuSelect;
        private readonly InputAction _menuCancel;

        private int _selectedEntry;

        public SaveFileMenuScreen(bool isLoadingMode)
        {
            _isLoadingMode = isLoadingMode;
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            _serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new Vector2Converter() }
            };

            _savePath = Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Content", "Saves"));

            // Initialize input actions
            _menuUp = new InputAction(
                [Buttons.DPadUp, Buttons.LeftThumbstickUp],
                [Keys.Up], true);
            _menuDown = new InputAction(
                [Buttons.DPadDown, Buttons.LeftThumbstickDown],
                [Keys.Down], true);
            _menuLeft = new InputAction(
                [Buttons.DPadLeft, Buttons.LeftThumbstickLeft],
                [Keys.Left], true);
            _menuRight = new InputAction(
                [Buttons.DPadRight, Buttons.LeftThumbstickRight],
                [Keys.Right], true);
            _menuSelect = new InputAction(
                [Buttons.A, Buttons.Start],
                [Keys.Enter, Keys.Space], true);
            _menuCancel = new InputAction(
                [Buttons.B, Buttons.Back],
                [Keys.Escape], true);

            PopulateEntries();
        }

        private void PopulateEntries()
        {
            _pages.Clear();
            List<SaveFileEntry> currentPage = [];

            // Add back button
            _backButton = new SaveFileEntry("Back", "", true);

            if (!Directory.Exists(_savePath))
            {
                _pages.Add([new SaveFileEntry("No save files found", "", false)]);
                return;
            }

            List<string> saveFiles = Directory
                .GetFiles(_savePath, "Save*.sav")
                .OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f).Replace("Save", "")))
                .ToList();

            foreach (string file in saveFiles)
            {
                string saveFileName = Path.GetFileName(file);
                try
                {
                    string jsonContent = File.ReadAllText(file);
                    GameStateContent saveState = JsonSerializer.Deserialize<GameStateContent>(jsonContent, _serializerOptions);

                    string entryText = $"{saveFileName} - HP: {saveState.PlayerHealth}, Crops: {saveState.CropsLeft}";
                    currentPage.Add(new SaveFileEntry(entryText, saveFileName, true));
                }
                catch
                {
                    currentPage.Add(new SaveFileEntry($"{saveFileName} (Corrupted)", saveFileName, false));
                }

                if (currentPage.Count == EntriesPerPage)
                {
                    _pages.Add(currentPage);
                    currentPage = [];
                }
            }

            // Add any remaining entries
            if (currentPage.Count > 0)
            {
                _pages.Add(currentPage);
            }

            // If in save mode, add "New Save" to the last page
            if (!_isLoadingMode)
            {
                if (_pages.Count == 0 || _pages[^1].Count == EntriesPerPage)
                {
                    _pages.Add([]);
                }
                _pages[^1].Add(new SaveFileEntry("Create New Save", "", true));
            }

            // If no pages were created, add an empty page
            if (_pages.Count == 0)
            {
                _pages.Add([new SaveFileEntry("No save files found", "", false)]);
            }
        }

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            // Handle page navigation
            if (_menuLeft.Occurred(input, ControllingPlayer, out _))
            {
                if (_pages.Count > 1)
                {
                    _currentPage = (_currentPage - 1 + _pages.Count) % _pages.Count;
                    _selectedEntry = 0; // Reset selection when changing pages
                }
            }

            if (_menuRight.Occurred(input, ControllingPlayer, out _))
            {
                if (_pages.Count > 1)
                {
                    _currentPage = (_currentPage + 1) % _pages.Count;
                    _selectedEntry = 0; // Reset selection when changing pages
                }
            }

            // Handle entry navigation
            if (_menuUp.Occurred(input, ControllingPlayer, out _))
            {
                _selectedEntry--;
                if (_selectedEntry < 0)
                {
                    _selectedEntry = _pages[_currentPage].Count - 1;
                }
            }

            if (_menuDown.Occurred(input, ControllingPlayer, out _))
            {
                _selectedEntry++;
                if (_selectedEntry >= _pages[_currentPage].Count)
                {
                    _selectedEntry = 0;
                }
            }

            if (_menuSelect.Occurred(input, ControllingPlayer, out PlayerIndex playerIndex))
            {
                HandleSelection(playerIndex);
            }

            if (_menuCancel.Occurred(input, ControllingPlayer, out _))
            {
                ExitScreen();
            }
        }

        private void HandleSelection(PlayerIndex playerIndex)
        {
            List<SaveFileEntry> currentEntries = _pages[_currentPage];
            if (_selectedEntry < 0 || _selectedEntry >= currentEntries.Count)
                return;

            SaveFileEntry selectedEntry = currentEntries[_selectedEntry];

            if (!selectedEntry.IsValid)
                return;

            if (selectedEntry.Text == "Back")
            {
                ExitScreen();
                return;
            }

            if (_isLoadingMode)
            {
                if (selectedEntry.FileName != "")
                {
                    const string message = "Are you sure you want to load this save?\nUnsaved progress will be lost.";
                    MessageBoxScreen confirmBox = new(message);
                    confirmBox.Accepted += (s, e) => LoadSaveFile(selectedEntry.FileName, e.PlayerIndex);
                    ScreenManager.AddScreen(confirmBox, ControllingPlayer);
                }
            }
            else
            {
                if (selectedEntry.Text == "Create New Save")
                {
                    CreateNewSave(playerIndex);
                }
                else
                {
                    SaveToFile(selectedEntry.FileName, playerIndex);
                }
            }
        }

        private void LoadSaveFile(string fileName, PlayerIndex playerIndex)
        {
            GameplayScreen newGameplayScreen = new() { SaveFileToLoad = fileName };
            LoadingScreen.Load(ScreenManager, true, playerIndex, newGameplayScreen);
        }

        private void SaveToFile(string fileName, PlayerIndex playerIndex)
        {
            GameplayScreen gameplayScreen = GetGameplayScreen();
            if (gameplayScreen?.GameStateManager == null) return;

            GameStateSaver.SaveGameState(
                gameplayScreen.GameStateManager,
                ScreenManager.Game.Content,
                fileName
            );

            const string message = "Game saved successfully!";
            MessageBoxScreen confirmationBox = new(message);
            confirmationBox.Accepted += (s, e) => PopulateEntries();
            ScreenManager.AddScreen(confirmationBox, ControllingPlayer);
        }

        private void CreateNewSave(PlayerIndex playerIndex)
        {
            GameplayScreen gameplayScreen = GetGameplayScreen();
            if (gameplayScreen?.GameStateManager == null) return;

            GameStateSaver.SaveGameState(
                gameplayScreen.GameStateManager,
                ScreenManager.Game.Content,
                null
            );

            const string message = "Game saved successfully!";
            MessageBoxScreen confirmationBox = new(message);
            confirmationBox.Accepted += (s, e) => PopulateEntries();
            ScreenManager.AddScreen(confirmationBox, ControllingPlayer);
        }

        private GameplayScreen GetGameplayScreen()
        {
            return ScreenManager.GetScreens().OfType<GameplayScreen>().FirstOrDefault();
        }

        private int GetSelectedIndex()
        {
            return _selectedEntry;
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            UpdateEntryLocations();

            spriteBatch.Begin();

            string title = _isLoadingMode ? "Load Game" : "Save Game";
            DrawTitle(title, new Vector2(viewport.Width / 2f, viewport.Height * 0.1f));

            // Draw entries with selection highlight
            for (int i = 0; i < _pages[_currentPage].Count; i++)
            {
                SaveFileEntry entry = _pages[_currentPage][i];
                bool isSelected = i == _selectedEntry;
                entry.Draw(this, isSelected);
            }

            // Draw page indicator if there are multiple pages
            if (_pages.Count > 1)
            {
                DrawPageIndicator();
            }

            spriteBatch.End();
        }

        private void UpdateEntryLocations()
        {
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            float centerY = viewport.Height / 2f;
            float totalHeight = _pages[_currentPage].Count * (ScreenManager.Font.LineSpacing * 0.8f);
            float y = centerY - totalHeight / 2f;

            foreach (SaveFileEntry entry in _pages[_currentPage])
            {
                float x = viewport.Width / 2f - entry.GetWidth(ScreenManager) / 2f;
                entry.Position = new Vector2(x, y);
                y += entry.GetHeight(ScreenManager) + 10;
            }

            // Position back button at bottom
            float backX = viewport.Width / 2f - _backButton.GetWidth(ScreenManager) / 2f;
            _backButton.Position = new Vector2(backX, y + 20);

            // Position page indicator
            _pageIndicatorPosition = new Vector2(viewport.Width * 0.8f, y + 20);
        }

        private void DrawTitle(string title, Vector2 position)
        {
            SpriteFont font = ScreenManager.Font;
            Vector2 origin = font.MeasureString(title) / 2;
            const float scale = 1.5f;
            const float shadowOffset = 4f;

            ScreenManager.SpriteBatch.DrawString(font, title,
                position + new Vector2(shadowOffset),
                Color.Black * TransitionAlpha,
                0, origin, scale, SpriteEffects.None, 0);

            ScreenManager.SpriteBatch.DrawString(font, title,
                position,
                new Color(220, 220, 220) * TransitionAlpha,
                0, origin, scale, SpriteEffects.None, 0);
        }

        private void DrawPageIndicator()
        {
            string text = $"Page {_currentPage + 1}/{_pages.Count}";
            SpriteFont font = ScreenManager.Font;

            ScreenManager.SpriteBatch.DrawString(font, text,
                _pageIndicatorPosition + new Vector2(2),
                Color.Black * TransitionAlpha,
                0, Vector2.Zero, 0.8f, SpriteEffects.None, 0);

            ScreenManager.SpriteBatch.DrawString(font, text,
                _pageIndicatorPosition,
                Color.White * TransitionAlpha,
                0, Vector2.Zero, 0.8f, SpriteEffects.None, 0);
        }
    }
}
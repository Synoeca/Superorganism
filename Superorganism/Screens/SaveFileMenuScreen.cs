using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Superorganism.AI;
using Superorganism.Core.Managers;
using Superorganism.Core.SaveLoadSystem;
using Superorganism.ScreenManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Superorganism.Graphics;

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
        private PixelTextRenderer _titleRenderer;
        private string _menuTitle = "";

        private readonly InputAction _menuLeft;
        private readonly InputAction _menuRight;
        private readonly InputAction _menuUp;
        private readonly InputAction _menuDown;
        private readonly InputAction _menuSelect;
        private readonly InputAction _menuCancel;

        private bool _isNaming;
        private string _currentInput = "";
        private string _defaultSaveName;
        private readonly InputAction _menuDelete;

        private KeyboardState _previousKeyboardState;
        private HashSet<Keys> _pressedKeys = [];

        private int _selectedEntry;

        public SaveFileMenuScreen(bool isLoadingMode)
        {
            _isLoadingMode = isLoadingMode;
            _menuTitle = _isLoadingMode ? "LOAD GAME" : "SAVE GAME";

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            _serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new Vector2Converter() }
            };

            _savePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Superorganism",
                "Saves"
            );

            _menuDelete = new InputAction(
                [],  // No gamepad button for delete
                [Keys.Delete],
                true);

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

        public override void Activate()
        {
            base.Activate();

            // Initialize the title renderer if it doesn't exist
            if (_titleRenderer == null && !string.IsNullOrEmpty(_menuTitle))
            {
                _titleRenderer = new PixelTextRenderer(
                    ScreenManager.Game,
                    _menuTitle
                );
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Update the title renderer
            if (_titleRenderer != null && !string.IsNullOrEmpty(_menuTitle))
            {
                _titleRenderer.Update(gameTime, TransitionPosition, ScreenState);
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                ExitScreen();
            }
        }

        private void PopulateEntries()
        {
            _pages.Clear();
            List<SaveFileEntry> currentPage = [];

            // Add back button
            _backButton = new SaveFileEntry("Back", "");

            // Create save directory if it doesn't exist
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
                if (_isLoadingMode)
                {
                    _pages.Add([new SaveFileEntry("No save files found", "", false)]);
                }
                else
                {
                    _pages.Add([new SaveFileEntry("Create New Save", "")]);
                    _selectedEntry = 0; // Focus on Create New Save
                }
                return;
            }

            // If in save mode, add "Create New Save" at the beginning of the first page 
            if (!_isLoadingMode)
            {
                currentPage.Add(new SaveFileEntry("Create New Save", ""));
                _selectedEntry = 0; // Focus on Create New Save
            }

            // Get all .sav files in the directory, regardless of prefix
            List<string> saveFiles = Directory
                .GetFiles(_savePath, "*.sav") // Use *.sav to match any save file
                .OrderByDescending(f => File.GetLastWriteTime(f))
                .ToList();

            foreach (string file in saveFiles)
            {
                string saveFileName = Path.GetFileName(file);
                try
                {
                    string jsonContent = File.ReadAllText(file);
                    GameStateContent saveState = JsonSerializer.Deserialize<GameStateContent>(jsonContent, _serializerOptions);

                    // Find the player (Ant) entity
                    EntityData playerEntity = saveState.Entities.FirstOrDefault(e => e.Type == "Ant");
                    // Count remaining crops
                    int cropsCount = saveState.Entities.Count(e => e.Type == "Crop");

                    string entryText = $"{saveFileName} - HP: {playerEntity?.Health ?? 0}, Crops: {cropsCount}";
                    currentPage.Add(new SaveFileEntry(entryText, saveFileName));
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

            // If no pages were created, add an empty page
            if (_pages.Count == 0)
            {
                if (_isLoadingMode)
                {
                    _pages.Add([new SaveFileEntry("No save files found", "", false)]);
                }
                else
                {
                    _pages.Add([new SaveFileEntry("Create New Save", "")]);
                    _selectedEntry = 0; // Focus on Create New Save
                }
            }
        }


        private void HandleNamingInput(InputState input)
        {
            // Get current keyboard state
            KeyboardState currentKeyboard = input.CurrentKeyboardStates[0];

            // Handle Enter/Space to confirm
            if (_menuSelect.Occurred(input, ControllingPlayer, out _))
            {
                // If the current input is empty, fall back to the default name
                string fileName = string.IsNullOrWhiteSpace(_currentInput)
                    ? _defaultSaveName
                    : _currentInput;  // We no longer append ".sav" here, as the user provides the full name
                SaveToFile(fileName);
                _isNaming = false;
                _currentInput = "";
                _pressedKeys.Clear();
                return;
            }

            // Handle Escape to cancel
            if (_menuCancel.Occurred(input, ControllingPlayer, out _))
            {
                _isNaming = false;
                _currentInput = "";
                _pressedKeys.Clear();
                return;
            }

            // Check for modifier keys
            bool shiftPressed = currentKeyboard.IsKeyDown(Keys.LeftShift) || currentKeyboard.IsKeyDown(Keys.RightShift);
            bool capsLockActive = Console.CapsLock; // Or use input-dependent CapsLock check
            bool numLockActive = currentKeyboard.IsKeyDown(Keys.NumLock);

            // Track currently pressed keys
            foreach (Keys key in currentKeyboard.GetPressedKeys())
            {
                if (_previousKeyboardState.IsKeyUp(key))
                {
                    _pressedKeys.Add(key);
                }
            }

            // Handle key releases
            foreach (Keys key in _pressedKeys.ToList())
            {
                if (currentKeyboard.IsKeyUp(key))
                {
                    if (key == Keys.Back && _currentInput.Length > 0)
                    {
                        _currentInput = _currentInput[..^1]; // Remove last character
                    }
                    else if (_currentInput.Length < 20) // Limit name length
                    {
                        char? c = ConvertKeyToChar(key, shiftPressed, capsLockActive, numLockActive);
                        if (c.HasValue && (char.IsLetterOrDigit(c.Value) || c == '_' || c == '-'))
                        {
                            _currentInput += c.Value;
                        }
                    }
                    _pressedKeys.Remove(key);
                }
            }

            _previousKeyboardState = currentKeyboard;
        }



        public static char? ConvertKeyToChar(Keys key, bool shiftPressed, bool capsLockActive, bool numLockActive)
        {
            // Handle alphabetic keys
            if (key >= Keys.A && key <= Keys.Z)
            {
                char baseChar = (char)(key - Keys.A + 'A');
                // Adjust for Shift and CapsLock
                return (shiftPressed ^ capsLockActive) ? baseChar : char.ToLower(baseChar);
            }

            // Handle digits (top row)
            if (key >= Keys.D0 && key <= Keys.D9)
            {
                char[] shiftChars = [')', '!', '@', '#', '$', '%', '^', '&', '*', '('];
                int index = key - Keys.D0;
                return shiftPressed ? shiftChars[index] : (char)('0' + index);
            }

            // Handle NumPad keys
            if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
            {
                // Only output numbers if NumLock is active
                return numLockActive ? (char)('0' + (key - Keys.NumPad0)) : null;
            }

            // Handle special keys with shift
            switch (key)
            {
                case Keys.OemPlus: return shiftPressed ? '+' : '=';
                case Keys.OemComma: return shiftPressed ? '<' : ',';
                case Keys.OemMinus: return shiftPressed ? '_' : '-';
                case Keys.OemPeriod: return shiftPressed ? '>' : '.';
                case Keys.OemQuestion: return shiftPressed ? '?' : '/';
                case Keys.OemTilde: return shiftPressed ? '~' : '`';
                case Keys.OemOpenBrackets: return shiftPressed ? '{' : '[';
                case Keys.OemCloseBrackets: return shiftPressed ? '}' : ']';
                case Keys.OemPipe: return shiftPressed ? '|' : '\\';
                case Keys.OemSemicolon: return shiftPressed ? ':' : ';';
                case Keys.OemQuotes: return shiftPressed ? '"' : '\'';
            }

            // Return null for unmapped keys
            return null;
        }




        private void HandleDeleteOption()
        {
            List<SaveFileEntry> currentEntries = _pages[_currentPage];
            if (_selectedEntry < 0 || _selectedEntry >= currentEntries.Count)
                return;

            SaveFileEntry selectedEntry = currentEntries[_selectedEntry];
            if (!selectedEntry.IsValid || selectedEntry.Text == "Create New Save" ||
                selectedEntry.FileName == "")
                return;

            const string message = "Are you sure you want to delete this save file?";
            MessageBoxScreen confirmBox = new(message);
            confirmBox.Accepted += (_, _) => DeleteSaveFile(selectedEntry.FileName);
            ScreenManager.AddScreen(confirmBox, ControllingPlayer);
        }

        private void DeleteSaveFile(string fileName)
        {
            try
            {
                string filePath = Path.Combine(_savePath, fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    PopulateEntries();
                }
            }
            catch (Exception)
            {
                const string message = "Failed to delete save file.";
                MessageBoxScreen errorBox = new(message);
                ScreenManager.AddScreen(errorBox, ControllingPlayer);
            }
        }

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (_isNaming)
            {
                HandleNamingInput(input);
                return;
            }

            // Handle delete key
            if (_menuDelete.Occurred(input, ControllingPlayer, out _))
            {
                HandleDeleteOption();
                return;
            }

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

            if (_menuSelect.Occurred(input, ControllingPlayer, out PlayerIndex _))
            {
                HandleSelection();
            }

            if (_menuCancel.Occurred(input, ControllingPlayer, out _))
            {
                ExitScreen();
            }
        }

        private void HandleSelection()
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
                    confirmBox.Accepted += (_, e) => LoadSaveFile(selectedEntry.FileName, e.PlayerIndex);
                    ScreenManager.AddScreen(confirmBox, ControllingPlayer);
                }
            }
            else
            {
                if (selectedEntry.Text == "Create New Save")
                {
                    CreateNewSave();
                }
                else
                {
                    SaveToFile(selectedEntry.FileName);
                }
            }
        }

        private void LoadSaveFile(string fileName, PlayerIndex playerIndex)
        {
            GameplayScreen newGameplayScreen = new() { SaveFileToLoad = fileName };
            LoadingScreen.Load(ScreenManager, true, playerIndex, newGameplayScreen);
        }

        private void SaveToFile(string fileName)
        {
            GameplayScreen gameplayScreen = GetGameplayScreen();
            if (gameplayScreen?.GameStateManager == null) return;

            GameStateSaver.SaveGameState(
                new GameStateInfo
                {
                    Entities = DecisionMaker.Entities,
                    GameProgressTime = gameplayScreen.GameStateManager.GameTime.TotalGameTime
                },
                fileName
            );

            const string message = "Game saved successfully!";
            MessageBoxScreen confirmationBox = new(message);
            confirmationBox.Accepted += (_, _) => PopulateEntries();
            ScreenManager.AddScreen(confirmationBox, ControllingPlayer);
        }

        private void CreateNewSave()
        {
            GameplayScreen gameplayScreen = GetGameplayScreen();
            if (gameplayScreen?.GameStateManager == null) return;

            // Start naming process
            _isNaming = true;
            _currentInput = "";

            // Generate default save name
            int nextNumber = Directory
                .GetFiles(_savePath, "Save*.sav")
                .Select(f =>
                {
                    string fileName = Path.GetFileNameWithoutExtension(f);
                    string numberPart = fileName.Replace("Save", "");
                    return int.TryParse(numberPart, out int num) ? num : 0;
                })
                .DefaultIfEmpty(0)
                .Max() + 1;
            _defaultSaveName = $"Save{nextNumber}.sav";
        }

        private GameplayScreen GetGameplayScreen()
        {
            return ScreenManager.GetScreens().OfType<GameplayScreen>().FirstOrDefault();
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;


            // Draw the 3D title first if it exists
            if (_titleRenderer != null && !string.IsNullOrEmpty(_menuTitle) && !IsExiting)
            {
                float transitionOffset = (float)Math.Pow(TransitionPosition, 2);
                if (ScreenState == ScreenState.TransitionOn ||
                    ScreenState == ScreenState.Active ||
                    ScreenState == ScreenState.TransitionOff)
                {
                    _titleRenderer.Draw();
                }
            }

            UpdateEntryLocations();

            spriteBatch.Begin();

            if (_isNaming)
            {
                DrawNamingInterface(viewport);
            }
            else
            {
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
            }

            spriteBatch.End();
        }

        private void DrawNamingInterface(Viewport viewport)
        {
            string title = "Enter Save Name";
            Vector2 titlePos = new(viewport.Width / 2f, viewport.Height * 0.3f);
            DrawTitle(title, titlePos);

            string displayText = string.IsNullOrEmpty(_currentInput) ?
                $"Default: {_defaultSaveName}" :
                _currentInput;

            Vector2 textSize = ScreenManager.Font.MeasureString(displayText);
            Vector2 textPos = new(
                viewport.Width / 2f - textSize.X / 2f,
                viewport.Height * 0.5f
            );

            ScreenManager.SpriteBatch.DrawString(
                ScreenManager.Font,
                displayText,
                textPos,
                Color.White * TransitionAlpha
            );

            // Draw instructions
            string instructions = "Press Enter to save, Escape to cancel";
            Vector2 instrSize = ScreenManager.Font.MeasureString(instructions);
            Vector2 instrPos = new(
                viewport.Width / 2f - instrSize.X / 2f,
                viewport.Height * 0.7f
            );

            ScreenManager.SpriteBatch.DrawString(
                ScreenManager.Font,
                instructions,
                instrPos,
                Color.Gray * TransitionAlpha
            );
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
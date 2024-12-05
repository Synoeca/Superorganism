using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Superorganism.Screens
{
    public class InstructionsScreen : MenuScreen
    {
        private readonly MenuEntry _movementEntry;
        private readonly MenuEntry _movementEntry2;
        private readonly MenuEntry _jumpEntry;
        private readonly MenuEntry _dashEntry;
        private readonly MenuEntry _restartEntry;
        private readonly MenuEntry _exitGameEntry;
        private readonly MenuEntry _backEntry;

        public InstructionsScreen() : base("Instructions")
        {
            _movementEntry = new MenuEntry(
                "Movement Controls:    " +
                "A = Move Left"
            );

            _movementEntry2 = new MenuEntry("D = Move Right");

            _jumpEntry = new MenuEntry(
                "Jump:    " +
                "Press SPACE while on ground"
            );

            _dashEntry = new MenuEntry(
                "Dash:    " +
                "Hold SHIFT to increase speed"
            );

            _restartEntry = new MenuEntry(
                "Restart:    " +
                "Press R to restart current level"
            );

            _exitGameEntry = new MenuEntry(
                "Exit Game:    " +
                "ESC > Pause Menu > Select 'Quit Game'"
            );

            _backEntry = new MenuEntry("Back");
            _backEntry.Selected += OnCancel;

            MenuEntries.Add(_movementEntry);
            MenuEntries.Add(_movementEntry2);
            MenuEntries.Add(_jumpEntry);
            MenuEntries.Add(_dashEntry);
            MenuEntries.Add(_restartEntry);
            MenuEntries.Add(_exitGameEntry);
            MenuEntries.Add(_backEntry);
        }
    }
}

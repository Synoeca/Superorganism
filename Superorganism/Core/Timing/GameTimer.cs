using Microsoft.Xna.Framework;

namespace Superorganism.Core.Timing
{
    /// <summary>
    /// A custom timer that tracks only actual gameplay time, excluding time when
    /// the game is paused, loading, or in menus. This ensures that AI behaviors,
    /// strategy durations, and other time-dependent game logic progress only
    /// during active gameplay.
    /// </summary>
    public static class GameTimer
    {
        /// <summary>
        /// Total gameplay time in seconds, excluding pauses and non-gameplay states.
        /// </summary>
        private static double _totalGameplayTime = 0.0;

        /// <summary>
        /// Whether the timer is currently active (not paused).
        /// </summary>
        private static bool _isActive = true;

        /// <summary>
        /// Gets the total gameplay time in seconds, excluding periods when
        /// the game was paused or in non-gameplay states.
        /// </summary>
        public static double TotalGameplayTime => _totalGameplayTime;

        /// <summary>
        /// Updates the gameplay timer. This should be called once per frame
        /// during actual gameplay.
        /// </summary>
        /// <param name="gameTime">The game's timing information.</param>
        public static void Update(GameTime gameTime)
        {
            if (_isActive)
            {
                // Only accumulate the frame time when actively playing
                _totalGameplayTime += gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        /// <summary>
        /// Pauses the gameplay timer. Time will not accumulate while paused.
        /// Call this when entering pause menus, loading screens, or other non-gameplay states.
        /// </summary>
        public static void Pause()
        {
            _isActive = false;
        }

        /// <summary>
        /// Resumes the gameplay timer after being paused.
        /// Call this when returning to active gameplay.
        /// </summary>
        public static void Resume()
        {
            _isActive = true;
        }

        /// <summary>
        /// Resets the gameplay timer to zero.
        /// Call this when starting a new game or when timing needs to be reset.
        /// </summary>
        public static void Reset()
        {
            _totalGameplayTime = 0.0;
            _isActive = true;
        }

        /// <summary>
        /// Loads the gameplay timer from a saved value.
        /// Call this when loading a saved game to restore the timer state.
        /// </summary>
        /// <param name="gameplayTime">The saved gameplay time in seconds.</param>
        public static void Load(double gameplayTime)
        {
            _totalGameplayTime = gameplayTime;
            _isActive = true;  // Assume active when loading
        }

        /// <summary>
        /// Gets the elapsed gameplay time since the last frame.
        /// This only includes time when the game was actively running.
        /// </summary>
        /// <param name="gameTime">The game's timing information.</param>
        /// <returns>Elapsed gameplay time in seconds.</returns>
        public static double GetElapsedGameplayTime(GameTime gameTime)
        {
            if (!_isActive)
                return 0.0;

            return gameTime.ElapsedGameTime.TotalSeconds;
        }
    }
}
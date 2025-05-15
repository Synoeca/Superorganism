using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Superorganism.Collisions;
using Superorganism.Common;
using Superorganism.Core.Managers;
using Superorganism.Core.Timing;
using Superorganism.Interfaces;

namespace Superorganism.Entities
{
    /// <summary>
    /// Base class for entities that can be controlled by player input, featuring movement, jumping, 
    /// stamina/hunger management, and physics-based collision detection
    /// </summary>
    public class ControllableEntity : MovableAnimatedDestroyableEntity, IControllable
    {
        /// <summary>
        /// The current state of the gamepad input device
        /// </summary>
        public GamePadState GamePadState { get; set; }

        /// <summary>
        /// The current state of the keyboard input device
        /// </summary>
        public KeyboardState KeyboardState { get; set; }

        /// <summary>
        /// Indicates whether this entity is currently being controlled by player input
        /// </summary>
        public bool IsControlled { get; set; }

        /// <summary>
        /// Internal field storing the entity's movement speed
        /// </summary>
        protected float _movementSpeed;

        /// <summary>
        /// Gets or sets the movement speed of the controllable entity
        /// This value is affected by stamina levels and other status effects
        /// </summary>
        public float MovementSpeed
        {
            get => _movementSpeed;
            set => _movementSpeed = value;
        }

        /// <summary>
        /// The keyboard state from the previous frame
        /// Used to detect key press/release events
        /// </summary>
        public KeyboardState PreviousKeyboardState { get; set; }

        /// <summary>
        /// The friction coefficient applied to movement
        /// Higher values result in more gradual stops
        /// </summary>
        public float Friction { get; set; } = 0.9f;

        /// <summary>
        /// The gravity force applied to the entity each frame
        /// Positive values pull the entity downward
        /// </summary>

        public float Gravity { get; set; } = 0.5f;

        /// <summary>
        /// Sound effect played when the entity moves
        /// Different sounds may play based on movement speed
        /// </summary>
        public SoundEffect MoveSound { get; set; }

        /// <summary>
        /// Sound effect played when the entity jumps
        /// </summary>
        public SoundEffect JumpSound { get; set; }

        /// <summary>
        /// Timer used to regulate the frequency of movement sound playback
        /// </summary>
        protected float _soundTimer;

        /// <summary>
        /// Timer tracking time since last jump to enforce cooldown
        /// </summary>
        private double _lastJumpTime = 0.0;

        /// <summary>
        /// Minimum time in seconds that must elapse between jumps
        /// </summary>
        private const float JumpCooldownDuration = 0.3f;


        /// <summary>
        /// Time interval between movement sounds during normal walking
        /// </summary>
        private const float MoveSoundInterval = 0.25f;

        /// <summary>
        /// Time interval between movement sounds during sprinting (shift key held)
        /// </summary>
        private const float ShiftMoveSoundInterval = 0.15f;

        private bool _lastShiftState = false;
        private bool _lastSpaceState = false;
        private bool _canSprint = true;
        private bool _canJump = true;


        /// <summary>
        /// Determines the time interval between movement sounds based on current movement speed
        /// Returns shorter intervals when sprinting (shift key held)
        /// </summary>
        /// <returns>The sound interval in seconds</returns>
        private float GetMoveSoundInterval()
        {
            return (KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.RightShift))
                ? ShiftMoveSoundInterval
                : MoveSoundInterval;
        }

        /// <summary>
        /// Plays movement sounds at regular intervals based on the current movement state
        /// Automatically adjusts timing for walking vs sprinting
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        protected void PlayMoveSound(GameTime gameTime)
        {
            _soundTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_soundTimer >= GetMoveSoundInterval())
            {
                MoveSound?.Play();
                _soundTimer = 0f;
            }
        }

        /// <summary>
        /// Updates the animation frames for the entity based on movement state
        /// Handles different animation patterns for directional vs non-directional sprites
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void UpdateAnimation(GameTime gameTime)
        {
            if (!IsSpriteAtlas) return;

            AnimationTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (AnimationTimer > AnimationSpeed)
            {
                if (HasDirection)
                {
                    AnimationFrame++;
                    if (AnimationFrame >= TextureInfo.NumOfSpriteCols)
                    {
                        AnimationFrame = 0;
                    }
                }
                else
                {
                    if (_isJumping)
                    {
                        AnimationFrame = 0;
                    }
                    else
                    {
                        // For walking animation: use frames 1 and 2 when moving
                        if (Math.Abs(_velocity.X) > 0.1f)
                        {
                            AnimationFrame++;
                            if (AnimationFrame < 1 || AnimationFrame > 2)  // Ensure we only use frames 1 and 2
                            {
                                AnimationFrame = 1;
                            }
                        }
                        else
                        {
                            AnimationFrame = 0;  // Idle frame
                        }
                    }
                }
                AnimationTimer -= AnimationSpeed;
            }
        }

        /// <summary>
        /// Handles player input processing and applies appropriate movement/action responses
        /// Automatically switches between stamina-restricted and regular movement based on EntityStatus availability
        /// </summary>
        /// <param name="keyboardState">Current keyboard input state</param>
        /// <param name="gamePadState">Current gamepad input state</param>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public virtual void HandleInput(KeyboardState keyboardState, GamePadState gamePadState, GameTime gameTime)
        {
            GamePadState = GamePad.GetState(0);
            KeyboardState = Keyboard.GetState();

            // Check current key states
            bool isShiftPressed = KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.RightShift);
            bool isSpacePressed = KeyboardState.IsKeyDown(Keys.Space);

            // Check for fresh key presses (key was released and now pressed again)
            bool isShiftFreshPress = isShiftPressed && !_lastShiftState;
            bool isSpaceFreshPress = isSpacePressed && !_lastSpaceState;

            // Declare local variables for current sprint/jump abilities
            bool canSprint;
            bool canJump;

            // Calculate time elapsed since last jump using GameTimer
            double currentTime = GameTimer.TotalGameplayTime;
            double timeSinceLastJump = currentTime - _lastJumpTime;

            // If stamina is zero, disable abilities until fresh key press
            if (EntityStatus != null)
            {
                if (EntityStatus.Stamina <= 0)
                {
                    _canSprint = false;
                    _canJump = false;
                }
                else
                {
                    // Allow abilities only on fresh press when stamina is available
                    if (isShiftFreshPress && EntityStatus.Stamina > EntityStatus.SprintStaminaThreshold)
                        _canSprint = true;
                    if (isSpaceFreshPress && EntityStatus.Stamina > EntityStatus.JumpStaminaThreshold)
                        _canJump = true;
                }

                // Reset abilities when keys are released
                if (!isShiftPressed)
                    _canSprint = true;
                if (!isSpacePressed)
                    _canJump = true;

                // Use the current ability states combined with stamina check
                canSprint = _canSprint && EntityStatus.Stamina > EntityStatus.SprintStaminaThreshold;
                canJump = _canJump && EntityStatus.Stamina > EntityStatus.JumpStaminaThreshold && timeSinceLastJump >= JumpCooldownDuration;
            }
            else
            {
                // If no EntityStatus, allow all abilities
                canSprint = true;
                canJump = true;
            }

            // Update tracking variables
            _lastShiftState = isShiftPressed;
            _lastSpaceState = isSpacePressed;

            // Check for jump before movement utilities to handle the sound
            bool wasJumping = _isJumping;
            bool wasOnGround = _isOnGround;

            // Use MovementUtilities to handle player input and physics
            if (EntityStatus != null)
            {
                // Use stamina restrictions if EntityStatus is available
                MovementUtilities.HandlePlayerInputWithStaminaRestrictions(
                    ref _position,
                    ref _velocity,
                    ref _isOnGround,
                    ref _isJumping,
                    ref _jumpDiagonalPosY,
                    ref _isCenterOnDiagonal,
                    ref _soundTimer,
                    ref _movementSpeed,
                    ref _animationSpeed,
                    KeyboardState,
                    PreviousKeyboardState,
                    GameState.CurrentMap,
                    CollisionBounding,
                    TextureInfo,
                    EntityStatus,
                    ref Flipped,
                    Friction,
                    Gravity,
                    EntityStatus.JumpStrength,
                    PlayMoveSound,
                    canSprint,    // Use the local canSprint variable
                    canJump,      // Use the local canJump variable
                    gameTime);
            }
            else
            {
                // Use regular movement if no EntityStatus
                MovementUtilities.HandlePlayerInput(
                    ref _position,
                    ref _velocity,
                    ref _isOnGround,
                    ref _isJumping,
                    ref _jumpDiagonalPosY,
                    ref _isCenterOnDiagonal,
                    ref _soundTimer,
                    ref _movementSpeed,
                    ref _animationSpeed,
                    KeyboardState,
                    PreviousKeyboardState,
                    GameState.CurrentMap,
                    CollisionBounding,
                    TextureInfo,
                    EntityStatus,
                    ref Flipped,
                    Friction,
                    Gravity,
                    EntityStatus.JumpStrength,
                    PlayMoveSound,
                    gameTime);
            }

            // Play jump sound if we just started jumping and reset cooldown timer
            if (!wasJumping && _isJumping && wasOnGround)
            {
                JumpSound?.Play();
                _lastJumpTime = GameTimer.TotalGameplayTime;  // Record the jump time using GameTimer
            }

            // Update the previous keyboard state
            PreviousKeyboardState = KeyboardState;
        }

        /// <summary>
        /// Updates the entity's state, including collision boundaries, resource management, and input handling
        /// This method is called each frame to maintain entity behavior
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public void LoadSound(ContentManager content)
        {
            MoveSound = content.Load<SoundEffect>("move");
            JumpSound = content.Load<SoundEffect>("Jump");
        }

        /// <summary>
        /// Updates the entity's state, including collision boundaries, resource management, and input handling
        /// This method is called each frame to maintain entity behavior
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Update(GameTime gameTime)
        {
            CollisionBounding ??= TextureInfo.CollisionType;
            if (CollisionBounding is BoundingCircle bc)
            {
                bc.Center = new Vector2(Position.X + (bc.Radius / 2), Position.Y + (bc.Radius / 2));
                CollisionBounding = bc;
            }
            else if (CollisionBounding is BoundingRectangle br)
            {
                br = new BoundingRectangle(Position, br.Width, br.Height);
                CollisionBounding = br;
            }

            // Track if we just started jumping this frame
            bool wasJumping = _isJumping;

            // Process resource management if EntityStatus is available
            if (EntityStatus != null)
            {
                float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Check for active movement input (not just velocity)
                bool isActivelyMoving = KeyboardState.IsKeyDown(Keys.Left) ||
                                      KeyboardState.IsKeyDown(Keys.Right) ||
                                      KeyboardState.IsKeyDown(Keys.A) ||
                                      KeyboardState.IsKeyDown(Keys.D);

                // Check if shift is pressed
                bool isShiftPressed = KeyboardState.IsKeyDown(Keys.LeftShift) ||
                                     KeyboardState.IsKeyDown(Keys.RightShift);

                // Only consider it actual sprinting if we can sprint and have stamina
                bool isSprinting = isShiftPressed && _canSprint &&
                                  EntityStatus.Stamina > EntityStatus.SprintStaminaThreshold;

                // Update resources first to get new state
                MovementSpeed = EntityStatus.UpdateResourceManagement(deltaTime, isActivelyMoving, isSprinting, false);

                // Handle input which might trigger jumping
                HandleInput(KeyboardState, GamePadState, gameTime);

                // If we just started jumping this frame, consume stamina
                if (!wasJumping && _isJumping)
                {
                    EntityStatus.UpdateResourceManagement(0, false, false, true);
                }
            }
            else
            {
                HandleInput(KeyboardState, GamePadState, gameTime);
            }
        }
    }
}
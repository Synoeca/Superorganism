using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Superorganism
{
	public abstract class AnimatedEntity : IAnimated
	{
		// Position property (could be added for convenience)
		protected Vector2 _position;
		public Vector2 Position
		{
			get => _position;
			set => _position = value;
		}

		// Health Points
		public int HitPoints { get; set; } = 100;
		public int MaxHitPoints { get; private set; } = 100;

		// Sound effect variables
		//protected SoundEffect _moveSound;
		//protected SoundEffect _jumpSound;
		//protected float _soundTimer = 0.0f;
		//protected float _moveSoundInterval = 0.25f;

		// Animation variables
		protected float _animationTimer = 0f;
		protected float _animationInterval = 0.15f;
		protected Texture2D _currentTexture;

		// Constructor
		//protected AnimatedEntity(Vector2 initialPosition)
		//{
		//	_position = initialPosition;
		//}

		// Implement abstract methods for interfaces
		public abstract void UpdateAnimation(GameTime gameTime, float movementSpeed);
		public abstract void DrawAnimation(SpriteBatch spriteBatch);

		//// Implement sound effect methods
		//public virtual void PlayMoveSound()
		//{
		//	if (_moveSound != null && _soundTimer <= 0)
		//	{
		//		_moveSound.Play();
		//		_soundTimer = _moveSoundInterval; // Reset the sound timer to interval
		//	}
		//}

		//public virtual void PlayJumpSound()
		//{
		//	if (_jumpSound != null)
		//	{
		//		_jumpSound.Play();
		//	}
		//}

		//// Update method to manage sound timer
		//public void UpdateSoundTimer(GameTime gameTime)
		//{
		//	if (_soundTimer > 0)
		//	{
		//		_soundTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
		//	}
		//}
	}
}

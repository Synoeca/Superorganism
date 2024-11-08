using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Collisions;

namespace Superorganism;

public abstract class AnimatedEntity(Vector2 position) : IEntity, IAnimated
{
	

	private int _animationFrame;

	private double _animationTimer;

	protected Vector2 _position = position;
	protected float AnimationInterval = 0.15f;
	private short _animationFrame1;

	// Animation variables
	//protected float AnimationTimer = 0f;

	public bool Collected { get; set; }

	public BoundingCircle Bounds { get; } = new(position + new Vector2(8, 8), 8);

	public bool IsSpriteAtlas { get; }
	public bool HasDirection { get; }
	public TextureInfo TextureInfo { get; }
	public int NumOfSpriteCols { get; }
	public int NumOfSpriteRows { get; }
	public int DirectionIndex { get; }
	public double DirectionTimer { get; }
	public double DirectionInterval { get; }
	public double AnimationTimer { get; set; }

	public float AnimationSpeed { get; } = 0.1f;
	short IAnimated.AnimationFrame => _animationFrame1;

	public double AnimationFrame { get; set; }

	public virtual void UpdateAnimation(GameTime gameTime)
	{
		_animationTimer += gameTime.ElapsedGameTime.TotalSeconds;

		if (!(_animationTimer > AnimationSpeed)) return;
		_animationFrame++;
		if (_animationFrame > 7) { _animationFrame = 0; }
		_animationTimer -= AnimationSpeed;
	}

	public virtual void DrawAnimation(SpriteBatch spriteBatch)
	{
		Rectangle source = new(_animationFrame * 16, 0, 16, 16);
		spriteBatch.Draw(Texture, Position, source, Color.White);
	}

	public Texture2D Texture { get; set; }

	public Vector2 Position
	{
		get => _position;
		set => _position = value;
	}

	public Color Color { get; }


	public virtual void LoadContent(ContentManager content)
	{
		Texture = content.Load<Texture2D>("crops");
	}

	public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
	{
		if (Collected) return;
		UpdateAnimation(gameTime);
		DrawAnimation(spriteBatch);
	}

	public void Update(bool collected)
	{
		Collected = collected;
	}
}
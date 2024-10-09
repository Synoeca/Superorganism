using Microsoft.Xna.Framework;

namespace Superorganism;

public class AntSprite : ControllableEntity
{
	public AntSprite(Vector2 position) : base(position)
	{
		_position = position;
	}

	public int HitPoint { get; set; } = 100;
	public int MaxHitPoint { get; private set; } = 100;
}
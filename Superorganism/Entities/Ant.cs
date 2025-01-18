using Microsoft.Xna.Framework;

namespace Superorganism.Entities
{
    public sealed class Ant : ControllableEntity
    {
	    public Ant()
	    {
		    IsSpriteAtlas = true;
		    HasDirection = false;
            Color = Color.White;
        }

	    public int HitPoints { get; set; } = 100;

	    public int MaxHitPoint { get; set; } = 100;

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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

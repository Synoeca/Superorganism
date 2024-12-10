using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Superorganism.Collisions;

namespace Superorganism.Entities
{
	public class Crop : StaticAnimatedCollectableEntity
	{
		public override bool IsSpriteAtlas { get; set; } = true;
		public override bool HasDirection { get; set; } = false;
		public override int DirectionIndex { get; set; } = 0;
		public override float AnimationSpeed { get; set; } = 0.1f;
    }
}

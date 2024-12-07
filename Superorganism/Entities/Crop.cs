﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Superorganism.Collisions;

namespace Superorganism.Entities
{
	public class Crop(Vector2 position) : StaticAnimatedCollectableEntity(position)
	{
		public override bool IsSpriteAtlas { get; set; } = true;
		public override bool HasDirection { get; set; } = false;
		public override int DirectionIndex { get; set; } = 0;
		public override float AnimationSpeed { get; set; } = 0.1f;
		public override ICollisionBounding CollisionBounding { get; set; } = new BoundingCircle(position + new Vector2(8, 8), 8);
	}
}

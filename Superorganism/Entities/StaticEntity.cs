using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.Collisions;
using Superorganism.Common;
using Superorganism.Core.Managers;
using Superorganism.Tiles;

namespace Superorganism.Entities
{
	public class StaticEntity : Entity
	{
		public override Texture2D Texture { get; set; }
		public override EntityStatus EntityStatus { get; set; }
        public override Vector2 Position { get; set; }
		public override Color Color { get; set; } = Color.White;
        public virtual Vector2 Velocity { get; set; }

        protected ICollisionBounding _collisionBounding;
        public override ICollisionBounding CollisionBounding
        {
            get => _collisionBounding;
            set => _collisionBounding = value;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Texture, Position, Color);
		}

        public override void Update(GameTime gameTime)
        {
            if (CollisionBounding == null)
            {
                CollisionBounding = TextureInfo.CollisionType;
                switch (CollisionBounding)
                {
                    case BoundingCircle br:
                        br.Center = new Vector2(Position.X + br.Radius * TextureInfo.SizeScale, Position.Y + br.Radius * TextureInfo.SizeScale);
                        CollisionBounding = br;
                        break;

                    case BoundingRectangle br:
                        br.X = Position.X;
                        br.Y = Position.Y;
                        br.Width = TextureInfo.UnitTextureHeight * TextureInfo.SizeScale;
                        br.Height = TextureInfo.UnitTextureHeight * TextureInfo.SizeScale;
                        CollisionBounding = br;
                        break;
                }
            }

            Velocity = Velocity with { X = 0 };
            Velocity = Velocity with { Y = Velocity.Y + 0.5f };

            Vector2 newPosition = Position + Velocity;

            // Check map bounds
            newPosition.X = MathHelper.Clamp(newPosition.X,
                (TextureInfo.UnitTextureWidth * TextureInfo.SizeScale) / 2f,
                MapHelper.GetMapWorldBounds().Width - (TextureInfo.UnitTextureWidth * TextureInfo.SizeScale) / 2f);

            // Get ground level at new position
            float groundY = MapHelper.GetGroundYPosition(
                GameState.CurrentMap,
                newPosition.X,
                Position.Y,
                TextureInfo.UnitTextureHeight * TextureInfo.SizeScale,
                CollisionBounding
            );

            // Handle ground collision
            if (newPosition.Y > groundY - (TextureInfo.UnitTextureHeight * TextureInfo.SizeScale))
            {
                newPosition.Y = groundY - (TextureInfo.UnitTextureHeight * TextureInfo.SizeScale);
                Velocity = Velocity with { Y = 0 };
            }

            Position = newPosition;
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
        }
    }
}

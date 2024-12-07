using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.AI;
using Superorganism.Collisions;
using Superorganism.Common;
using Superorganism.Enums;
using Superorganism.Interfaces;

namespace Superorganism.Entities
{
    public class MovableEntity : Entity, IMovable
	{
		public override Texture2D Texture { get; set; }
		public override EntityStatus EntityStatus { get; set; }
        public override Color Color { get; set; }

        protected List<(Strategy Strategy, double StartTime, double LastActionTime)> _strategyHistory = [];
        public List<(Strategy Strategy, double StartTime, double LastActionTime)> StrategyHistory
        {
            get => _strategyHistory;
            set => _strategyHistory = value;
        }

        protected Strategy _strategy;
		public virtual Strategy Strategy
		{
			get => _strategy;
			set => _strategy = value;
		}

		public void ApplyGravity(float gravity)
		{
			Velocity += new Vector2(0, gravity);
		}

		protected Direction _direction;
		public virtual Direction Direction
		{
			get => _direction;
			set => _direction = value;
		}

		protected Vector2 _velocity;
		public Vector2 Velocity
		{
			get => _velocity;
			set => _velocity = value;
		}

		protected Vector2 _position;
		public override Vector2 Position
		{
			get => _position;
			set => _position = value;
		}

		protected double _directionTimer = 0;
		public virtual double DirectionTimer
		{
			get => _directionTimer;
			set => _directionTimer = value;
		}

		protected double _directionInterval;
		public virtual double DirectionInterval
		{
			get => _directionInterval;
			set => _directionInterval = value;
		}

		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Texture, Position, Color);
		}

		public void Move(Vector2 direction)
		{
			Position += Velocity * direction;
			Direction = Math.Abs(direction.X) > Math.Abs(direction.Y)
				? direction.X > 0 ? Direction.Right : Direction.Left
				: direction.Y > 0 ? Direction.Down : Direction.Up;
		}

		public override void Update(GameTime gameTime)
		{
			DecisionMaker.Action(ref _strategy, ref _strategyHistory, gameTime, ref _direction, ref _position, ref _directionTimer, ref _directionInterval,
				ref _velocity, 800, 600, TextureInfo, EntityStatus); 
		}
	}
}

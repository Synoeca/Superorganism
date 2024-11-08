using Microsoft.Xna.Framework;
using Superorganism.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Superorganism.Enums;
using Superorganism.Collisions;

namespace Superorganism
{
    public static class DecisionMaker
    {
        public static GameTime GameTime { get; set; }
        public static List<Entity> Entities { get; set; }
        public static Strategy Strategy { get; set; }


        public static void Action(Strategy strategy, GameTime gameTime, ref Direction direction, ref Vector2 position, 
	        ref double directionTimer, double directionInterval,  ref ICollisionBounding collisionBounding)
        {
            //foreach (Entity entity in Entities)
            //{
            //    //if (entity is MoveableEntity)
            //    //{
            //    //          //entity
            //    //}
            //}
            if (strategy == Strategy.RandomMovement)
            {
	            directionTimer += gameTime.ElapsedGameTime.TotalSeconds;
	            if (directionTimer > directionInterval)
	            {
		            switch (direction)
		            {
			            case Direction.Up:
				            direction = Direction.Down;
				            break;
			            case Direction.Down:
				            direction = Direction.Right;
				            break;
			            case Direction.Right:
				            direction = Direction.Left;
				            break;
			            case Direction.Left:
				            direction = Direction.Up;
				            break;
		            }
		            directionTimer -= directionInterval;
	            }

	            switch (direction)
	            {
		            case Direction.Up:
			            position += new Vector2(0, -1) * 100 * (float)gameTime.ElapsedGameTime.TotalSeconds;
			            break;
		            case Direction.Down:
			            position += new Vector2(0, 1) * 100 * (float)gameTime.ElapsedGameTime.TotalSeconds;
			            break;
		            case Direction.Left:
			            position += new Vector2(-1, 0) * 100 * (float)gameTime.ElapsedGameTime.TotalSeconds;
			            break;
		            case Direction.Right:
			            position += new Vector2(1, 0) * 100 * (float)gameTime.ElapsedGameTime.TotalSeconds;
			            break;
	            }
	            collisionBounding.Center = position + new Vector2(16, 16);
			}
        }
    }
}

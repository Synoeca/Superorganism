using Microsoft.Xna.Framework;
using Superorganism.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Superorganism
{
    public class DecisionMaker
    {
        public GameTime GameTime { get; set; }
        public List<Entity> Entities { get; set; }
        public Strategy Strategy { get; set; }
        

        public Vector2 Action()
        {
	        foreach (Entity entity in Entities)
	        {
		        if (entity is MoveableEntity)
		        {
                    //entity
		        }
	        }
	        return new Vector2();
        }
    }
}

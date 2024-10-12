using Microsoft.Xna.Framework;

namespace Superorganism.Particle
{
	public interface IParticleEmitter
	{
		public Vector2 Position { get;  }

		public Vector2 Velocity { get;  }
	}
}

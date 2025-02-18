using Microsoft.Xna.Framework;

namespace Superorganism.Particle
{
	public class RainParticleSystem : ParticleSystem
	{
		Rectangle _source;

		public bool IsRaining { get; set; } = true;

		public RainParticleSystem(Game game, Rectangle source) : base(game, 4000)
		{
			_source = source;
		}

		protected override void InitializeConstants()
		{
			TextureFilename = "drop";
			MinNumParticles = 10;
			MaxNumParticles = 20;
		}

		protected override void InitializeParticle(ref Particle p, Vector2 where)
		{
			p.Initialize(where, Vector2.UnitY * 260, Vector2.Zero, Color.LightSkyBlue, scale: RandomHelper.NextFloat(0.1f, 0.4f), lifetime: 3);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			if (IsRaining) { AddParticles(_source); }
		}


	}
}

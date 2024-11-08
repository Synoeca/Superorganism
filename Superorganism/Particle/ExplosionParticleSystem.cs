using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Superorganism.Particle
{
	public class ExplosionParticleSystem : ParticleSystem
	{
		public ExplosionParticleSystem(Game game, int maxExplosions) : base(game, maxExplosions * 25) { }

		protected override void InitializeConstants()
		{
			TextureFilename = "explosion";

			MinNumParticles = 20;
			MaxNumParticles = 25;

			BlendState = BlendState.Additive;
			DrawOrder = AdditiveBlendDrawOrder;
		}

		protected override void InitializeParticle(ref Particle p, Vector2 where)
		{
			var velocity = RandomHelper.NextDirection() * RandomHelper.NextFloat(40, 200);

			var lifetime = RandomHelper.NextFloat(0.5f, 1.0f);

			var acceleration = -velocity / lifetime;

			var rotation = RandomHelper.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4);

			var angularVelocity = RandomHelper.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4);

			p.Initialize(where, velocity, acceleration, lifetime: lifetime, rotation: rotation, angularVelocity: angularVelocity);
		}

		protected override void UpdateParticle(ref Particle particle, float dt)
		{
			base.UpdateParticle(ref particle, dt);

			float normalizedLifetime = particle.TimeSinceStart / particle.Lifetime;

			float alpha = 4 * normalizedLifetime * (1 - normalizedLifetime);

			particle.Color = Color.White * alpha;

			particle.Scale = 0.1f + 0.25f * normalizedLifetime;
		}

		public void PlaceExplosion(Vector2 where) => AddParticles(where);
	}
}

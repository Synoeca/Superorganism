using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Superorganism.Particle
{
	public class FireWorkParticleSystem : ParticleSystem
	{
		private Color[] _colors = new Color[]
		{
			Color.Fuchsia,
			Color.Red,
			Color.Crimson,
			Color.CadetBlue,
			Color.Aqua,
			Color.HotPink,
			Color.LimeGreen
		};

		private Color color;

		public FireWorkParticleSystem(Game game, int maxExplosions) : base(game, maxExplosions * 25) { }

		protected override void InitializeConstants()
		{
			textureFilename = "circle";

			minNumParticles = 20;
			maxNumParticles = 25;

			blendState = BlendState.Additive;
			DrawOrder = AdditiveBlendDrawOrder;
		}

		protected override void InitializeParticle(ref Particle p, Vector2 where)
		{
			var velocity = RandomHelper.NextDirection() * RandomHelper.NextFloat(40, 200);

			var lifetime = RandomHelper.NextFloat(0.5f, 1.0f);

			var acceleration = -velocity / lifetime;

			var rotation = RandomHelper.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4);

			var angularVelocity = RandomHelper.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4);

			var scale = RandomHelper.NextFloat(14, 26);

			p.Initialize(where, velocity, acceleration, color, lifetime: lifetime, rotation: rotation, angularVelocity: angularVelocity, scale: scale);
		}

		protected override void UpdateParticle(ref Particle particle, float dt)
		{
			base.UpdateParticle(ref particle, dt);

			float normalizedLifetime = particle.TimeSinceStart / particle.Lifetime;

			particle.Scale = 0.1f + 0.25f * normalizedLifetime;
		}

		public void PlaceFireWork(Vector2 where)
		{
			color = _colors[RandomHelper.Next(_colors.Length)];
			AddParticles(where);
		}
	}
}

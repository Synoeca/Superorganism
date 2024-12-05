using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Superorganism.Particle
{
	public class FireWorkParticleSystem : ParticleSystem
	{
		private Color[] _colors =
        [
            Color.Fuchsia,
			Color.Red,
			Color.Crimson,
			Color.CadetBlue,
			Color.Aqua,
			Color.HotPink,
			Color.LimeGreen
        ];

		private Color _color;

		public FireWorkParticleSystem(Game game, int maxExplosions) : base(game, maxExplosions * 25) { }

		protected override void InitializeConstants()
		{
			TextureFilename = "circle";

			MinNumParticles = 20;
			MaxNumParticles = 25;

			BlendState = BlendState.Additive;
			DrawOrder = AdditiveBlendDrawOrder;
		}

		protected override void InitializeParticle(ref Particle p, Vector2 where)
		{
			Vector2 velocity = RandomHelper.NextDirection() * RandomHelper.NextFloat(40, 200);

			float lifetime = RandomHelper.NextFloat(0.5f, 1.0f);

			Vector2 acceleration = -velocity / lifetime;

			float rotation = RandomHelper.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4);

			float angularVelocity = RandomHelper.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4);

			float scale = RandomHelper.NextFloat(14, 26);

			p.Initialize(where, velocity, acceleration, _color, lifetime: lifetime, rotation: rotation, angularVelocity: angularVelocity, scale: scale);
		}

		protected override void UpdateParticle(ref Particle particle, float dt)
		{
			base.UpdateParticle(ref particle, dt);

			float normalizedLifetime = particle.TimeSinceStart / particle.Lifetime;

			particle.Scale = 0.1f + 0.25f * normalizedLifetime;
		}

		public void PlaceFireWork(Vector2 where)
		{
			_color = _colors[RandomHelper.Next(_colors.Length)];
			AddParticles(where);
		}
	}
}

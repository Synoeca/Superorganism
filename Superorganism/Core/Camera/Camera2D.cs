using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Superorganism.Core.Camera
{
    public class Camera2D
    {
        private Vector2 _position;
        private readonly float _zoom;
        private Matrix _transformMatrix;
        private readonly GraphicsDevice _graphicsDevice;

        public Camera2D(GraphicsDevice graphicsDevice, float zoom = 1f)
        {
            _graphicsDevice = graphicsDevice;
            _zoom = zoom;
        }

        public void Update(Vector2 targetPosition)
        {
            _position = targetPosition;
            UpdateTransformMatrix();
        }

        private void UpdateTransformMatrix()
        {
            _transformMatrix = Matrix.CreateTranslation(new Vector3(
                                   -_position.X + _graphicsDevice.Viewport.Width / 2.0f,
                                   -_position.Y + _graphicsDevice.Viewport.Height / 2.0f,
                                   0)) *
                               Matrix.CreateScale(_zoom);
        }

        public Matrix TransformMatrix => _transformMatrix;
    }
}
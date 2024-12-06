using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Superorganism.Core.Camera
{
    public class Camera2D
    {
        private Vector2 _position;
        private readonly float _baseZoom;
        private float _currentZoom;
        private float _targetZoom;
        private float _rotation;
        private float _shakeIntensity;
        private float _shakeTimer;
        private bool _isShaking;
        private Vector2 _focusTarget;
        private bool _isTransitioning;
        private float _transitionTimer;
        private readonly GraphicsDevice _graphicsDevice;
        private Matrix _transformMatrix;
        private readonly Random _random;

        // Constants for effects
        private const float ZOOM_SPEED = 2f;
        private const float MAX_SHAKE_INTENSITY = 15f;
        private const float SHAKE_DECAY = 0.95f;
        private const float TRANSITION_SPEED = 3f;
        private const float INITIAL_ZOOM = 0.2f;

        public Matrix TransformMatrix => _transformMatrix;

        public Camera2D(GraphicsDevice graphicsDevice, float baseZoom)
        {
            _graphicsDevice = graphicsDevice;
            _baseZoom = baseZoom;
            _currentZoom = INITIAL_ZOOM;
            _targetZoom = baseZoom;
            _random = new Random();
            _position = Vector2.Zero;
            _focusTarget = Vector2.Zero;
            _transformMatrix = Matrix.Identity;
        }

        public void Initialize(Vector2 startPosition)
        {
            _position = startPosition;
            _focusTarget = startPosition;
            _currentZoom = INITIAL_ZOOM;
            _targetZoom = _baseZoom;
            UpdateTransformMatrix();
        }

        public void StartShake(float intensity = 1.0f)
        {
            _isShaking = true;
            _shakeIntensity = Math.Min(intensity * MAX_SHAKE_INTENSITY, MAX_SHAKE_INTENSITY);
            _shakeTimer = 0;
        }

        public void TransitionToTarget(Vector2 target, float zoomLevel = 1.0f)
        {
            _isTransitioning = true;
            _transitionTimer = 0;
            _focusTarget = target;
            _targetZoom = _baseZoom * zoomLevel;
        }

        public void Update(Vector2 playerPosition, GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Handle initial zoom out effect
            if (_currentZoom < _targetZoom)
            {
                _currentZoom = MathHelper.Lerp(_currentZoom, _targetZoom, ZOOM_SPEED * deltaTime);
            }

            // Handle transition to target
            if (_isTransitioning)
            {
                _transitionTimer += deltaTime;
                float progress = Math.Min(_transitionTimer * TRANSITION_SPEED, 1f);
                _position = Vector2.Lerp(_position, _focusTarget, progress);

                if (progress >= 1f)
                {
                    _isTransitioning = false;
                    _focusTarget = playerPosition;
                }
            }
            else
            {
                // Smooth follow player when not transitioning
                _position = Vector2.Lerp(_position, playerPosition, 5f * deltaTime);
            }

            // Handle camera shake
            Vector2 shakeOffset = Vector2.Zero;
            if (_isShaking)
            {
                _shakeTimer += deltaTime;
                _shakeIntensity *= SHAKE_DECAY;

                if (_shakeIntensity > 0.1f)
                {
                    float offsetX = (_random.Next(-100, 100) / 100f) * _shakeIntensity;
                    float offsetY = (_random.Next(-100, 100) / 100f) * _shakeIntensity;
                    shakeOffset = new Vector2(offsetX, offsetY);
                }
                else
                {
                    _isShaking = false;
                    _shakeIntensity = 0;
                }
            }

            // Apply all transformations
            UpdateTransformMatrix(shakeOffset);
        }

        private void UpdateTransformMatrix(Vector2 shakeOffset = default)
        {
            Vector2 screenCenter = new(
                _graphicsDevice.Viewport.Width * 0.5f,
                _graphicsDevice.Viewport.Height * 0.5f
            );

            _transformMatrix =
                Matrix.CreateTranslation(new Vector3(-_position - shakeOffset, 0.0f)) *
                Matrix.CreateRotationZ(_rotation) *
                Matrix.CreateScale(_currentZoom) *
                Matrix.CreateTranslation(new Vector3(screenCenter, 0.0f));
        }

        // Helper method to reset camera
        public void Reset(Vector2 position)
        {
            _position = position;
            _focusTarget = position;
            _currentZoom = _baseZoom;
            _targetZoom = _baseZoom;
            _rotation = 0f;
            _isShaking = false;
            _isTransitioning = false;
            UpdateTransformMatrix();
        }
    }
}
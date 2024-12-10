using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Superorganism.Core.Camera
{
    public class Camera2D(GraphicsDevice graphicsDevice, float baseZoom)
    {
        private Vector2 _position = Vector2.Zero;
        private readonly float _baseZoom = baseZoom;
        private float _currentZoom = InitialZoom;
        private float _targetZoom = baseZoom;
        private float _rotation;
        private float _shakeIntensity;
        private float _shakeTimer;
        private bool _isShaking;
        private Vector2 _focusTarget = Vector2.Zero;
        private bool _isTransitioning;
        private float _transitionTimer;
        private readonly Random _random = new();

        // Constants for effects
        private const float ZoomSpeed = 2f;
        private const float MaxShakeIntensity = 15f;
        private const float ShakeDecay = 0.95f;
        private const float TransitionSpeed = 3f;
        private const float InitialZoom = 0.2f;

        public Matrix TransformMatrix { get; private set; } = Matrix.Identity;

        public void Initialize(Vector2 startPosition)
        {
            _position = startPosition;
            _focusTarget = startPosition;
            _currentZoom = InitialZoom;
            _targetZoom = _baseZoom;
            UpdateTransformMatrix();
        }

        public void StartShake(float intensity)
        {
            _isShaking = true;
            _shakeIntensity = Math.Min(intensity * MaxShakeIntensity, MaxShakeIntensity);
            _shakeTimer = 0;
        }

        public Vector2 Position => _position;

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
                _currentZoom = MathHelper.Lerp(_currentZoom, _targetZoom, ZoomSpeed * deltaTime);
            }

            // Handle transition to target
            if (_isTransitioning)
            {
                _transitionTimer += deltaTime;
                float progress = Math.Min(_transitionTimer * TransitionSpeed, 1f);
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
                _shakeIntensity *= ShakeDecay;

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
                graphicsDevice.Viewport.Width * 0.5f,
                graphicsDevice.Viewport.Height * 0.5f
            );

            // Calculate vertical offset to show more area above the player
            float verticalOffset = graphicsDevice.Viewport.Height * 0.1f; // Moves focus point up by 1/4 screen height
            Vector2 offsetScreenCenter = new(screenCenter.X, screenCenter.Y + verticalOffset);

            // Make sure position is in world coordinates (pixels)
            Vector2 cameraPos = _position + shakeOffset;

            TransformMatrix =
                Matrix.CreateTranslation(new Vector3(-cameraPos, 0.0f)) *
                Matrix.CreateRotationZ(_rotation) *
                Matrix.CreateScale(_currentZoom) *
                Matrix.CreateTranslation(new Vector3(offsetScreenCenter, 0.0f));
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
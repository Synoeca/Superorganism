﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.ScreenManagement;

namespace Superorganism.Core.Camera
{
    public class Camera2D(GraphicsDevice graphicsDevice, float baseZoom)
    {
        private readonly float _baseZoom = baseZoom;
        public float CurrentZoom = InitialZoom;
        private float _targetZoom = baseZoom;
        private float _rotation;
        private float _shakeIntensity;
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

        private const int MapWidth = 12800; // 200 * 64
        private const int MapHeight = 3200; // 50 * 64

        public float ScaleFactor { get; set; }

        public Matrix TransformMatrix { get; private set; } = Matrix.Identity;

        public ScreenManager ScreenManager { get; set; }

        public void Initialize(Vector2 startPosition, ScreenManager screenManager)
        {
            Position = startPosition;
            _focusTarget = startPosition;
            CurrentZoom = InitialZoom;
            _targetZoom = _baseZoom;
            ScreenManager = screenManager;
            UpdateTransformMatrix();
        }

        public void StartShake(float intensity)
        {
            _isShaking = true;
            _shakeIntensity = Math.Min(intensity * MaxShakeIntensity, MaxShakeIntensity);
        }

        public Vector2 Position { get; set; } = Vector2.Zero;

        public void TransitionToTarget(Vector2 target, float zoomLevel = 1.0f)
        {
            _isTransitioning = true;
            _transitionTimer = 0;
            _focusTarget = target;
            _targetZoom = _baseZoom * zoomLevel;
        }

        private Vector2 ClampCameraPosition(Vector2 position)
        {
            // Calculate visible area based on zoom
            float viewportWidth = graphicsDevice.Viewport.Width / CurrentZoom;
            float viewportHeight = graphicsDevice.Viewport.Height / CurrentZoom;

            // Calculate the bounds where the camera should stop
            float minX = viewportWidth / 2;
            float maxX = MapWidth - (viewportWidth / 2);
            float minY = viewportHeight / 2;
            float maxY = MapHeight - (viewportHeight / 2);

            // Clamp the camera position
            return new Vector2(
                MathHelper.Clamp(position.X, minX, maxX),
                MathHelper.Clamp(position.Y, minY, maxY)
            );
        }

        public void Update(Vector2 playerPosition, GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Handle initial zoom out effect
            

            CurrentZoom = MathHelper.Lerp(CurrentZoom, _targetZoom, ZoomSpeed * deltaTime);

            // Handle transition to target
            if (_isTransitioning)
            {
                _transitionTimer += deltaTime;
                float progress = Math.Min(_transitionTimer * TransitionSpeed, 1f);
                Vector2 clampedTarget = ClampCameraPosition(_focusTarget);
                Position = Vector2.Lerp(Position, clampedTarget, progress);

                if (progress >= 1f)
                {
                    _isTransitioning = false;
                    _focusTarget = playerPosition;
                }
            }
            else
            {
                // Smooth follow player when not transitioning
                Vector2 targetPos = ClampCameraPosition(playerPosition);
                Position = Vector2.Lerp(Position, targetPos, 5f * deltaTime);
            }

            // Handle camera shake
            Vector2 shakeOffset = Vector2.Zero;
            if (_isShaking)
            {
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

        public void UpdateTransformMatrix(Vector2 shakeOffset = default)
        {
            Vector2 screenCenter = new(
                graphicsDevice.Viewport.Width * 0.5f,
                graphicsDevice.Viewport.Height * 0.5f
            );

            // Calculate vertical offset to show more area above the player
            float verticalOffset = graphicsDevice.Viewport.Height * 0.1f;
            Vector2 offsetScreenCenter = new(screenCenter.X, screenCenter.Y + verticalOffset);

            // Make sure position is in world coordinates (pixels) and clamped
            Vector2 cameraPos = Position + shakeOffset;

            // Apply the scale factor to the current zoom
            //float adjustedZoom = CurrentZoom * ScaleFactor;

            TransformMatrix =
                Matrix.CreateTranslation(new Vector3(-cameraPos, 0.0f)) *
                Matrix.CreateRotationZ(_rotation) *
                Matrix.CreateScale(CurrentZoom) *
                Matrix.CreateTranslation(new Vector3(offsetScreenCenter, 0.0f));
        }


        // Helper method to reset camera
        public void Reset(Vector2 position)
        {
            Position = position;
            _focusTarget = position;
            CurrentZoom = _baseZoom;
            _targetZoom = _baseZoom;
            _rotation = 0f;
            _isShaking = false;
            _isTransitioning = false;
            UpdateTransformMatrix();
        }
    }
}
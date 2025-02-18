using Microsoft.Xna.Framework.Input;
using System;

public static class InputHelper
{
    public struct InputResult
    {
        public float MovementSpeed;
        public float AnimationSpeed;
        public float ProposedXVelocity;
        public bool Flipped;
        public bool WantsToJump;
    }

    public static InputResult HandlePlayerInput(
        KeyboardState keyboardState,
        float currentXVelocity,
        bool isOnGround,
        float friction,
        float defaultSpeed = 1.0f,
        float sprintSpeed = 4.5f)
    {
        InputResult result = new();

        // Update movement speed based on shift key
        if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
        {
            result.MovementSpeed = sprintSpeed;
            result.AnimationSpeed = 0.1f;
        }
        else
        {
            result.MovementSpeed = defaultSpeed;
            result.AnimationSpeed = 0.15f;
        }

        // Calculate proposed horizontal movement
        if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
        {
            result.ProposedXVelocity = -result.MovementSpeed;
            result.Flipped = true;
        }
        else if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
        {
            result.ProposedXVelocity = result.MovementSpeed;
            result.Flipped = false;
        }
        else if (isOnGround)
        {
            result.ProposedXVelocity = currentXVelocity * friction;
            if (Math.Abs(result.ProposedXVelocity) < 0.1f)
            {
                result.ProposedXVelocity = 0;
            }
        }

        // Check for jump input
        result.WantsToJump = keyboardState.IsKeyDown(Keys.Space);

        return result;
    }
}
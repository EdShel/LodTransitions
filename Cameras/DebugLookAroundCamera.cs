using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

namespace LodTransitions.Cameras
{
    public class DebugLookAroundCamera
    {
        private MyGame game;
        private Vector3 position;
        private Vector3 rotation;

        private MouseState? prevMouseState;

        public DebugLookAroundCamera(MyGame game, Vector3 position, Vector3 rotation)
        {
            this.game = game;
            this.position = position;
            this.rotation = rotation;
        }

        public Matrix Transform { get; private set; }
        public float MouseSensitivity { get; set; } = 0.001f;
        public float MovementSpeedMetersPerSecond { get; set; } = 10f;

        public void Update(TimeSpan deltaTime)
        {
            var keyboardState = Keyboard.GetState();

            bool mouseLockEnabled = keyboardState.IsKeyDown(Keys.LeftControl);
            this.game.IsMouseVisible = !mouseLockEnabled;
            if (mouseLockEnabled)
            {

                var mouseState = Mouse.GetState();
                if (this.prevMouseState != null)
                {
                    var (windowX, windowY) = this.game.Window.ClientBounds.Size;

                    int yawDelta = mouseState.X - windowX / 2;
                    int pitchDelta = mouseState.Y - windowY / 2;
                    this.rotation.Z += yawDelta * this.MouseSensitivity;
                    this.rotation.Y += pitchDelta * this.MouseSensitivity;
                    this.rotation.X = MathHelper.Clamp(this.rotation.X, -MathHelper.Pi, MathHelper.Pi);

                    Mouse.SetPosition(windowX / 2, windowY / 2);
                }
                this.prevMouseState = mouseState;
            }
            else
            {
                this.prevMouseState = null;
            }

            var rotationMatrix = Matrix.CreateFromYawPitchRoll(this.rotation.Z, this.rotation.Y, this.rotation.X);

            Vector2 moveDirection = GetDirection(keyboardState);
            Vector3 transformBasisForward = rotationMatrix.Forward;
            Vector3 transformBasisRight = rotationMatrix.Left;

            Debug.WriteLine(transformBasisForward);

            float dt = (float)deltaTime.TotalSeconds;
            this.position +=
                moveDirection.Y * transformBasisForward * this.MovementSpeedMetersPerSecond * dt
                + moveDirection.X * transformBasisRight * this.MovementSpeedMetersPerSecond * dt;

            this.Transform = Matrix.CreateTranslation(this.position)
                * rotationMatrix;
        }

        private static Vector2 GetDirection(in KeyboardState keyboardState)
        {
            bool up = keyboardState.IsKeyDown(Keys.W);
            bool down = keyboardState.IsKeyDown(Keys.S);
            bool left = keyboardState.IsKeyDown(Keys.A);
            bool right = keyboardState.IsKeyDown(Keys.D);
            Vector2 result = new Vector2();
            if (left && !right)
            {
                result.X = -1f;
            }
            else if (!left && right)
            {
                result.X = 1f;
            }
            if (up && !down)
            {
                result.Y = -1f;
            }
            else if (!up && down)
            {
                result.Y = 1f;
            }
            return result;
        }
    }
}

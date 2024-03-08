using Microsoft.Xna.Framework;

namespace LodTransitions.Rendering.Cameras
{
    public class LookAtTargetCameraViewConfig : ICameraViewConfig
    {
        private Vector3 position;
        private Vector3 lookAt;
        private Vector3 up;

        private Matrix? viewMatrixCache;

        public Vector3 Position
        {
            get => this.position;
            set
            {
                this.position = value;
                this.viewMatrixCache = null;
            }
        }

        public Vector3 LookAt
        {
            get => this.lookAt;
            set
            {
                this.lookAt = value;
                this.viewMatrixCache = null;
            }
        }

        public Vector3 Up
        {
            get => this.up;
            set
            {
                this.up = value;
                this.viewMatrixCache = null;
            }
        }

        public Matrix Matrix
        {
            get
            {
                if (this.viewMatrixCache == null)
                {
                    this.viewMatrixCache = Matrix.CreateLookAt(this.position, this.lookAt, this.up);
                }
                return this.viewMatrixCache.Value;
            }
        }
    }
}

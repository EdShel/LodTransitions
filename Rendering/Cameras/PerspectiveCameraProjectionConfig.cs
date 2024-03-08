using Microsoft.Xna.Framework;

namespace LodTransitions.Rendering.Cameras
{
    public class PerspectiveCameraProjectionConfig : ICameraProjectionConfig
    {
        private float fov;
        private float aspectRatio;
        private float nearPlane;
        private float farPlane;

        private Matrix? projectionMatrixCache;

        public float Fov
        {
            get => this.fov;
            set
            {
                this.fov = value;
                this.projectionMatrixCache = null;
            }
        }

        public float AspectRatio
        {
            get => this.aspectRatio;
            set
            {
                this.aspectRatio = value;
                this.projectionMatrixCache = null;
            }
        }

        public float NearPlane
        {
            get => this.nearPlane;
            set
            {
                this.nearPlane = value;
                this.projectionMatrixCache = null;
            }
        }

        public float FarPlane
        {
            get => this.farPlane;
            set
            {
                this.farPlane = value;
                this.projectionMatrixCache = null;
            }
        }

        public Matrix Matrix
        {
            get
            {
                if (this.projectionMatrixCache == null)
                {
                    this.projectionMatrixCache = Matrix.CreatePerspectiveFieldOfView(this.fov, this.aspectRatio, this.nearPlane, this.farPlane);
                }
                return this.projectionMatrixCache.Value;
            }
        }

    }
}

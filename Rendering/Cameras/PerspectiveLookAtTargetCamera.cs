namespace LodTransitions.Rendering.Cameras
{
    public record PerspectiveLookAtTargetCamera(
        LookAtTargetCameraViewConfig ViewConfig,
        PerspectiveCameraProjectionConfig ProjectionConfig
    ) : ICamera
    {
        public ICameraViewConfig View => this.ViewConfig;

        public ICameraProjectionConfig Projection => this.ProjectionConfig;
    }
}

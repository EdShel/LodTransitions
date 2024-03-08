namespace LodTransitions.Rendering.Cameras
{
    public interface ICamera
    {
        ICameraViewConfig View { get; }
        ICameraProjectionConfig Projection { get; }
    }
}

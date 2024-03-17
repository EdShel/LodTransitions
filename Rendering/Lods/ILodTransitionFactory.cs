namespace LodTransitions.Rendering.Lods
{
    public interface ILodTransitionFactory
    {
        ILodTransition CreateTransition(LodLevel start, LodLevel end, float progress = 0.0f);
    }
}

using VoxelEngine.Cameras;
using VoxelEngine.VoxelObjects.Components;

namespace NewWorldVoxel
{
    public class CameraAnchor : ITrackable
    {
        public TransformComponent Transform { get; set; }

        public CameraAnchor()
        {
            Transform = new TransformComponent("transform");
        }
    }
}

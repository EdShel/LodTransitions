using System;
using System.Collections.Generic;

namespace LodTransitions.Rendering.Lods
{
    public class GeomorphedMesh : IDisposable
    {
        public IReadOnlyList<GeomorphedMeshPart> Parts { get; set; }

        public GeomorphedMesh(IReadOnlyList<GeomorphedMeshPart> parts)
        {
            this.Parts = parts;
        }

        public void Dispose()
        {
            foreach (var part in Parts)
            {
                part.Dispose();
            }
        }
    }
}
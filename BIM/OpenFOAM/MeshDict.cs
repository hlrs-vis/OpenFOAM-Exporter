using System;
using System.Collections.Generic;

namespace BIM.OpenFOAMExport.OpenFOAM
{
    /// <summary>
    /// MeshDict is in use for meshing with cfMesh.
    /// </summary>
    public class MeshDict : FOAMDict
    {
        public MeshDict(Version version, string path, Dictionary<string, object> attributes, SaveFormat format, Settings settings)
            : base("meshDict", "dictionary", version, path, attributes, format, settings)
        {

        }

        public override void InitAttributes()
        {
            throw new NotImplementedException();
        }
    }
}

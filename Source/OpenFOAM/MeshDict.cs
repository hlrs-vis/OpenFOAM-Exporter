using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIM.OpenFoamExport.OpenFOAM
{
    /// <summary>
    /// MeshDict is in use for meshing with cfMesh.
    /// </summary>
    public class MeshDict : FoamDict
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

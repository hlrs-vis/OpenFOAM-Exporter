using System.Collections.Generic;

namespace BIM.OpenFoamExport.OpenFOAM
{
    /// <summary>
    /// Interface for OpenFOAM-Dictionaries.
    /// </summary>
    public abstract class FoamDict
    {
        /// <summary>
        /// Name of the dictionary
        /// </summary>
        protected string name = string.Empty;

        /// <summary>
        /// Class of the dictionary
        /// </summary>
        protected string _class = string.Empty;

        /// <summary>
        /// Settings-object
        /// </summary>
        protected Settings m_Settings;

        /// <summary>
        /// FoamFile-Object
        /// </summary>
        private FoamFile foamFile;

        /// <summary>
        /// Getter for the FoamFile.
        /// </summary>
        public FoamFile FoamFile { get => foamFile;}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="_name">Name of the dictionary.</param>
        /// <param name="m_class">Class of the dicitonary.</param>
        /// <param name="version">Version-object.</param>
        /// <param name="path">Path to this File.</param>
        /// <param name="attributes">Additional attributes.</param>
        /// <param name="format">Ascii or Binary.</param>
        public FoamDict(string _name, string m_class, Version version, string path, Dictionary<string, object> attributes, SaveFormat format)
        {
            name = _name;
            _class = m_class;

            if (format == SaveFormat.ascii)
            {
                foamFile = new FoamFileAsAscII(name, version, path, _class, attributes, format);
            }
            else if (format == SaveFormat.binary)
            {
                foamFile = new FoamFileAsBinary(name, version, path, _class, attributes, format);
            }
        }

        /// <summary>
        /// Interface for initializing attributes.
        /// </summary>
        public abstract void InitAttributes();

        /// <summary>
        /// Add the attributes to the Foamfile.
        /// </summary>
        public virtual void Init()
        {
            FoamFile.WriteFile();
        }
    }
}

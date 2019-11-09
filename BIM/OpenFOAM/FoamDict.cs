﻿using System.Collections.Generic;

namespace BIM.OpenFOAMExport.OpenFOAM
{
    /// <summary>
    /// Represents general OpenFOAM-File.
    /// </summary>
    public abstract class FOAMDict
    {
        ///// <summary>
        ///// Name of the file
        ///// </summary>
        //protected string m_Name = string.Empty;

        ///// <summary>
        ///// Class of the file
        ///// </summary>
        //protected string m_Class = string.Empty;


        ///// <summary>
        ///// ParentDictionary of the simulation dictionary in settings.
        ///// </summary>
        //protected Dictionary<string, object> m_ParentDictionary;

        /// <summary>
        /// Dictionary in settings which contains all attributes for this file.
        /// </summary>
        protected Dictionary<string, object> m_DictFile;

        /// <summary>
        /// FoamFile-Object
        /// </summary>
        private readonly FOAMFile m_FoamFile;

        /// <summary>
        /// Getter for the FoamFile.
        /// </summary>
        public FOAMFile FoamFile { get => m_FoamFile;}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="_name">Name of the dictionary.</param>
        /// <param name="_class">Class of the dicitonary.</param>
        /// <param name="version">Version-object.</param>
        /// <param name="path">Path to this File.</param>
        /// <param name="attributes">Additional attributes.</param>
        /// <param name="format">Ascii or Binary.</param>
        /// <param name="settings">Settings-object</param>
        public FOAMDict(string _name, string _class, Version version, string path, Dictionary<string, object> attributes, SaveFormat format)
        {
            //m_Name = _name;
            //m_Class = _class;

            if (format == SaveFormat.ascii)
            {
                m_FoamFile = new FoamFileAsAscII(/*m_Name*/_name, version, path, /*m_Class*/_class, attributes, format);
            }
            else if (format == SaveFormat.binary)
            {
                m_FoamFile = new FoamFileAsBinary(/*m_Name*/_name, version, path, /*m_Class*/_class, attributes, format);
            }
            Dictionary<string, object> m_ParentDictionary = BIM.OpenFOAMExport.Exporter.Instance.settings.SimulationDefault[FoamFile.Location.Trim('"')] as Dictionary<string, object>;
            m_DictFile = m_ParentDictionary[_name] as Dictionary<string, object>;
        }

        /// <summary>
        /// Interface for initializing attributes.
        /// </summary>
        public virtual void InitAttributes()
        {
            foreach (var obj in m_DictFile)
            {
                if(obj.Value == null)
                {
                    continue;
                }
                FoamFile.Attributes.Add(obj.Key, obj.Value);
            }
        }

        /// <summary>
        /// Add the attributes to the Foamfile.
        /// </summary>
        public virtual void Init()
        {
            FoamFile.WriteFile();
        }
    }
}

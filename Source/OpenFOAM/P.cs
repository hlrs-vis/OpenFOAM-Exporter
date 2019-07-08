﻿using System;
using System.Collections.Generic;

namespace BIM.OpenFoamExport.OpenFOAM
{
    /// <summary>
    /// This class represents the pressure for a simulation in OpenFOAM.
    /// </summary>
    public class P : FoamParameter
    {
        /// <summary>
        /// InternalField entry
        /// </summary>
        private InternalField<double> m_InternalField;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="version">Version-Object</param>
        /// <param name="path">Path to this file.</param>
        /// <param name="attributes">Additional attributs.</param>
        /// <param name="format">Format of this file.</param>
        /// <param name="settings">Settings-object</param>
        /// <param name="_wallName">Name of the patch wall.</param>
        /// <param name="_InletNames">Patchnames of the inlets as string-array.</param>
        /// <param name="_OutletNames">Patchnames of the outlets as string-array.</param>
        public P(Version version, string path, Dictionary<string, object> attributes, SaveFormat format, Settings settings, string _wallName,
            List<string> _InletNames, List<string> _OutletNames)
            : base(version, path, attributes, format, settings, "p", "volScalarField",  _wallName, _InletNames, _OutletNames)
        {

        }

        /// <summary>
        /// Initialize Attributes.
        /// </summary>
        public override void InitAttributes()
        {
            m_Dimensions = new int[] { 0, 2, -2, 0, 0, 0, 0 };
            m_InternalField.Value = m_Settings.InternalFieldP;
            m_BoundaryField.Add(m_WallName, m_Settings.WallP.Attributes);

            foreach (string s in m_OutletNames)
            {
                m_BoundaryField.Add(s, m_Settings.OutletP.Attributes);
            }
            foreach (string s in m_InletNames)
            {
                m_BoundaryField.Add(s, m_Settings.InletP.Attributes);
            }

            FoamFile.Attributes.Add("dimensions", m_Dimensions);
            string internalField = m_Uniform + " " + m_InternalField.Value.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US").NumberFormat);
            FoamFile.Attributes.Add("internalField", internalField);
            FoamFile.Attributes.Add("boundaryField", m_BoundaryField);
        }
    }
}
using System.Collections.Generic;

namespace BIM.OpenFoamExport.OpenFOAM
{
    /// <summary>
    /// The ControlDict-Class represents the controlDict in the OpenFOAM-System folder.
    /// </summary>
    public class ControlDict : FoamDict
    {
        /// <summary>
        /// additional functions
        /// </summary>
        private Dictionary<string, object> m_Functions;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="version">Version-object.</param>
        /// <param name="path">Path to this file.</param>
        /// <param name="attributes">Additional attributes.</param>
        /// <param name="format">Ascii or Binary</param>
        /// <param name="settings">Settings-object</param>
        /// <param name="_functions">Additional functions as string</param>
        public ControlDict(Version version, string path, Dictionary<string, object> attributes, SaveFormat format, Settings settings, string _functions)
            : base("controlDict" , "dictionary", version, path, attributes, format)
        {
            m_Settings = settings;
            m_Functions = new Dictionary<string, object>();

            InitAttributes();
        }

        /// <summary>
        /// Initialize attributes of this file.
        /// </summary>
        public override void InitAttributes()
        {
            Dictionary<string, object> system = m_Settings.SimulationDefault["System"] as Dictionary<string, object>;
            Dictionary<string, object> control = system["ControlDictionary"] as Dictionary<string, object>;

            foreach(var obj in control)
            {
                FoamFile.Attributes.Add(obj.Key, obj.Value);
            }

            FoamFile.Attributes.Add("functions", m_Functions);

            //FoamFile.Attributes.Add("startFrom", control["StartFrom"]/*m_Settings._StartFrom*/);
            //FoamFile.Attributes.Add("startTime", m_Settings.StartTime);
            //FoamFile.Attributes.Add("stopAt", control["StopAt"] /*m_Settings._StopAt*/);
            //FoamFile.Attributes.Add("endTime", control["EndTime"]/*m_Settings.EndTime*/);
            //FoamFile.Attributes.Add("deltaT", control["DeltaT"]/*m_Settings.DeltaT*/);
            //FoamFile.Attributes.Add("writeControl", /*control["WriteControl"]*/m_Settings._WriteControl);
            //FoamFile.Attributes.Add("writeInterval", m_Settings.WriteInterval);
            //FoamFile.Attributes.Add("purgeWrite", m_Settings.PurgeWrite);
            //FoamFile.Attributes.Add("writeFormat", m_Settings._WriteFormat);
            //FoamFile.Attributes.Add("writePrecision", m_Settings.WritePrecision);
            //FoamFile.Attributes.Add("writeCompression", m_Settings.WriteCompression);
            //FoamFile.Attributes.Add("timeFormat", m_Settings._TimeFormat);
            //FoamFile.Attributes.Add("timePrecision", m_Settings.TimePrecision);
            //FoamFile.Attributes.Add("runTimeModifiable", m_Settings.RunTimeModifiable);
            //FoamFile.Attributes.Add("functions", m_Functions);
        }
        
        /// <summary>
        /// Initializes functions dictionary.
        /// </summary>
        private void InitFunction()
        {
            //TO-DO: Implement later.
        }
    }
}

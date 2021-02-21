//GNU License ENTRY FROM STL-EXPORTER
//Source Code: https://github.com/Autodesk/revit-stl-extension
// STL exporter library: this library works with Autodesk(R) Revit(R) to export an STL file containing model geometry.
// Copyright (C) 2013  Autodesk, Inc.
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//
// Modified version
// Author: Marko Djuric

using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using utils;

using Autodesk.Revit;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM.OpenFOAMExport
{
    /// <summary>
    /// Class OpenFOAMSimulateCommand is the entry of the AddIn program.
    /// </summary>
    [Regeneration(RegenerationOption.Manual)]
    [Transaction(TransactionMode.Manual)]
    
    public class OpenFOAMSimulateCommand : IExternalCommand
    {
        /// <summary>
        /// The application object for the active instance of Autodesk Revit.
        /// </summary>
        private UIApplication m_Revit;

        /// <summary>
        /// Implement the member of IExternalCommand Execute.
        /// </summary>
        /// <param name="commandData">
        /// The application object for the active instance of Autodesk Revit.
        /// </param>
        /// <param name="message">
        /// A message that can be set by the external command and displayed in case of error.
        /// </param>
        /// <param name="elements">
        /// A set of elements that can be displayed if an error occurs.
        /// </param>
        /// <returns>
        /// A value that signifies if yout command was successful, failed or the user wishes to cancel.
        /// </returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //for repeating click-events

            m_Revit = commandData.Application;

            BIM.OpenFOAMExport.Exporter.Instance.settings.setDocument(m_Revit);
            string fileName = "wallSTL.stl";
            // save Revit document's triangular data in a temporary file, generate openFOAM-casefolder and start simulation

            Directory.CreateDirectory(BIM.OpenFOAMExport.Exporter.Instance.settings.localCaseFolder);
            Directory.CreateDirectory(BIM.OpenFOAMExport.Exporter.Instance.settings.localCaseFolder + "\\constant");
            Directory.CreateDirectory(BIM.OpenFOAMExport.Exporter.Instance.settings.localCaseFolder + "\\constant\\triSurface");

            DataGenerator Generator = new DataGenerator(m_Revit.Application, m_Revit.ActiveUIDocument.Document);
            DataGenerator.GeneratorStatus succeed = Generator.SaveSTLFile(fileName);

            return Result.Succeeded;
        }

    }
}

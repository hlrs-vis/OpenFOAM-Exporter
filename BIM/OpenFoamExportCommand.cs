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
    /// Class OpenFOAMExportCommand is the entry of the AddIn program.
    /// </summary>
    [Regeneration(RegenerationOption.Manual)]
    [Transaction(TransactionMode.Manual)]
    
    public class OpenFOAMExportCommand : IExternalCommand
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
            var iterator = System.Windows.Forms.Application.OpenForms.GetEnumerator();
            while(iterator.MoveNext())
            {
                System.Windows.Forms.Form form = iterator.Current as System.Windows.Forms.Form;
                if(form is OpenFOAMExportForm)
                {
                    return Result.Succeeded;
                }
            }
            m_Revit = commandData.Application;
            Result result = StartOpenFOAMExportForm();
            return result;

            ///pop up the form //////FROM STL-EXPORTER/////////
            //using (OpenFOAMExportForm exportForm = new OpenFOAMExportForm(m_Revit))
            //{
            //    //    Attempt to make non modal window  NOT INCLUDED IN STL EXPORTER //asynchronous call of showDialog()
            //    //    var task = Task.Run(async () => await exportForm.ShowDialogAsync());
            //    if (DialogResult.Cancel == /*task.Result*/exportForm.ShowDialog())
            //    {
            //        return Result.Cancelled;
            //    }
            //}

            //return result.succeeded;
        }

        /// <summary>
        /// Generates OpenFOAMExportForm and shows it.
        /// </summary>
        private Result StartOpenFOAMExportForm()
        {
            if (m_Revit == null)
                return Result.Failed;

            using (OpenFOAMExportForm exportForm = new OpenFOAMExportForm(m_Revit))
            {
                System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
                System.Windows.Forms.Application.EnableVisualStyles();
                //exportForm.TopMost = true;

                //Start modal form with with responsive messageloop.
                System.Windows.Forms.Application.Run(exportForm);

                if (exportForm.DialogResult == DialogResult.Cancel)
                {
                    return Result.Cancelled;
                }
            }

            return Result.Succeeded;
        }
    }
}

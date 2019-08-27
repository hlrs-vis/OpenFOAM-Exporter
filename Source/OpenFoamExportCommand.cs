//
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

using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using AsyncShowDialog;

using Autodesk.Revit;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows;

namespace BIM.OpenFOAMExport
{
    /// <summary>
    /// Class STLExportCommand is the entry of the AddIn program and contains the method to save STL file.
    /// </summary>
    [Regeneration(RegenerationOption.Manual)]
    [Transaction(TransactionMode.Manual)]
    public class OpenFOAMExportCommand : IExternalCommand
    {
        /// <summary>
        /// The application object for the active instance of Autodesk Revit.
        /// </summary>
        private UIApplication m_Revit;

        private OpenFOAMExportForm m_FOAMExportForm;

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
            //m_Revit = commandData.Application;

            ///pop up the STL export form
            //using (OpenFOAMExportForm exportForm = new OpenFOAMExportForm(m_Revit))
            //{
            //    //TO-DO: Implement with .Show()
            //    //asynchronous call of showDialog()
            //    //var task = Task.Run(async () => await exportForm.ShowDialogAsync());
            //    if (DialogResult.Cancel == task.Result)
            //    {
            //        return Result.Cancelled;
            //    }
            //}

            if(m_FOAMExportForm == null)
            {
                m_Revit = commandData.Application;
                m_FOAMExportForm = new OpenFOAMExportForm(m_Revit);
                m_FOAMExportForm.TopMost = true;
                m_FOAMExportForm.FormClosed += new FormClosedEventHandler(OpenFOAMExportForm_FormClosed);
                m_FOAMExportForm.Show();
            }

            return Result.Succeeded;
        }

        private void OpenFOAMExportForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_FOAMExportForm = null;
        }
    }
}

namespace AsyncShowDialog
{
    public static class ShowDialogAsyncExt
    {
        /// <summary>
        /// ExtensionMethod for asynchronous use of showDialog().
        /// Source:https://stackoverflow.com/questions/33406939/async-showdialog/43420090#43420090
        /// </summary>
        /// <param name="this">Windows form object.</param>
        /// <returns>DialogResult in Task.</returns>
        public static async Task<DialogResult> ShowDialogAsync(this System.Windows.Forms.Form @this)
        {
            await Task.Yield();
            if (@this.IsDisposed)
                return DialogResult.OK;
            return @this.ShowDialog();
        }
    }
}

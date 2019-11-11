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
using Autodesk.Revit.UI;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace BIM.OpenFOAMExport
{
    public class OpenFOAMExporterUI : IExternalApplication
    {
        // Fields
        private static string AddInPath;

        // Methods
        static OpenFOAMExporterUI()
        {
            AddInPath = typeof(OpenFOAMExporterUI).Assembly.Location;
        }

        Result IExternalApplication.OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        Result IExternalApplication.OnStartup(UIControlledApplication application)
        {
            try
            {
                string str = "OpenFOAM Exporter";
                RibbonPanel panel = application.CreateRibbonPanel(str);
                string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                PushButtonData data = new PushButtonData("Exporter GUI", "Exporter GUI", directoryName + @"\OpenFOAMExport.dll", "BIM.OpenFOAMExport.OpenFOAMExportCommand");
                PushButtonData dataDirect = new PushButtonData("Direct Export", "Direct Export", directoryName + @"\OpenFOAMExport.dll", "BIM.OpenFOAMExport.OpenFOAMExportButtonCommand");
                PushButton buttonDirect = panel.AddItem(dataDirect) as PushButton;
                PushButton button = panel.AddItem(data) as PushButton;

                //button.LargeImage = LoadPNGImageFromResource(directoryName + @"../../share/covise/icons/logo_64.png");
                //button.LargeImage = LoadPNGImageFromResource("BIM.Properties.Resources.logo_64");
                using (Stream xstr = new MemoryStream())
                {
                    try
                    {
                        BIM.Properties.Resources.logo_64.Save(xstr, System.Drawing.Imaging.ImageFormat.Bmp);
                        xstr.Seek(0, SeekOrigin.Begin);
                        BitmapDecoder bdc = new BmpBitmapDecoder(xstr, BitmapCreateOptions.IgnoreImageCache/*.PreservePixelFormat*/, BitmapCacheOption.OnLoad);
                        button.LargeImage = bdc.Frames[0];
                        buttonDirect.LargeImage = bdc.Frames[0];
                    }
                    catch (Exception)
                    {
                        //button without image
                        button.LargeImage = null;
                    }
                }

                button.ToolTip = "The OpenFOAM Exporter for Revit is designed to produce a stereolithography file (STL) of your building model and a OpenFOAM-Config.";
                button.LongDescription = "The OpenFOAM Exporter for the Autodesk Revit Platform is a project designed to create an STL file from a 3D building information model for OpenFOAM with a Config-Folder that includes the boundary conditions for airflow simulation.";
                
                buttonDirect.ToolTip = button.ToolTip;
                buttonDirect.LongDescription = button.LongDescription;

                //STL-Exporter only
                ContextualHelp help = new ContextualHelp(ContextualHelpType.ChmFile, directoryName + @"\Resources\ADSKSTLExporterHelp.htm");
                button.SetContextualHelp(help);
                buttonDirect.SetContextualHelp(help);
                return Result.Succeeded;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "OpenFOAM Exporter for Revit");
                return Result.Failed;
            }
        }


        //private static System.Windows.Media.ImageSource LoadPNGImageFromResource(string imageResourceName)
        //  {
        //      string[] names =  Assembly.GetExecutingAssembly().GetManifestResourceNames();
        //      int i = 0;
        //      foreach(var name in names)
        //      {

        //          MessageBox.Show(names[i],name);
        //          i++;
        //      }
        //      PngBitmapDecoder decoder = new PngBitmapDecoder(Assembly.GetExecutingAssembly().GetManifestResourceStream(imageResourceName), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
        //   return decoder.Frames[0];
        //}
    }
}

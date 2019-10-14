﻿//GNU License ENTRY FROM STL-EXPORTER
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
            PushButtonData data = new PushButtonData("OpenFOAM Exporter for Revit", "OpenFOAM Exporter for Revit", directoryName + @"\OpenFOAMExport.dll", "BIM.OpenFOAMExport.OpenFOAMExportCommand");
            PushButton button = panel.AddItem(data) as PushButton;
            button.LargeImage = LoadPNGImageFromResource("BIM.OpenFOAMExport.Resources.logo_64.png");
            button.ToolTip = "The OpenFOAM Exporter for Revit is designed to produce a stereolithography file (STL) of your building model and a OpenFOAM-Config.";
            button.LongDescription = "The OpenFOAM Exporter for the Autodesk Revit Platform is a project designed to create an STL file from a 3D building information model for OpenFOAM with a Config-File that includes the boundary conditions for airflow simulation.";
            ContextualHelp help = new ContextualHelp(ContextualHelpType.ChmFile, directoryName + @"\Resources\ADSKSTLExporterHelp.htm");
            button.SetContextualHelp(help);
            return Result.Succeeded;
         }
         catch (Exception exception)
         {
            MessageBox.Show(exception.ToString(), "OpenFOAM Exporter for Revit");
            return Result.Failed;
         }
      }

      private static System.Windows.Media.ImageSource LoadPNGImageFromResource(string imageResourceName)
      {
         PngBitmapDecoder decoder = new PngBitmapDecoder(Assembly.GetExecutingAssembly().GetManifestResourceStream(imageResourceName), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
         return decoder.Frames[0];
      }
   }
}

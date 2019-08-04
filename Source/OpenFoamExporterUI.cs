using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

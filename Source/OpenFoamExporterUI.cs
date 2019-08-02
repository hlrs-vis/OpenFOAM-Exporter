using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace BIM.OpenFoamExport
{
   public class OpenFoamExporterUI : IExternalApplication
   {
      // Fields
      private static string AddInPath;

      // Methods
      static OpenFoamExporterUI()
      {
         AddInPath = typeof(OpenFoamExporterUI).Assembly.Location;
      }

      Result IExternalApplication.OnShutdown(UIControlledApplication application)
      {
         return Result.Succeeded;
      }

      Result IExternalApplication.OnStartup(UIControlledApplication application)
      {
         try
         {
            string str = "OpenFoam Exporter";
            RibbonPanel panel = application.CreateRibbonPanel(str);
            string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            PushButtonData data = new PushButtonData("OpenFoam Exporter for Revit", "OpenFoam Exporter for Revit", directoryName + @"\OpenFoamExport.dll", "BIM.OpenFoamExport.OpenFoamExportCommand");
            PushButton button = panel.AddItem(data) as PushButton;
            button.LargeImage = LoadPNGImageFromResource("BIM.OpenFoamExport.Resources.openfoam_32.png");
            button.ToolTip = "The OpenFoam Exporter for Revit is designed to produce a stereolithography file (STL) of your building model and a OpenFoam-Config.";
            button.LongDescription = "The OpenFoam Exporter for the Autodesk Revit Platform is a project designed to create an STL file from a 3D building information model for OpenFoam with a Config-File that includes the boundary conditions for airflow simulation.";
            ContextualHelp help = new ContextualHelp(ContextualHelpType.ChmFile, directoryName + @"\Resources\ADSKSTLExporterHelp.htm");
            button.SetContextualHelp(help);
            return Result.Succeeded;
         }
         catch (Exception exception)
         {
            MessageBox.Show(exception.ToString(), "OpenFoam Exporter for Revit");
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

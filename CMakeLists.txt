USING(Revit)
PROJECT(RevitPlugin CSharp)
include(CSharpUtilities)


add_library(OpenFOAMExporter SHARED
BIM/AssemblyInfo.cs
BIM/DataGenerator.cs
BIM/OpenFoamDialogManager.cs
BIM/OpenFoamExportCommand.cs
BIM/OpenFoamSimulateCommand.cs
BIM/OpenFoamExporterUI.cs
BIM/OpenFOAMExportResource.Designer.cs
BIM/OpenFOAMExportResource.resx
BIM/SaveData.cs
BIM/Settings.cs
BIM/OpenFOAMExportCancelForm.cs
BIM/OpenFOAMExportCancelForm.Designer.cs
BIM/OpenFOAMExportCancelForm.resx
BIM/OpenFOAMExportForm.cs
BIM/OpenFOAMExportForm.Designer.cs
BIM/OpenFOAMExportForm.resx
BIM/Properties/Resources.Designer.cs
BIM/Properties/Resources.resx
BIM/OpenFOAMUI/OpenFOAMDropDownTreeNode.cs
BIM/OpenFOAMUI/OpenFOAMTextBoxForm.cs
BIM/OpenFOAMUI/OpenFOAMTextBoxForm.Designer.cs
BIM/OpenFOAMUI/OpenFOAMTextBoxTreeNode.cs
BIM/OpenFOAMUI/OpenFOAMTreeNode.cs
BIM/OpenFOAMUI/OpenFOAMTreeView.cs
BIM/OpenFOAM/Alphat.cs
BIM/OpenFOAM/BlockMeshDict.cs
BIM/OpenFOAM/ControlDict.cs
BIM/OpenFOAM/DecomposeParDict.cs
BIM/OpenFOAM/Epsilon.cs
BIM/OpenFOAM/FoamDict.cs
BIM/OpenFOAM/FoamFile.cs
BIM/OpenFOAM/FoamParameter.cs
BIM/OpenFOAM/FvSchemes.cs
BIM/OpenFOAM/FvSolution.cs
BIM/OpenFOAM/G.cs
BIM/OpenFOAM/K.cs
BIM/OpenFOAM/MeshDict.cs
BIM/OpenFOAM/Nut.cs
BIM/OpenFOAM/OpenFOAMCalculator.cs
BIM/OpenFOAM/P.cs
BIM/OpenFOAM/P_rgh.cs
BIM/OpenFOAM/RunManager.cs
BIM/OpenFOAM/SnappyHexMeshDict.cs
BIM/OpenFOAM/SurfaceFeatureExtract.cs
BIM/OpenFOAM/T.cs
BIM/OpenFOAM/TransportProperties.cs
BIM/OpenFOAM/TurbulenceProperties.cs
BIM/OpenFOAM/U.cs
BIM/OpenFOAM/Version.cs
BIM/Resources/ADSKSTLExporterHelp.htm
BIM/Resources/DropDownTreeView.dll
BIM/Resources/logo_64.png
BIM/Resources/OpenFoamExport.addin
BIM/Resources/PackageContents.xml
BIM/Resources/wind_32.png
BIM/Resources/wind_32_reworked.png
BIM/Resources/wind_64.ico
BIM/Resources/wind_64.png
)

set_property(SOURCE BIM/Resources/ADSKSTLExporterHelp.htm PROPERTY VS_TOOL_OVERRIDE "Resource")
set_property(SOURCE BIM/Resources/DropDownTreeView.dll PROPERTY VS_TOOL_OVERRIDE "Resource")
set_property(SOURCE BIM/Resources/logo_64.png PROPERTY VS_TOOL_OVERRIDE "Resource")
set_property(SOURCE BIM/Resources/OpenFoamExport.addin PROPERTY VS_TOOL_OVERRIDE "Resource")
set_property(SOURCE BIM/Resources/PackageContents.xml PROPERTY VS_TOOL_OVERRIDE "Resource")
set_property(SOURCE BIM/Resources/wind_32.png PROPERTY VS_TOOL_OVERRIDE "Resource")
set_property(SOURCE BIM/Resources/wind_32_reworked.png PROPERTY VS_TOOL_OVERRIDE "Resource")
set_property(SOURCE BIM/Resources/wind_64.ico PROPERTY VS_TOOL_OVERRIDE "Resource")
set_property(SOURCE BIM/Resources/wind_64.png PROPERTY VS_TOOL_OVERRIDE "Resource")

# Set the C# language version (defaults to 3.0).
set(CMAKE_CSharp_FLAGS "/langversion:latest")
set_target_properties(OpenFOAMExporter PROPERTIES FOLDER "Plugins/RevitPlugins")

SET(BINLIB_SUFFIX "lib")
SET(MYPATH_POSTFIX )
set_target_properties(OpenFOAMExporter PROPERTIES OUTPUT_NAME OpenFOAMExporter$<$<CONFIG:Debug>:_d>)

IF(CMAKE_CONFIGURATION_TYPES)
      # generator supports configuration types
      FOREACH(conf_type ${CMAKE_CONFIGURATION_TYPES})
        STRING(TOUPPER "${conf_type}" upper_conf_type_str)
        IF(upper_conf_type_str STREQUAL "DEBUG")
            SET_TARGET_PROPERTIES(OpenFOAMExporter PROPERTIES ARCHIVE_OUTPUT_DIRECTORY_${upper_conf_type_str} "${COVISE_DESTDIR}/${DBG_ARCHSUFFIX}/${BINLIB_SUFFIX}${MYPATH_POSTFIX}")
            SET_TARGET_PROPERTIES(OpenFOAMExporter PROPERTIES LIBRARY_OUTPUT_DIRECTORY_${upper_conf_type_str} "${COVISE_DESTDIR}/${DBG_ARCHSUFFIX}/${BINLIB_SUFFIX}${MYPATH_POSTFIX}")
            SET_TARGET_PROPERTIES(OpenFOAMExporter PROPERTIES RUNTIME_OUTPUT_DIRECTORY_${upper_conf_type_str} "${COVISE_DESTDIR}/${DBG_ARCHSUFFIX}/${BINLIB_SUFFIX}${MYPATH_POSTFIX}")
        ELSE(upper_conf_type_str STREQUAL "DEBUG")
            SET_TARGET_PROPERTIES(OpenFOAMExporter PROPERTIES ARCHIVE_OUTPUT_DIRECTORY_${upper_conf_type_str} "${COVISE_DESTDIR}/${DBG_ARCHSUFFIX}opt/${BINLIB_SUFFIX}${MYPATH_POSTFIX}")
            SET_TARGET_PROPERTIES(OpenFOAMExporter PROPERTIES LIBRARY_OUTPUT_DIRECTORY_${upper_conf_type_str} "${COVISE_DESTDIR}/${DBG_ARCHSUFFIX}opt/${BINLIB_SUFFIX}${MYPATH_POSTFIX}")
            SET_TARGET_PROPERTIES(OpenFOAMExporter PROPERTIES RUNTIME_OUTPUT_DIRECTORY_${upper_conf_type_str} "${COVISE_DESTDIR}/${DBG_ARCHSUFFIX}opt/${BINLIB_SUFFIX}${MYPATH_POSTFIX}")
        ENDIF(upper_conf_type_str STREQUAL "DEBUG")
      ENDFOREACH(conf_type)
    ELSE(CMAKE_CONFIGURATION_TYPES)
      # no configuration types - probably makefile generator
      STRING(TOUPPER "${CMAKE_BUILD_TYPE}" upper_build_type_str)
      SET_TARGET_PROPERTIES(OpenFOAMExporter PROPERTIES ARCHIVE_OUTPUT_DIRECTORY_${upper_build_type_str} "${COVISE_DESTDIR}/${ARCHSUFFIX}/${BINLIB_SUFFIX}${MYPATH_POSTFIX}")
      SET_TARGET_PROPERTIES(OpenFOAMExporter PROPERTIES LIBRARY_OUTPUT_DIRECTORY_${upper_build_type_str} "${COVISE_DESTDIR}/${ARCHSUFFIX}/${BINLIB_SUFFIX}${MYPATH_POSTFIX}")
      SET_TARGET_PROPERTIES(OpenFOAMExporter PROPERTIES RUNTIME_OUTPUT_DIRECTORY_${upper_build_type_str} "${COVISE_DESTDIR}/${ARCHSUFFIX}/${BINLIB_SUFFIX}${MYPATH_POSTFIX}")
    ENDIF(CMAKE_CONFIGURATION_TYPES)

csharp_set_designer_cs_properties(
BIM/OpenFOAMExportResource.Designer.cs
BIM/OpenFOAMExportResource.resx
BIM/Properties/Resources.Designer.cs
BIM/Properties/Resources.resx
)
csharp_set_windows_forms_properties(
BIM/AssemblyInfo.cs
BIM/OpenFOAMExportCancelForm.cs
BIM/OpenFOAMExportCancelForm.Designer.cs
BIM/OpenFOAMExportCancelForm.resx
BIM/OpenFOAMExportForm.Designer.cs
BIM/OpenFOAMExportForm.resx
BIM/OpenFOAMExportForm.cs
BIM/OpenFOAMUI/OpenFOAMTextBoxForm.cs
BIM/OpenFOAMUI/OpenFOAMTextBoxForm.Designer.cs
BIM/OpenFOAMUI/OpenFOAMTextBoxForm.resx
)
set_property(TARGET OpenFOAMExporter PROPERTY VS_DOTNET_TARGET_FRAMEWORK_VERSION "v4.8")
#set_property(TARGET OpenFOAMExporter PROPERTY WIN32_EXECUTABLE TRUE)

set_property(TARGET OpenFOAMExporter PROPERTY VS_DOTNET_REFERENCES
    ${EXTRA_REFS}
    "PresentationCore"
    "System"
    "System.Core"
    "System.Data"
    "System.Drawing"
    "System.Windows.Forms"
    "System.Windows.Presentation"
    "System.Xaml"
    "System.Xml"
    "WindowsBase")
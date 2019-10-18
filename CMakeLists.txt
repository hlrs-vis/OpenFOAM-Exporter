USING(Revit)
PROJECT(RevitPlugin CSharp)
include(CSharpUtilities)

add_library(OpenFOAMExporter SHARED
Source/AssemblyInfo.cs
Source/DataGenerator.cs
Source/OpenFoamDialogManager.cs
Source/OpenFoamExportCommand.cs
Source/OpenFoamExporterUI.cs
Source/OpenFoamExportResource.Designer.cs
Source/OpenFoamExportResource.resx
Source/SaveData.cs
Source/Settings.cs
Source/STLExportCancelForm.cs
Source/STLExportCancelForm.Designer.cs
Source/STLExportCancelForm.resx
Source/STLExportForm.cs
Source/STLExportForm.Designer.cs
Source/STLExportForm.resx
Source/Properties/Resources.Designer.cs
Source/Properties/Resources.resx
Source/OpenFOAMUI/OpenFOAMDropDownTreeNode.cs
Source/OpenFOAMUI/OpenFOAMTextBoxTreeNode.cs
Source/OpenFOAMUI/OpenFOAMTreeView.cs
Source/OpenFOAM/BlockMeshDict.cs
Source/OpenFOAM/ControlDict.cs
Source/OpenFOAM/DecomposeParDict.cs
Source/OpenFOAM/Epsilon.cs
Source/OpenFOAM/FoamDict.cs
Source/OpenFOAM/FoamFile.cs
Source/OpenFOAM/FoamParameter.cs
Source/OpenFOAM/FvSchemes.cs
Source/OpenFOAM/FvSolution.cs
Source/OpenFOAM/G.cs
Source/OpenFOAM/K.cs
Source/OpenFOAM/MeshDict.cs
Source/OpenFOAM/Nut.cs
Source/OpenFOAM/P.cs
Source/OpenFOAM/RunManager.cs
Source/OpenFOAM/SnappyHexMeshDict.cs
Source/OpenFOAM/SurfaceFeatureExtract.cs
Source/OpenFOAM/TransportProperties.cs
Source/OpenFOAM/TurbulenceProperties.cs
Source/OpenFOAM/U.cs
Source/OpenFOAM/Version.cs

)
# Set the C# language version (defaults to 3.0).
set(CMAKE_CSharp_FLAGS "/langversion:latest")
set_target_properties(OpenFOAMExporter PROPERTIES FOLDER "${MAIN_FOLDER}/Plugins/RevitPlugins")

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
Source/AssemblyInfo.cs
Source/OpenFoamExportResource.Designer.cs
Source/OpenFoamExportResource.resx
Source/STLExportCancelForm.cs
Source/STLExportCancelForm.Designer.cs
Source/STLExportCancelForm.resx
Source/STLExportForm.cs
Source/STLExportForm.Designer.cs
Source/STLExportForm.resx
Source/Properties/Resources.Designer.cs
Source/Properties/Resources.resx
)
set_property(TARGET OpenFOAMExporter PROPERTY VS_DOTNET_TARGET_FRAMEWORK_VERSION "v4.7.1")
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
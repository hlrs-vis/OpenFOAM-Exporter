﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BIM {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class OpenFOAMExportResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal OpenFOAMExportResource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("BIM.OpenFOAMExportResource", typeof(OpenFOAMExportResource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to STLFile was not saved..
        /// </summary>
        public static string CANCEL_FILE_NOT_SAVED {
            get {
                return ResourceManager.GetString("CANCEL_FILE_NOT_SAVED", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Occur an unknown error,the application will be terminated..
        /// </summary>
        public static string ERR_EXCEPTION {
            get {
                return ResourceManager.GetString("ERR_EXCEPTION", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to File is ReadOnly..
        /// </summary>
        public static string ERR_FILE_READONLY {
            get {
                return ResourceManager.GetString("ERR_FILE_READONLY", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid Format for string:.
        /// </summary>
        public static string ERR_FORMAT {
            get {
                return ResourceManager.GetString("ERR_FORMAT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Occur an IO error,the application will be terminated..
        /// </summary>
        public static string ERR_IO_EXCEPTION {
            get {
                return ResourceManager.GetString("ERR_IO_EXCEPTION", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There is no solid in this document..
        /// </summary>
        public static string ERR_NOSOLID {
            get {
                return ResourceManager.GetString("ERR_NOSOLID", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Save STLFile has failed..
        /// </summary>
        public static string ERR_SAVE_FILE_FAILED {
            get {
                return ResourceManager.GetString("ERR_SAVE_FILE_FAILED", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Make sure you have the required permission to read or write files..
        /// </summary>
        public static string ERR_SECURITY_EXCEPTION {
            get {
                return ResourceManager.GetString("ERR_SECURITY_EXCEPTION", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The vector entries cannot be converted into double..
        /// </summary>
        public static string ERR_VECTOR_FORMAT {
            get {
                return ResourceManager.GetString("ERR_VECTOR_FORMAT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to STL Exporter.
        /// </summary>
        public static string MESSAGE_BOX_TITLE {
            get {
                return ResourceManager.GetString("MESSAGE_BOX_TITLE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to .stl.
        /// </summary>
        public static string SAVE_DIALOG_DEFAULT_FILE_EXTEND {
            get {
                return ResourceManager.GetString("SAVE_DIALOG_DEFAULT_FILE_EXTEND", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to stl files (*.stl)|*.stl|All files (*.*)|*.*.
        /// </summary>
        public static string SAVE_DIALOG_FILE_FILTER {
            get {
                return ResourceManager.GetString("SAVE_DIALOG_FILE_FILTER", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Warning: All linked models must use the same Project Location Position as the host model.
        /// </summary>
        public static string WARN_PROJECT_POSITION {
            get {
                return ResourceManager.GetString("WARN_PROJECT_POSITION", resourceCulture);
            }
        }
    }
}
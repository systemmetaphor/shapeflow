﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ShapeFlow.Infrastructure {
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
    internal class InfrastructureErrorMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal InfrastructureErrorMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ShapeFlow.Infrastructure.InfrastructureErrorMessages", typeof(InfrastructureErrorMessages).Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An unexpected error ocurred while initializing the logging service..
        /// </summary>
        internal static string LogInitializationFailed {
            get {
                return ResourceManager.GetString("LogInitializationFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to It was not possible to execute command &apos;{0}&apos;. {1}.
        /// </summary>
        internal static string RequiredOptionsMissing {
            get {
                return ResourceManager.GetString("RequiredOptionsMissing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An unexpected error ocurred while attempting to complete the request operation. Check the inner exception for details..
        /// </summary>
        internal static string UnexpectedErrorOcurred {
            get {
                return ResourceManager.GetString("UnexpectedErrorOcurred", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A valid Factory function is required..
        /// </summary>
        internal static string ValidFactoryIsRequired {
            get {
                return ResourceManager.GetString("ValidFactoryIsRequired", resourceCulture);
            }
        }
    }
}

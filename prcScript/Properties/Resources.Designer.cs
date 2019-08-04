﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace prcScript.Properties {
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
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("prcScript.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to prcScript: edit params through lua
        ///required: [script file] (allows multiple files)
        ///optional:
        ///  -h = print help text
        ///       (alias: -help)
        ///  -a = print lua api
        ///       (alias: -api)
        ///  -l = load label file [path]
        ///  -s = sandbox lua environment (prevents running unsafe code)
        ///       (alias: -safe | -sandbox).
        /// </summary>
        internal static string Help {
            get {
                return ResourceManager.GetString("Help", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Lua version: 5.3
        ///
        ///[Globals] 
        ///sandbox : boolean
        ///   &lt;returns true if prcScript is run with the -s flag&gt;
        ///Param : userdata
        ///|   &lt;contains methods and properties to interact with params&gt;
        ///+-&gt; open_dir : string
        ///|       &lt;gets or sets the directory root for opening params&gt;
        ///+-&gt; save_dir : string
        ///|       &lt;gets or sets the directory root for saving params&gt;
        ///+-&gt; open : function(string)
        ///|       &lt;returns a param object using the given relative path&gt;
        ///+-&gt; save : function(param)
        ///|       &lt;saves a param object, ass [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string LuaAPI {
            get {
                return ResourceManager.GetString("LuaAPI", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to return {}.
        /// </summary>
        internal static string LuaNewTable {
            get {
                return ResourceManager.GetString("LuaNewTable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to time = os.time
        ///clock = os.clock
        ///date = os.date
        ///
        ///if sandbox then
        ///    collectgarbage = nil
        ///    debug = nil
        ///    dofile = nil
        ///    io = nil
        ///    luanet = nil
        ///    load = nil
        ///    loadfile = nil
        ///    os = nil
        ///    package = nil
        ///    require = nil
        ///end.
        /// </summary>
        internal static string Sandbox {
            get {
                return ResourceManager.GetString("Sandbox", resourceCulture);
            }
        }
    }
}
﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SoundFlux.Resources {
    using System;
    
    
    /// <summary>
    ///   Класс ресурса со строгой типизацией для поиска локализованных строк и т.д.
    /// </summary>
    // Этот класс создан автоматически классом StronglyTypedResourceBuilder
    // с помощью такого средства, как ResGen или Visual Studio.
    // Чтобы добавить или удалить член, измените файл .ResX и снова запустите ResGen
    // с параметром /str или перестройте свой проект VS.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Возвращает кэшированный экземпляр ResourceManager, использованный этим классом.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SoundFlux.Resources.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Перезаписывает свойство CurrentUICulture текущего потока для всех
        ///   обращений к ресурсу с помощью этого класса ресурса со строгой типизацией.
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
        ///   Ищет локализованную строку, похожую на [BASS exception]
        ///
        ///{0}.
        /// </summary>
        public static string BassErrorFormat {
            get {
                return ResourceManager.GetString("BassErrorFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на bit.
        /// </summary>
        public static string BitSuffix {
            get {
                return ResourceManager.GetString("BitSuffix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на channels.
        /// </summary>
        public static string ChannelCountSuffix {
            get {
                return ResourceManager.GetString("ChannelCountSuffix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Mono.
        /// </summary>
        public static string ChannelsConfigMono {
            get {
                return ResourceManager.GetString("ChannelsConfigMono", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Stereo.
        /// </summary>
        public static string ChannelsConfigStereo {
            get {
                return ResourceManager.GetString("ChannelsConfigStereo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Default.
        /// </summary>
        public static string Default {
            get {
                return ResourceManager.GetString("Default", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на [Exception]
        ///
        ///{0}.
        /// </summary>
        public static string ExceptionFormat {
            get {
                return ResourceManager.GetString("ExceptionFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Hz.
        /// </summary>
        public static string HzSuffix {
            get {
                return ResourceManager.GetString("HzSuffix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Input device is not selected.
        /// </summary>
        public static string InputDeviceNotSelectedError {
            get {
                return ResourceManager.GetString("InputDeviceNotSelectedError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Port must be an integer in range from 0 to 65535. Zero means to select a random free port.
        /// </summary>
        public static string InvalidPortError {
            get {
                return ResourceManager.GetString("InvalidPortError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Server address &quot;{0}&quot; has invalid format. Correct format: x.x.x.x:port.
        /// </summary>
        public static string InvalidServerAddressError {
            get {
                return ResourceManager.GetString("InvalidServerAddressError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на • AvaloniaUI
        ///• BASS
        ///• ManagedBass
        ///• .NET CommunityToolkit.Mvvm.
        /// </summary>
        public static string LibrariesUsedText {
            get {
                return ResourceManager.GetString("LibrariesUsedText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Your device is not connected to a network.
        /// </summary>
        public static string NetworkNotConnectedError {
            get {
                return ResourceManager.GetString("NetworkNotConnectedError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на No sound.
        /// </summary>
        public static string NoSound {
            get {
                return ResourceManager.GetString("NoSound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Output device is not selected.
        /// </summary>
        public static string OutputDeviceNotSelectedError {
            get {
                return ResourceManager.GetString("OutputDeviceNotSelectedError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Unnamed.
        /// </summary>
        public static string Unnamed {
            get {
                return ResourceManager.GetString("Unnamed", resourceCulture);
            }
        }
    }
}

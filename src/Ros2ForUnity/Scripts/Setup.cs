using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace ROS2
{
    /// <summary>
    /// Class used for setup tasks.
    /// </summary>
    public static class Setup
    {
        private const string MetadataFileName = "ros2-for-unity";

        [XmlRoot("ros2-for-unity", IsNullable = false)]
        public sealed class MetaData
        {
            [XmlElement("ros")]
            public string RosVersion;

            [XmlElement("standalone")]
            public bool IsStandalone;

            public MetaData()
            { }

            public static MetaData Load()
            {
                XmlSerializer serializer = new XmlSerializer(typeof(MetaData));
                TextAsset asset = Resources.Load<TextAsset>(MetadataFileName);
                return (MetaData)serializer.Deserialize(new MemoryStream(asset.bytes));
            }
        }

        private enum Platform
        {
            Windows,
            Linux
        }

        private static Platform CurrentPlatform
        {
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.LinuxEditor:
                    case RuntimePlatform.LinuxPlayer:
                        return Platform.Linux;
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.WindowsPlayer:
                        return Platform.Windows;
                    default:
                        throw new NotSupportedException("Only Linux and Windows are supported");
                }
            }
        }

        internal static string PluginPath
        {
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.LinuxEditor:
                        return Path.GetFullPath("Packages/ai.robotec.ros2-for-unity/Plugins") + "/Linux/x86_64";
                    case RuntimePlatform.LinuxPlayer:
                        return Path.GetFullPath(Application.dataPath) + "/Plugins";
                    case RuntimePlatform.WindowsEditor:
                        return Path.GetFullPath("Packages/ai.robotec.ros2-for-unity/Plugins").Replace("/", "\\") + "\\Windows\\x86_64";
                    case RuntimePlatform.WindowsPlayer:
                        return Path.GetFullPath(Application.dataPath).Replace("/", "\\") + "\\Plugins\\x86_64";
                    default:
                        throw new NotSupportedException("Only Linux and Windows are supported");
                }
            }
        }

        private static string GetSourcedROS()
        {
            return Environment.GetEnvironmentVariable("ROS_DISTRO");
        }

        /// <summary>
        /// Setup PATH for native libraries.
        /// </summary>
        /// <remarks>
        /// Is called automatically when Unity starts, has to be called manually when running in edit mode.
        /// </remarks>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void SetupPath()
        {
            string pathVariable;
            string pathSeparator;
            switch (CurrentPlatform)
            {
                case Platform.Windows:
                    pathVariable = "PATH";
                    pathSeparator = ";";
                    break;
                case Platform.Linux:
                    pathVariable = "LD_LIBRARY_PATH";
                    pathSeparator = ":";
                    break;
                default:
                    throw new NotSupportedException("unsupported platform");
            }
            Environment.SetEnvironmentVariable(pathVariable, $"{PluginPath}{pathSeparator}{Environment.GetEnvironmentVariable(pathVariable)}");
        }

        /// <summary>
        /// Check for correct ROS2 setup.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void CheckIntegrity()
        {
            MetaData metaData = MetaData.Load();
            string sourced = GetSourcedROS();
            if (!metaData.IsStandalone && sourced != metaData.RosVersion)
            {
                Debug.LogError(
                    $"ROS2 version in 'ros2cs' metadata ({metaData.RosVersion}) doesn't match currently sourced version ({sourced}). " +
                    "This is caused by mixing versions/builds. Plugin might not work correctly."
                );
            }
            if (metaData.IsStandalone && !string.IsNullOrEmpty(sourced))
            {
                Debug.LogError(
                    "You should not source ROS2 in 'ros2-for-unity' standalone build. " +
                    "Plugin might not work correctly."
                );
            }
            string rosVersion = string.IsNullOrEmpty(sourced) ? metaData.RosVersion : sourced;
            string standalone = metaData.IsStandalone ? "standalone" : "non-standalone";
            Debug.Log($"ROS2 version: {rosVersion}. Build type: {standalone}. RMW: {Context.GetRMWImplementation()}");
        }

        /// <summary>
        /// Connect <see cref="Ros2csLogger"/> to the Unity logging system.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void ConnectLoggers()
        {
            Ros2csLogger.setCallback(LogLevel.ERROR, Debug.LogError);
            Ros2csLogger.setCallback(LogLevel.WARNING, Debug.LogWarning);
            Ros2csLogger.setCallback(LogLevel.INFO, Debug.Log);
            Ros2csLogger.setCallback(LogLevel.DEBUG, Debug.Log);
            Ros2csLogger.LogLevel = LogLevel.WARNING;
        }
    }
}
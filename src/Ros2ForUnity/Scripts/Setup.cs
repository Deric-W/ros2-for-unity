using System;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ROS2
{
    /// <summary>
    /// Class used for setup tasks.
    /// </summary>
    internal static class Setup
    {
        private enum Platform
        {
            Windows,
            Linux
        }

        private static Platform GetOS()
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

        private static string GetOSName(this Platform platform)
        {
            switch (platform)
            {
                case Platform.Linux:
                    return "Linux";
                case Platform.Windows:
                    return "Windows";
                default:
                    throw new NotSupportedException("Only Linux and Windows are supported");
            }
        }

        internal static string GetRos2ForUnityPath()
        {
            string pluginPath = Application.dataPath;

            if (Application.isEditor)
            {
                pluginPath += Path.DirectorySeparatorChar + "Ros2ForUnity";
            }
            return pluginPath;
        }

        internal static string GetPluginPath()
        {
            string pluginPath = GetRos2ForUnityPath();
            Platform platform = GetOS();

            pluginPath += Path.DirectorySeparatorChar + "Plugins";

            if (Application.isEditor)
            {
                pluginPath += Path.DirectorySeparatorChar + platform.GetOSName();
            }

            if (Application.isEditor || platform == Platform.Windows)
            {
                pluginPath += Path.DirectorySeparatorChar + "x86_64";
            }

            if (platform == Platform.Windows)
            {
                pluginPath = pluginPath.Replace("/", "\\");
            }

            return pluginPath;
        }

        private static (string ROS, bool Standalone) GetRos2csMetadata()
        {
            XmlDocument metadata = new XmlDocument();
            metadata.Load(GetPluginPath() + Path.DirectorySeparatorChar + "metadata_ros2cs.xml");
            return (
                metadata.DocumentElement.SelectSingleNode("/ros2cs/ros2").InnerText,
                Convert.ToBoolean(Convert.ToInt16(metadata.DocumentElement.SelectSingleNode("/ros2cs/standalone").InnerText))
            );
        }

        private static string GetRos2ForUnityMetadata()
        {
            XmlDocument metadata = new XmlDocument();
            metadata.Load(GetRos2ForUnityPath() + Path.DirectorySeparatorChar + "metadata_ros2_for_unity.xml");
            return metadata.DocumentElement.SelectSingleNode("/ros2_for_unity/ros2").InnerText;
        }

        private static string GetSourcedROS()
        {
            return Environment.GetEnvironmentVariable("ROS_DISTRO");
        }

        /// <summary>
        /// Check for correct ROS2 setup.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void CheckIntegrity()
        {
            string sourced = GetSourcedROS();
            var ros2cs = GetRos2csMetadata();
            var ros2forUnity = GetRos2ForUnityMetadata();
            if (ros2cs.ROS != ros2forUnity)
            {
                Debug.LogError(
                    $"ROS2 versions in 'ros2cs' ({ros2cs.ROS}) and 'ros2-for-unity' ({ros2forUnity}) metadata files are not the same. " +
                    "This is caused by mixing versions/builds. Plugin might not work correctly."
                );
            }
            if (!ros2cs.Standalone && sourced != ros2cs.ROS)
            {
                Debug.LogError(
                    $"ROS2 version in 'ros2cs' metadata ({ros2cs.ROS}) doesn't match currently sourced version ({sourced}). " +
                    "This is caused by mixing versions/builds. Plugin might not work correctly."
                );
            }
            if (ros2cs.Standalone && !string.IsNullOrEmpty(sourced))
            {
                Debug.LogError(
                    "You should not source ROS2 in 'ros2-for-unity' standalone build. " +
                    "Plugin might not work correctly."
                );
            }
            string rosVersion = string.IsNullOrEmpty(sourced) ? ros2forUnity : sourced;
            string standalone = ros2cs.Standalone ? "standalone" : "non-standalone";
            Debug.Log($"ROS2 version: {rosVersion}. Build type: {standalone}. RMW: {Context.GetRMWImplementation()}");
        }

        /// <summary>
        /// Setup PATH for native libraries.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void SetupPath()
        {
            if (GetOS() == Platform.Windows)
            {
                Environment.SetEnvironmentVariable("PATH", $"{GetPluginPath()};{Environment.GetEnvironmentVariable("PATH")}");
            }
            else
            {
                ROS2.GlobalVariables.absolutePath = $"{GetPluginPath()}/";
            }
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
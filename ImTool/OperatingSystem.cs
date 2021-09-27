namespace ImTool
{
    using System.Runtime.InteropServices;

    public static class OperatingSystem
    {
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        public static bool Is64BitProcess => System.Environment.Is64BitProcess;

        public static string GetRuntimeIdentifier => (IsWindows ? "win" : IsLinux ? "linux" : IsMacOS ? "osx" : "unknown") + "-" + (Is64BitProcess ? "x64" : "x86");

    }
}
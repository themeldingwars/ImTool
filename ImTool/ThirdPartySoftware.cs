using System.Collections.Generic;

namespace ImTool;

public static class ThirdPartySoftware
{
    public static readonly List<ThirdPartySoftwareInfo> Info = new List<ThirdPartySoftwareInfo>
    {
        new ThirdPartySoftwareInfo
        {
            Name = "Veldrid",
            ProjectUrl = "https://github.com/mellinoe/veldrid",
            License = "MIT",
            LicenseUrl = "https://github.com/mellinoe/veldrid/blob/master/LICENSE"
        },
        
        new ThirdPartySoftwareInfo
        {
            Name = "ImGui.NET",
            ProjectUrl = "https://github.com/mellinoe/ImGui.NET",
            License = "MIT",
            LicenseUrl = "https://github.com/mellinoe/ImGui.NET/blob/master/LICENSE"
        },
        
        new ThirdPartySoftwareInfo
        {
            Name = "imgui",
            ProjectUrl = "https://github.com/ocornut/imgui",
            License = "MIT",
            LicenseUrl = "https://github.com/ocornut/imgui/blob/master/LICENSE.txt"
        },
        
        new ThirdPartySoftwareInfo
        {
            Name = "cimgui",
            ProjectUrl = "https://github.com/cimgui/cimgui",
            License = "MIT",
            LicenseUrl = "https://github.com/cimgui/cimgui/blob/master/LICENSE"
        },
        
        new ThirdPartySoftwareInfo
        {
            Name = "implot",
            ProjectUrl = "https://github.com/epezent/implot",
            License = "MIT",
            LicenseUrl = "https://github.com/epezent/implot/blob/master/LICENSE"
        },
        
        new ThirdPartySoftwareInfo
        {
            Name = "ImGuizmo",
            ProjectUrl = "https://github.com/CedricGuillemet/ImGuizmo",
            License = "MIT",
            LicenseUrl = "https://github.com/CedricGuillemet/ImGuizmo/blob/master/LICENSE"
        },
        
        new ThirdPartySoftwareInfo
        {
            Name = "imnodes",
            ProjectUrl = "https://github.com/Nelarius/imnodes",
            License = "MIT",
            LicenseUrl = "https://github.com/Nelarius/imnodes/blob/master/LICENSE.md"
        },
        
        new ThirdPartySoftwareInfo
        {
            Name = "vk",
            ProjectUrl = "https://github.com/mellinoe/vk",
            License = "MIT",
            LicenseUrl = "https://github.com/mellinoe/vk/blob/master/LICENSE.md"
        },
        
        new ThirdPartySoftwareInfo
        {
            Name = "Vortice.Windows",
            ProjectUrl = "https://github.com/amerkoleci/Vortice.Windows",
            License = "MIT",
            LicenseUrl = "https://github.com/amerkoleci/Vortice.Windows/blob/main/LICENSE"
        },
        
        new ThirdPartySoftwareInfo
        {
            Name = "ImageSharp",
            ProjectUrl = "https://github.com/SixLabors/ImageSharp",
            License = "Apache 2.0",
            LicenseUrl = "https://github.com/SixLabors/ImageSharp/blob/main/LICENSE"
        },
        
        new ThirdPartySoftwareInfo
        {
            Name = "octokit.net",
            ProjectUrl = "https://github.com/octokit/octokit.net",
            License = "MIT",
            LicenseUrl = "https://github.com/octokit/octokit.net/blob/main/LICENSE.txt"
        },
        
        new ThirdPartySoftwareInfo
        {
            Name = "Newtonsoft.Json",
            ProjectUrl = "https://github.com/JamesNK/Newtonsoft.Json",
            License = "MIT",
            LicenseUrl = "https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md"
        },
        
        new ThirdPartySoftwareInfo
        {
            Name = "K4os.Hash.xxHash",
            ProjectUrl = "https://github.com/MiloszKrajewski/K4os.Hash.xxHash",
            License = "MIT",
            LicenseUrl = "https://github.com/MiloszKrajewski/K4os.Hash.xxHash/blob/master/LICENSE"
        },
        
        new ThirdPartySoftwareInfo
        {
            Name = "nativelibraryloader",
            ProjectUrl = "https://github.com/mellinoe/nativelibraryloader",
            License = "MIT",
            LicenseUrl = "https://github.com/mellinoe/nativelibraryloader/blob/master/LICENSE"
        },
        
        new ThirdPartySoftwareInfo
        {
            Name = "SDL",
            ProjectUrl = "https://github.com/libsdl-org/SDL",
            License = "zlib",
            LicenseUrl = "https://github.com/libsdl-org/SDL/blob/main/LICENSE.txt"
        },
        
        new ThirdPartySoftwareInfo
        {
            Name = ".NET Runtime",
            ProjectUrl = "https://github.com/dotnet/runtime",
            License = "MIT",
            LicenseUrl = "https://github.com/dotnet/runtime/blob/main/LICENSE.TXT"
        },
        
        new ThirdPartySoftwareInfo
        {
            Name = "Font-Awesome",
            ProjectUrl = "https://github.com/FortAwesome/Font-Awesome",
            License = "Font Awesome Free License",
            LicenseUrl = "https://github.com/FortAwesome/Font-Awesome/blob/6.x/LICENSE.txt"
        },
        
        new ThirdPartySoftwareInfo
        {
            Name = "GNU FreeFont",
            ProjectUrl = "https://www.gnu.org/software/freefont",
            License = "GNU GPLv3",
            LicenseUrl = "https://www.gnu.org/licenses/gpl-3.0.html"
        },
        
        new ThirdPartySoftwareInfo
        {
            Name = "Source Sans 3",
            ProjectUrl = "https://github.com/adobe-fonts/source-sans",
            License = "OFL-1.1",
            LicenseUrl = "https://github.com/adobe-fonts/source-sans/blob/release/LICENSE.md"
        },
        
        
    };

    public record ThirdPartySoftwareInfo
    {
        public string Name { get; init; }
        public string ProjectUrl { get; init; }
        public string License { get; init; }
        public string LicenseUrl { get; init; }
    }
}
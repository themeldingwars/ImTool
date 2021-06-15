using System.Numerics;
using ImGuiNET;

namespace ImTool
{
    public static partial class Widgets
    {
        public static bool ToggleHeader(string label)
        {
            ImGuiStoragePtr storage = ImGui.GetStateStorage();
            uint id = ImGui.GetID(label);
            bool toggled = storage.GetBool(id);
            
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0f,0.5f));
    
            Vector2 pos = ImGui.GetCursorScreenPos();
            
            if (ImGui.Button($"  {label}", new Vector2(ImGui.GetColumnWidth(), 24)))
            {
                toggled = !toggled;
                storage.SetBool(id, toggled);
            }
            
            pos.X += 4;
            pos.Y += 6;
            RenderArrow(pos, ImGui.GetColorU32(ImGuiCol.Text), toggled ? ImGuiDir.Down : ImGuiDir.Right, 1f);
            
            ImGui.PopStyleVar(2);
            return toggled;
        }
    }
}
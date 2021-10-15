using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;

namespace ImTool
{
    public class LogWindow<TCategoryType> where TCategoryType : struct, Enum
    {
        public string        Name;
        public List<LogLine> Lines = new(256);

        public bool[]   LogLevelsToShow = new bool[4];
        public bool[]   CategoriesToShow;
        public string[] CategoriesNames;

        private List<int> FilteredLineIdxs = new(256);

        public LogWindow(string name)
        {
            Name = name;

            CategoriesNames  = Enum.GetNames<TCategoryType>();
            CategoriesToShow = new bool[CategoriesNames.Length];

            for (int i = 0; i < LogLevelsToShow.Length; i++) {
                LogLevelsToShow[i] = true;
            }

            for (int i = 0; i < CategoriesToShow.Length; i++) {
                CategoriesToShow[i] = true;
            }
        }

        public void ToggleLevel(LogLevel level, bool? value = null)
        {
            LogLevelsToShow[(int) level] = value ?? !LogLevelsToShow[(int) level];
            ApplyFilters();
        }

        public void ToggleCategory(TCategoryType cat, bool? value = null)
        {
            CategoriesToShow[(int) (object) cat] = value ?? !CategoriesToShow[(int) (object) cat];
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            FilteredLineIdxs?.Clear();
            FilteredLineIdxs.Capacity = Lines.Count;

            for (var i = 0; i < Lines.Count; i++) {
                var line = Lines[i];
                if (LogLevelsToShow[(int) line.Level] && CategoriesToShow[line.Category]) {
                    FilteredLineIdxs.Add(i);
                }
            }
        }

        public unsafe void Draw()
        {
            ImGui.SetNextItemWidth(300);
            if (ImGui.BeginCombo("###LogLevels", "LogLevels")) {
                if (ImGui.Checkbox("Trace", ref LogLevelsToShow[0]) ||
                    ImGui.Checkbox("Info", ref LogLevelsToShow[1])  ||
                    ImGui.Checkbox("Warn", ref LogLevelsToShow[2])  ||
                    ImGui.Checkbox("Error", ref LogLevelsToShow[3])) {
                    ApplyFilters();
                }

                ImGui.EndCombo();
            }

            ImGui.SameLine();

            ImGui.SetNextItemWidth(300);
            if (ImGui.BeginCombo("###Categories", "Categories")) {
                var hasChanged = false;
                for (int i = 0; i < CategoriesToShow.Length; i++) {
                    hasChanged |= ImGui.Checkbox(CategoriesNames[i], ref CategoriesToShow[i]);
                }

                if (hasChanged) {
                    ApplyFilters();
                }

                ImGui.EndCombo();
            }

            ImGui.SameLine();

            if (ImGui.Button("Clear All")) ClearAll();

            if (ImGui.BeginTable("Logs", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable | ImGuiTableFlags.ScrollY | ImGuiTableFlags.RowBg)) {
                ImGui.TableSetupColumn("Time", ImGuiTableColumnFlags.None, 1.2f);
                ImGui.TableSetupColumn("Level", ImGuiTableColumnFlags.None, 1f);
                ImGui.TableSetupColumn("Category", ImGuiTableColumnFlags.None, 1.2f);
                ImGui.TableSetupColumn("Message", ImGuiTableColumnFlags.None, 10f);
                ImGui.TableSetupScrollFreeze(4, 1);
                ImGui.TableHeadersRow();

                ImGuiListClipper    clipperData;
                ImGuiListClipperPtr clipper = new ImGuiListClipperPtr(&clipperData);
                clipper.Begin(FilteredLineIdxs.Count);

                while (clipper.Step()) {
                    for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++) {
                        if (i < FilteredLineIdxs.Count) {
                            var line = Lines[FilteredLineIdxs[i]];
                            var color = line.Level switch
                            {
                                LogLevel.Trace => ImToolColors.LogTrace,
                                LogLevel.Info  => ImToolColors.LogInfo,
                                LogLevel.Warn  => ImToolColors.LogWarn,
                                LogLevel.Error => ImToolColors.LogError,
                                _              => ImToolColors.LogInfo
                            };

                            ImGui.TableNextColumn();
                            ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ImGui.ColorConvertFloat4ToU32(color));
                            if (line.ParentIdx <= 0) ImGui.Text($"{line.Time}");

                            ImGui.TableNextColumn();
                            ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ImGui.ColorConvertFloat4ToU32(color));
                            if (line.ParentIdx <= 0) ImGui.Text($"{line.Level}");

                            ImGui.TableNextColumn();
                            ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ImGui.ColorConvertFloat4ToU32(color));
                            if (line.ParentIdx <= 0) ImGui.Text($"{CategoriesNames[line.Category]}");

                            ImGui.TableNextColumn();
                            ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ImGui.ColorConvertFloat4ToU32(color));
                            ImGui.Text($"{line.Line}");
                        }
                    }
                }

                clipper.End();

                ImGui.EndTable();
            }
        }

        public void DrawWindow()
        {
            if (ImGui.Begin(Name)) {
                Draw();
            }

            ImGui.End();
        }

        public void AddLog(LogLevel level, TCategoryType cat, string message)
        {
            var logLine = new LogLine
            {
                Time      = DateTime.Now.ToLongTimeString(),
                Level     = level,
                Category  = (int) (object) cat,
                Line      = message,
                ParentIdx = -1
            };

            // If the message has new lines split those across multiple lines so scrolling works right
            if (message.Contains('\n')) {
                var lines     = message.Split('\n');
                var parentIdx = Lines.Count;
                logLine.Line = lines.First();
                Lines.Add(logLine);

                if (LogLevelsToShow[(int) logLine.Level] && CategoriesToShow[logLine.Category])
                    FilteredLineIdxs.Add(Lines.Count - 1);

                foreach (var line in lines[1..]) {
                    Lines.Add(new LogLine
                    {
                        Line      = line,
                        ParentIdx = parentIdx,
                        Category  = logLine.Category,
                        Level     = logLine.Level
                    });

                    if (LogLevelsToShow[(int) logLine.Level] && CategoriesToShow[logLine.Category])
                        FilteredLineIdxs.Add(Lines.Count - 1);
                }
            }

            else {
                Lines.Add(logLine);

                if (LogLevelsToShow[(int) logLine.Level] && CategoriesToShow[logLine.Category])
                    FilteredLineIdxs.Add(Lines.Count - 1);
            }
        }

        public void AddLogTrace(TCategoryType cat, string message) => AddLog(LogLevel.Trace, cat, message);
        public void AddLogInfo(TCategoryType  cat, string message) => AddLog(LogLevel.Info, cat, message);
        public void AddLogWarn(TCategoryType  cat, string message) => AddLog(LogLevel.Warn, cat, message);
        public void AddLogError(TCategoryType cat, string message) => AddLog(LogLevel.Error, cat, message);

        public void ClearAll()
        {
            Lines.Clear();
            FilteredLineIdxs.Clear();
        }

        public enum LogLevel : byte
        {
            Trace,
            Info,
            Warn,
            Error
        }

        public struct LogLine
        {
            public string   Time;
            public LogLevel Level;
            public int      Category;
            public string   Line;
            public int      ParentIdx;
        }
    }
}
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Num = System.Numerics;

namespace ImTool
{
    public class FileBrowser
    {
        public const  string POPUP_ID                        = "File Browser";
        const         string OVERRIDE_EXISTING_FILE_POPUP_ID = "Override Existing File?";
        public static bool   IsOpen                          = false;

        public static string        CurrentDir            = Environment.CurrentDirectory;
        public static string        LastDir               = Environment.CurrentDirectory;
        public static Stack<string> DirHistory            = new Stack<string>();
        public static Mode          DiaglogMode           = Mode.OpenFile;
        public static bool          ShowOverrideFilePopup = false;

        public static List<EntryInfo> CurrentEntries = new();
        public static string          SaveFilePath   = "";

        private static Action<string>   OpenFileCb    = null;
        private static Action<string[]> OpenFilesCb   = null;
        private static Action<string>   SelectDirCb   = null;
        private static Action<string>   SaveFileCb    = null;
        private static string           SearchPattern = "*";

        public static void OpenFile(Action<string> openFileCb, string baseDir = null, string searchPattern = null)
        {
            DiaglogMode = Mode.OpenFile;
            OpenFileCb  = openFileCb;
            Open(baseDir, searchPattern);
        }

        public static void OpenFiles(Action<string[]> openFilesCb, string baseDir = null, string searchPattern = null)
        {
            DiaglogMode = Mode.OpenMultiple;
            OpenFilesCb = openFilesCb;
            Open(baseDir, searchPattern);
        }

        public static void SaveFile(Action<string> saveFileCb, string baseDir = null, string searchPattern = null)
        {
            DiaglogMode = Mode.SaveFile;
            SaveFileCb  = saveFileCb;
            Open(baseDir, searchPattern);
        }

        public static void SelectDir(Action<string> selectDirCb, string baseDir = null)
        {
            DiaglogMode = Mode.SelectDir;
            SelectDirCb = selectDirCb;
            Open(baseDir);
        }

        private static void Open(string baseDir = null, string searchPattern = null)
        {
            SearchPattern = searchPattern;
            ChangeDir(baseDir ?? LastDir);
            IsOpen = true;
            DirHistory.Clear();
            ImGui.OpenPopup(POPUP_ID);
        }

        public static void Draw()
        {
            ImGui.SetNextWindowSize(new Num.Vector2(800, 400), ImGuiCond.Once);
            if (ImGui.BeginPopupModal(POPUP_ID, ref IsOpen)) {
                if (ImGui.ArrowButton("Back", ImGuiDir.Left)) GoBack();
                ImGui.SameLine();

                if (ImGui.ArrowButton("Up", ImGuiDir.Up)) GoUp();
                ImGui.SameLine();

                ImGui.SetNextItemWidth(-1);
                if (ImGui.InputText("###CurrentDir", ref CurrentDir, 400, ImGuiInputTextFlags.EnterReturnsTrue)) {
                    ChangeDir(CurrentDir);
                }

                if (ImGui.BeginChildFrame(1, new Num.Vector2(-1, -25), ImGuiWindowFlags.NoBackground)) {
                    // Draw the entries
                    if (ImGui.BeginTable("Dir Entries", 5,
                        ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Sortable)) {
                        ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.DefaultSort, 4f, 0);
                        ImGui.TableSetupColumn("Extension", ImGuiTableColumnFlags.DefaultSort, 0.8f);
                        ImGui.TableSetupColumn("Created", ImGuiTableColumnFlags.DefaultSort, 1f);
                        ImGui.TableSetupColumn("Modified", ImGuiTableColumnFlags.DefaultSort, 1f);
                        ImGui.TableSetupColumn("Size", ImGuiTableColumnFlags.DefaultSort, 0.5f);
                        ImGui.TableHeadersRow();

                        var sorts = ImGui.TableGetSortSpecs();
                        if (sorts.SpecsDirty) {
                            SortEntrys(sorts);
                        }

                        foreach (var entry in CurrentEntries) {
                            ImGui.TableNextColumn();
                            if (ImGui.Selectable($"###SelectEntry{entry.Name}", entry.IsSelected,
                                ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowDoubleClick | ImGuiSelectableFlags.DontClosePopups)) {
                                if (HandleSelection(entry)) return;
                            }

                            ImGui.SameLine();

                            if (entry.EntryType == EntryInfo.EntryTypes.Dir) {
                                ImGui.Text("[DIR]  ");
                            }
                            else {
                                ImGui.Text("[FILE] ");
                            }

                            ImGui.SameLine();

                            ImGui.Text(entry.Name);
                            ImGui.TableNextColumn();
                            ImGui.Text(entry.Extension);
                            ImGui.TableNextColumn();
                            ImGui.Text(entry.DateCreated);
                            ImGui.TableNextColumn();
                            ImGui.Text(entry.DateModified);
                            ImGui.TableNextColumn();
                            ImGui.Text(entry.SizeStr);
                        }

                        ImGui.EndTable();
                    }

                    ImGui.EndChildFrame();

                    DrawBottomBar();
                }

                // Confirm Override popup
                if (ImGui.BeginPopupModal(OVERRIDE_EXISTING_FILE_POPUP_ID, ref ShowOverrideFilePopup)) {
                    ImGui.Text("The selected file already exists, are you sure you want to override?");
                    if (ImGui.Button("Override")) {
                        SaveFileCb(SaveFilePath);
                        IsOpen                = false;
                        ShowOverrideFilePopup = false;
                    }

                    ImGui.SameLine();

                    if (ImGui.Button("Cancel")) {
                        ShowOverrideFilePopup = false;
                    }

                    ImGui.EndPopup();
                }

                ImGui.EndPopup();
            }
        }

        private static bool HandleSelection(EntryInfo entry)
        {
            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) || ImGui.IsMouseReleased(ImGuiMouseButton.Right)) {
                if (entry.EntryType == EntryInfo.EntryTypes.Dir) {
                    ChangeDir(entry.Path);
                    return true;
                }
                else if (entry.EntryType == EntryInfo.EntryTypes.File) {
                    if (DiaglogMode == Mode.OpenFile) {
                        OpenFileCb(entry.Path);
                        IsOpen = false;
                    }

                    if (DiaglogMode == Mode.SaveFile) {
                        CheckAndHandleExistingFile(entry.Path);
                    }
                }
            }

            if (DiaglogMode == Mode.OpenFile || DiaglogMode == Mode.SaveFile || DiaglogMode == Mode.SelectDir) ClearSelected();

            if ((entry.EntryType == EntryInfo.EntryTypes.Dir  && DiaglogMode == Mode.SelectDir) ||
                (entry.EntryType == EntryInfo.EntryTypes.File && DiaglogMode != Mode.SelectDir)) {
                entry.IsSelected = !entry.IsSelected;
            }

            if (DiaglogMode == Mode.SaveFile) {
                SaveFilePath = entry.Path;
            }

            return false;
        }

        private static void CheckAndHandleExistingFile(string filePath)
        {
            if (File.Exists(filePath)) {
                ImGui.OpenPopup(OVERRIDE_EXISTING_FILE_POPUP_ID);
                ShowOverrideFilePopup = true;
            }
            else {
                SaveFileCb(filePath);
                ShowOverrideFilePopup = false;
                IsOpen                = false;
            }
        }

        private static void DrawBottomBar()
        {
            if (DiaglogMode == Mode.OpenFile || DiaglogMode == Mode.OpenMultiple) {
                ImGui.Dummy(new Num.Vector2(0, 0));
                ImGui.SameLine(ImGui.GetWindowWidth() - 135);
                if (ImGui.Button(" Open ")) {
                    if (DiaglogMode == Mode.OpenFile) {
                        var selected = CurrentEntries.FirstOrDefault(x => x.IsSelected);
                        if (selected != default) {
                            OpenFileCb(selected.Path);
                            IsOpen = false;
                        }
                    }
                    else {
                        var paths = CurrentEntries.Where(x => x.IsSelected).Select(x => x.Path).ToArray();
                        if (paths.Length > 0) {
                            OpenFilesCb(paths);
                            IsOpen = false;
                        }
                    }
                }

                ImGui.SameLine(ImGui.GetWindowWidth() - 75);
                if (ImGui.Button("Cancel")) {
                    IsOpen = false;
                }
            }
            else if (DiaglogMode == Mode.SaveFile) {
                ImGui.SetNextItemWidth(-135);
                ImGui.InputText("###SaveFilePath", ref SaveFilePath, 400);
                ImGui.SameLine(ImGui.GetWindowWidth() - 135);
                if (ImGui.Button(" Save ")) {
                    CheckAndHandleExistingFile(SaveFilePath);
                }

                ImGui.SameLine(ImGui.GetWindowWidth() - 75);
                if (ImGui.Button("Cancel")) {
                    IsOpen = false;
                }
            }
            else if (DiaglogMode == Mode.SelectDir) {
                ImGui.Dummy(new Num.Vector2(0, 0));
                ImGui.SameLine(ImGui.GetWindowWidth() - 150);
                if (ImGui.Button(" Select ")) {
                    var selected = CurrentEntries.FirstOrDefault(x => x.IsSelected);
                    if (selected != default) {
                        SelectDirCb(selected.Path);
                        IsOpen = false;
                    }
                }

                ImGui.SameLine(ImGui.GetWindowWidth() - 75);
                if (ImGui.Button("Cancel")) {
                    IsOpen = false;
                }
            }
        }

        private static void SortEntrys(ImGuiTableSortSpecsPtr sorts)
        {
            CurrentEntries = CurrentEntries.OrderBy(x =>
            {
                return sorts.Specs.ColumnIndex switch
                {
                    0 => x.Name,
                    1 => x.Extension,
                    2 => x.DateCreated,
                    3 => x.DateModified,
                    4 => x.Size as object,
                    _ => x.Name
                };
            }).ToList();

            if (sorts.Specs.SortDirection == ImGuiSortDirection.Descending) {
                CurrentEntries.Reverse();
            }

            sorts.SpecsDirty = false;
        }

        public static void GoBack()
        {
            if (DirHistory.TryPop(out string dir)) {
                ChangeDir(dir, true);
            }
        }

        public static void GoUp()
        {
            try {
                var parentDir = new DirectoryInfo(CurrentDir);
                if (parentDir.Parent != null) {
                    ChangeDir(parentDir.Parent.FullName);
                }
                else {
                    CurrentEntries.Clear();
                    var drives = Directory.GetLogicalDrives();
                    foreach (var drive in drives) {
                        var dirEntry = new EntryInfo
                        {
                            EntryType = EntryInfo.EntryTypes.Dir,
                            Name      = drive,
                            Path      = drive,
                        };
                        CurrentEntries.Add(dirEntry);
                    }
                }
            }
            catch (Exception e) {
            }
        }

        public static void ChangeDir(string dir, bool noHistory = false)
        {
            try {
                var dirs = Directory.GetDirectories(dir);

                CurrentEntries.Clear();
                foreach (var dirStr in dirs) {
                    var dirInfo = new DirectoryInfo(dirStr);
                    var dirEntry = new EntryInfo
                    {
                        EntryType    = EntryInfo.EntryTypes.Dir,
                        Name         = dirInfo.Name,
                        Path         = dirInfo.FullName,
                        DateCreated  = $"{dirInfo.CreationTime.ToShortDateString()} {dirInfo.CreationTime.ToShortTimeString()}",
                        DateModified = $"{dirInfo.LastWriteTime.ToShortDateString()} {dirInfo.LastWriteTime.ToShortTimeString()}"
                    };
                    CurrentEntries.Add(dirEntry);
                }

                if (DiaglogMode != Mode.SelectDir) {
                    var files = Directory.GetFiles(dir, SearchPattern ?? "*");
                    foreach (var fileStr in files) {
                        var fileInfo = new FileInfo(fileStr);
                        var fileEntry = new EntryInfo
                        {
                            EntryType    = EntryInfo.EntryTypes.File,
                            Name         = fileInfo.Name,
                            Path         = fileInfo.FullName,
                            DateCreated  = $"{fileInfo.CreationTime.ToShortDateString()} {fileInfo.CreationTime.ToShortTimeString()}",
                            DateModified = $"{fileInfo.LastWriteTime.ToShortDateString()} {fileInfo.LastWriteTime.ToShortTimeString()}",
                            Extension    = fileInfo.Extension,
                            Size         = fileInfo.Length,
                            SizeStr      = $"{FormatFileSize(fileInfo.Length)}"
                        };
                        CurrentEntries.Add(fileEntry);
                    }
                }

                if (!noHistory) DirHistory.Push(CurrentDir);

                CurrentDir = dir;
                LastDir    = dir;
            }
            catch (Exception e) {
            }
        }

        public static void ClearSelected()
        {
            foreach (var entry in CurrentEntries) {
                entry.IsSelected = false;
            }
        }

        public static string FormatFileSize(long bytes)
        {
            var unit = 1024;
            if (bytes < unit) {
                return $"{bytes} B";
            }

            var exp = (int) (Math.Log(bytes) / Math.Log(unit));
            return $"{bytes / Math.Pow(unit, exp):F2} {("KMGTPE")[exp - 1]}B";
        }

        public class EntryInfo
        {
            public enum EntryTypes
            {
                File,
                Dir
            }

            public string     Name;
            public string     Path;
            public EntryTypes EntryType;
            public long       Size;
            public string     SizeStr      = "";
            public string     DateCreated  = "";
            public string     DateModified = "";
            public string     Extension    = "";
            public bool       IsSelected;
        }

        public enum Mode : byte
        {
            OpenFile,
            OpenMultiple,
            SelectDir,
            SaveFile
        }
    }
}
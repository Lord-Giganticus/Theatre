﻿using ImGuiNET;
using NativeFileDialogExtendedSharp;
using Silk.NET.Windowing;
using Theatre.Utils;
using SharpCompress.Readers;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives;

namespace Theatre.Handlers
{
    internal class FrameHandler
    {
        public static void RunFrame(IWindow window, ref List<string> selectedFiles, string ryuModsPath, string cacheModsPath)
        {
            ImGui.SetCursorPos(UIUtils.CenterCursorWithText(window, "3DW Mod Manager"));
            UIUtils.TextColoured(new Colour(1, 0, 0), "3DW Mod Manager");

            ImGui.GetFont().FontSize *= 1.3f;
            ImGui.SetCursorPos(UIUtils.CenterCursorWithText(window, "Theatre"));
            ImGui.SetCursorPosY(ImGui.GetCursorPosY()+ 25);
            UIUtils.TextColoured(new Colour(0, 0, 1), "Theatre");
            ImGui.GetFont().FontSize /= 1.3f;
            

            var tempPath = Path.GetTempPath() + "ModManagerUI";



            if (UIUtils.Button("Import Mod"))
            {
                var filePick = Nfd.FileOpen(new []{NfdFilterPresets.ZipFiles}, "Downloads");
                if (filePick.Status == NfdStatus.Ok)
                {

                    Console.WriteLine(filePick.Path);
                    selectedFiles.Add(filePick.Path);
                    Directory.CreateDirectory(tempPath);
                    FileInfo info = new(filePick.Path);


                    using (var stream = info.OpenRead())
                    {
                        // Zip and Rar get handled by ReaderFactory, but 7z isn't.
                        if (info.Extension != ".7z")
                        {
                            using var reader = ReaderFactory.Open(stream);
                            reader.WriteAllToDirectory(tempPath, new()
                            {
                                ExtractFullPath = true,
                                Overwrite = true,
                                PreserveFileTime = true,
                                PreserveAttributes = true
                            });
                        }
                        else
                        {
                            using var reader = SevenZipArchive.Open(stream);
                            reader.WriteToDirectory(tempPath, new()
                            {
                                ExtractFullPath = true,
                                Overwrite = true,
                                PreserveAttributes = true,
                                PreserveFileTime = true
                            });
                        }
                    }

                    foreach (var file in Directory.EnumerateFiles(tempPath))
                    {
                        if (!Directory.GetFiles(file).Contains("romfs")||!Directory.GetFiles(file).Contains("exefs"))
                        {
                            ImGui.OpenPopup("Error Importing Mod");
                            FileUtils.ReloadDirectory(tempPath);
                            return;
                        }
                    }

                    

                    string romfsPath = Directory.GetDirectories(filePick.Path, "romfs", SearchOption.AllDirectories)[0];
                    string exefsPath = Directory.GetDirectories(filePick.Path, "exefs", SearchOption.AllDirectories)[0];
                    


                    File.Copy(filePick.Path, cacheModsPath);

                    FileUtils.CopyDirectory(romfsPath, ryuModsPath + "\\romfs");
                    FileUtils.CopyDirectory(exefsPath, ryuModsPath + "\\exefs");
                    FileUtils.CreateDirectorySafe(cacheModsPath);
                }
            }

            if (ImGui.Button("Show popup"))
                ImGui.OpenPopup("Error Importing Mod");
            


            if (ImGui.BeginPopupModal("Error Importing Mod"))
            {
                ImGui.SetItemDefaultFocus();
                ImGui.Text("Could not import mod, be sure that the file includes either a romfs, or exefs.");

                ImGui.Separator();

                if (UIUtils.Button("OK"))
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }

            if (UIUtils.Button("Clear Files"))
            {
                foreach (var file in Directory.EnumerateFiles(cacheModsPath))
                {
                    File.Delete(file);
                    selectedFiles.Remove(file);
                }
            }

            if (UIUtils.Button("List Current Mods"))
            {
                foreach (var file in selectedFiles)
                {
                    Console.WriteLine(Path.GetFileName(file));
                }
            }
        }
    }
}

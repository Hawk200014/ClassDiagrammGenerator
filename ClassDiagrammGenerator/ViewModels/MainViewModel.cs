using Avalonia.Controls.Shapes;
using ClassDiagrammGenerator.Helper;
using ClassDiagrammGenerator.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using ClassDiagrammGenerator.Views; // Add this using directive at the top of the file

namespace ClassDiagrammGenerator.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string rootPath;

    [ObservableProperty]
    private string outputText;

    [ObservableProperty]
    private bool useGitIgnore;

    [ObservableProperty]
    private bool generateEnabled;

    public MainViewModel()
    {
        generateEnabled = false;
    }

    [RelayCommand]
    private async Task FolderSelect()
    {
        var storageProvider = MainWindow.Instance?.StorageProvider;
        if (storageProvider == null)
            return;

        var folder = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Root Directory",
            AllowMultiple = false
        });

        if (folder != null && folder.Any())
        {
            RootPath = folder.First().Path.LocalPath;
        }
        if(CheckInput())
        {
            GenerateEnabled = true;
        }
        else
        {
            GenerateEnabled = false;
        }
    }



    [RelayCommand]
    public void Generate()
    {
        List<string> files = GetFiles(RootPath, UseGitIgnore);

        List<ClassDiagrammGenerator.Models.ClassModel> classModels = new List<ClassDiagrammGenerator.Models.ClassModel>();
        List<ClassDiagrammGenerator.Models.InterfaceModel> interfaceModels = new List<ClassDiagrammGenerator.Models.InterfaceModel>();
        List<ClassDiagrammGenerator.Models.EnumModel> enumModels = new List<ClassDiagrammGenerator.Models.EnumModel>();

        string currentNamespace = string.Empty;

        foreach (var file in files)
        {
            if (!file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                continue;

            var lines = System.IO.File.ReadAllLines(file);

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                // Track namespace
                if (line.StartsWith("namespace "))
                {
                    currentNamespace = line.Substring("namespace ".Length).Trim();
                }

                // Parse enum
                if (line.Contains("enum "))
                {
                    enumModels.Add(ExtractorHelper.ExtractEnum(ref i, lines, currentNamespace));
                }

                if (line.Contains("class "))
                {
                    classModels.Add(ExtractorHelper.ExtractClass(ref i, lines, currentNamespace));
                }

                if (line.Contains("interface "))
                {
                    interfaceModels.Add(ExtractorHelper.ExtractInterface(ref i, lines, currentNamespace));
                }
            }
        }

        OutputText = MermaidDrawerHelper.GenerateMermaidDiagram(classModels, interfaceModels, enumModels);

    }


    private bool CheckInput()
    {
        if (string.IsNullOrWhiteSpace(RootPath))
        {
            return false;
        }

        if (!System.IO.Directory.Exists(RootPath))
        {
            return false;
        }

        return true;
    }


    private List<string> GetFiles(string path, bool useGitIgnore = false)
    {
        var files = new List<string>();
        var stack = new Stack<string>();
        stack.Push(path);

        while (stack.Count > 0)
        {
            var currentDir = stack.Pop();
            HashSet<string> ignorePatterns = new();

            // Load .gitignore if required
            if (useGitIgnore)
            {
                var gitIgnorePath = System.IO.Path.Combine(currentDir, ".gitignore");
                if (System.IO.File.Exists(gitIgnorePath))
                {
                    foreach (var line in System.IO.File.ReadAllLines(gitIgnorePath))
                    {
                        var trimmed = line.Trim();
                        if (!string.IsNullOrEmpty(trimmed) && !trimmed.StartsWith("#"))
                            ignorePatterns.Add(trimmed);
                    }
                }
            }

            // Get files
            foreach (var file in System.IO.Directory.GetFiles(currentDir))
            {
                var fileName = System.IO.Path.GetFileName(file);
                if (useGitIgnore && ignorePatterns.Any(pattern => IsMatch(fileName, pattern)))
                    continue;
                files.Add(file);
            }

            // Get directories
            foreach (var dir in System.IO.Directory.GetDirectories(currentDir))
            {
                var dirName = System.IO.Path.GetFileName(dir);
                if (useGitIgnore && ignorePatterns.Any(pattern => IsMatch(dirName + "/", pattern)))
                    continue;
                stack.Push(dir);
            }
        }

        return files;
    }

    // Simple pattern matcher for .gitignore (supports '*' wildcard)
    private bool IsMatch(string name, string pattern)
    {
        if (pattern == "/") return false;
        if (pattern.EndsWith("/")) // directory pattern
            pattern = pattern.TrimEnd('/');
        if (pattern == name) return true;
        if (pattern.Contains("*"))
        {
            var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern).Replace("\\*", ".*") + "$";
            return System.Text.RegularExpressions.Regex.IsMatch(name, regexPattern);
        }
        return false;
    }
}

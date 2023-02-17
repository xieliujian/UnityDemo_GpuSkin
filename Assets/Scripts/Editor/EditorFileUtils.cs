using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Animations;
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using Object = UnityEngine.Object;


public class EditorFileUtils
{
    public static List<string> GetDirSubFilePathList(string dirABSPath, bool isRecursive = true, string suffix = "")
    {
        List<string> pathList = new List<string>();
        DirectoryInfo di = new DirectoryInfo(dirABSPath);

        if (!di.Exists)
        {
            return pathList;
        }

        FileInfo[] files = di.GetFiles();
        foreach (FileInfo fi in files)
        {
            if (!string.IsNullOrEmpty(suffix))
            {
                if (!fi.FullName.EndsWith(suffix, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }
            }
            pathList.Add(fi.FullName);
        }

        if (isRecursive)
        {
            DirectoryInfo[] dirs = di.GetDirectories();
            foreach (DirectoryInfo d in dirs)
            {
                if (d.Name.Contains(".svn"))
                {
                    continue;
                }
                pathList.AddRange(GetDirSubFilePathList(d.FullName, isRecursive, suffix));
            }
        }

        return pathList;
    }

    public static List<string> GetDirSubDirNameList(string dirABSPath)
    {
        List<string> nameList = new List<string>();
        DirectoryInfo di = new DirectoryInfo(dirABSPath);

        if (!di.Exists)
        {
            return nameList;
        }

        DirectoryInfo[] dirs = di.GetDirectories();
        foreach (DirectoryInfo d in dirs)
        {
            if (d.Name.Contains(".svn"))
            {
                continue;
            }
            nameList.Add(d.Name);
        }

        return nameList;
    }

    public static List<string> GetDirSubFileNameList(string dirABSPath)
    {
        List<string> nameList = new List<string>();
        DirectoryInfo di = new DirectoryInfo(dirABSPath);

        if (!di.Exists)
        {
            return nameList;
        }

        FileInfo[] files = di.GetFiles();
        foreach (FileInfo f in files)
        {
            nameList.Add(f.Name);
        }

        return nameList;
    }

    public static string GetFileName(string absOrAssetsPath)
    {
        string name = absOrAssetsPath.Replace("\\", "/");
        int lastIndex = name.LastIndexOf("/");

        if (lastIndex >= 0)
        {
            return name.Substring(lastIndex + 1);
        }
        else
        {
            return name;
        }
    }

    public static string GetFileNameWithoutExtend(string absOrAssetsPath)
    {
        string fileName = GetFileName(absOrAssetsPath);
        int lastIndex = fileName.LastIndexOf(".");

        if (lastIndex >= 0)
        {
            return fileName.Substring(0, lastIndex);
        }
        else
        {
            return fileName;
        }
    }

    public static string GetFileExtendName(string absOrAssetsPath)
    {
        int lastIndex = absOrAssetsPath.LastIndexOf(".");

        if (lastIndex >= 0)
        {
            return absOrAssetsPath.Substring(lastIndex);
        }

        return string.Empty;
    }

    public static string GetDirPath(string absOrAssetsPath)
    {
        string name = absOrAssetsPath.Replace("\\", "/");
        int lastIndex = name.LastIndexOf("/");
        return name.Substring(0, lastIndex + 1);
    }

    public static string GetParentDirName(string absOrAssetsPath)
    {
        string parentDirPath = GetDirPath(absOrAssetsPath);
        if (parentDirPath.EndsWith("/"))
        {
            parentDirPath = parentDirPath.Substring(0, parentDirPath.Length - 1);
        }

        int lastPos = parentDirPath.LastIndexOf("/");
        if (lastPos >= 0)
        {
            return parentDirPath.Substring(lastPos + 1);
        }

        return string.Empty;
    }

    public static void SafeDeleteFile(string fileAbsPath)
    {
        if (File.Exists(fileAbsPath))
        {
            File.Delete(fileAbsPath);
        }
    }
    
    public static void SafeDeleteDir(string dirAbsPath)
    {
        if (Directory.Exists(dirAbsPath))
        {
            Directory.Delete(dirAbsPath, true);
        }
    }

    public static void SafeCreateDir(string dirAbsPath, bool recursive = false)
    {
        if (Directory.Exists(dirAbsPath))
        {
            return;
        }

        if (!recursive)
        {
            // 不递归创建，直接判断是否要创建新的
            if (!Directory.Exists(dirAbsPath))
            {
                Directory.CreateDirectory(dirAbsPath);
            }
            return;
        }

        dirAbsPath = dirAbsPath.Replace("/", "\\");
        string[] pathes = dirAbsPath.Split('\\');
        if (pathes.Length < 1)
        {
            return;
        }
        
        string path = pathes[0];
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        
        for (int i = 1; i < pathes.Length; i++)
        {
            path += "\\" + pathes[i];
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }

    public static void MoveFile(string srcAbsPath, string dstAbsPath, bool isDeleteDst = true)
    {
        if (isDeleteDst && File.Exists(dstAbsPath))
        {
            File.Delete(dstAbsPath);
        }

        File.Move(srcAbsPath, dstAbsPath);
    }

    public static void MoveAssetFile(string srcAssetPath, string dstAssetPath, bool isDeleteDst = true)
    {
        string srcAbsPath = EditorUtils.AssetsPath2ABSPath(srcAssetPath);
        string dstAbsPath = EditorUtils.AssetsPath2ABSPath(dstAssetPath);

        MoveFile(srcAbsPath, dstAbsPath, isDeleteDst);
    }

    public static void CopyDir(string srcDir, string dstDir)
    {
        DirectoryInfo source = new DirectoryInfo(srcDir);
        DirectoryInfo dst = new DirectoryInfo(dstDir);

        if (!source.Exists)
        {
            return;
        }

        if (!dst.Exists)
        {
            dst.Create();
        }

        FileInfo[] files = source.GetFiles();

        for (int i = 0; i < files.Length; i++)
        {
            File.Copy(files[i].FullName, dst.FullName + @"/" + files[i].Name, true);
        }

        DirectoryInfo[] dirs = source.GetDirectories();

        for (int j = 0; j < dirs.Length; j++)
        {
            CopyDir(dirs[j].FullName, dst.FullName + @"/" + dirs[j].Name);
        }
    }

    public static void FileContentReplace(string filePath, string oldStr, string newStr)
    {
        string oldFileContent = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
        string newFileContent = oldFileContent.Replace(oldStr, newStr);

        if (oldFileContent == newFileContent)
        {
            EditorUtils.BuildErrorLog(string.Format("FileContentReplace Failed path: {0} oldStr: {1} newStr: {2}", filePath, oldStr, newStr));
        }

        File.WriteAllText(filePath, newFileContent, new System.Text.UTF8Encoding(false));
    }
    
    public static string GetAssetPathFromFullPath(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
        {
            return string.Empty;
        }
        
        string assetsFolder = "Assets";
        int index = fullPath.IndexOf(assetsFolder);
        if (index < 0)
        {
            return fullPath;
        }
        return fullPath.Substring(index);

    }
}
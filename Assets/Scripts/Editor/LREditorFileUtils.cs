using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


public class LREditorFileUtils
{
    public static void TestPath()
    {
        //Path.GetFullPath(file)                    取全路径
        //Path.GetFileName(file)                    取文件名，包含扩展名
        //Path.GetFileNameWithoutExtension(file)    取文件名，不包含扩展名
        //Path.GetExtension(file)                   取扩展名
        //Path.GetDirectoryName(file)               取路径名
        //Path.GetPathRoot(file)                    取盘符
        //Path.Combine(file1,file2)                 合并2个路径

        string file = "App.config";                 // c:\users\qqq\source\repos\StudyDesignMode\StudyDesignMode\bin\Debug\App.config
        /* ==================================正常用法================================== */
        string fullpath = Path.GetFullPath(file);   // c:\users\qqq\source\repos\StudyDesignMode\StudyDesignMode\bin\Debug\App.config  // 取完整路径
        string name = Path.GetFileName(@"D:\1\2\App.config");                                   // App.config   // 取文件名
        string extension = Path.GetExtension(@"D:\1\2\App.config");                             // .config      // 取扩展名
        string nameWithoutExtension = Path.GetFileNameWithoutExtension(@"D:\1\2\App.config");   // App          // 取文件名 不带扩展名
        string dir = Path.GetDirectoryName(@"D:\1\2\App.config");                               // D:\1\2       // 取所在文件夹
        string root = Path.GetPathRoot(@"D:\1\2\App.config");                                   // D:\          // 取所在磁盘
        string combine = Path.Combine(@"1", @"2");                                              // 1\2          // 连接路径

        /* ==================================异常用法================================== */
        string fullpath3 = Path.GetFullPath(@"D:\\//\\//dfjk\\\\1///2\/\/\/\/\/3\");  //D:\dfjk\1\2\3\     忽略了多个 /  \ 为1个 \ 。保留了末尾的 \ 。
        //string fullpath1 = Path.GetFullPath(null);                // Exception    // 参数为null
        //string fullpath2 = Path.GetFullPath("");                  // Exception    // 参数为空字符串

        string name1 = Path.GetFileName(@"D:\1\2\App");             // App          // 无扩展名
        string name2 = Path.GetFileName(@"D:\1\2\.config");         //.config       // 只有扩展名
        string name3 = Path.GetFileName(@"D:\1\2\");                // ""           // 无文件名
        string name4 = Path.GetFileName(@"D:");                     // ""           // 只有盘符
        string name5 = Path.GetFileName(null);                      // null         // 参数为null
        string name6 = Path.GetFileName("");                        // ""           // 参数为""

        string extension1 = Path.GetExtension(@"D:\1\2\App");       // ""           // 无扩展名
        string extension2 = Path.GetExtension(@"D:\1\2\.config");   // .config      // 只有扩展名
        string extension3 = Path.GetExtension(@"D:\1\2\");          // ""           // 无文件名
        string extension4 = Path.GetExtension(@"D:");               // ""           // 只有盘符
        string extension5 = Path.GetExtension(null);                // null         // 参数为null
        string extension6 = Path.GetExtension("");                  // ""           // 参数为""

        //string combine1 = Path.Combine(null,null);                // Exception        // 参数为null
        //string combine2 = Path.Combine("", null);                 // Exception
        string combine3 = Path.Combine("", "");                     // ""               // 参数为""
        string combine4 = Path.Combine(@"///1\\\2\3", @"4");        // ///1\\\2\3\4     // 多个/ \
        string combine5 = Path.Combine(@"///1\\\2\3", @"/4");       // /4               // 第二个参数以/开头
        string combine6 = Path.Combine(@"///1\\\2\3", @"\4");       // \4               // 第二个参数以\开头
        string combine7 = Path.Combine(@"///1\\\2\3\\", @"4");      // ///1\\\2\3\\4    // 第一个参数以\结尾
        string combine8 = Path.Combine(@"///1\\\2\3/", @"4");       // ///1\\\2\3/4     // 第一个参数以/结尾
        string combine9 = Path.Combine(@"///1\\\2\3\", @"/4");      // /4               // 第二个参数以/开头
        string combine10 = Path.Combine(@"///1\\\2\3\", @"\4");     // \4               // 第二个参数以\开头

        string dir1 = Path.GetDirectoryName(@"D:\1\2\");                            // D:\1\2           // 取所在文件夹
        string dir2 = Path.GetDirectoryName(Path.Combine(@"D:\1", "Temp"));         // D:\1
        string dir3 = Path.GetDirectoryName(Path.Combine(@"D:\1\", "Temp"));        // D:\1

        string dir4 = Path.GetDirectoryName(@"\\192.168.0.2\share\file_1.txt");     // \\192.168.0.2\share
        string root1 = Path.GetPathRoot(@"\\192.168.0.2\share\file_1.txt");         // \\192.168.0.2\share
        string root2 = Path.GetPathRoot(@"192.168.0.2\share\file_1.txt");           // ""
        string root3 = Path.GetPathRoot(@"//192.168.0.2\share\file_1.txt");         // \\192.168.0.2\share
        string root4 = Path.GetPathRoot(@"//192.168.0.2\share\1\2file_1.txt");      // \\192.168.0.2\share
    }

    /// <summary>
    /// 获取工程名
    /// </summary>
    //[MenuItem("GetProjectName/GetProjectName")]
    public static string GetProjectName()
    {
#if ART_SCENE_PROJECT
        return "scene";
#endif
        
        var path = Application.dataPath;
        path = path.Substring(0, path.LastIndexOf("/"));
        var projectName = path.Substring(path.LastIndexOf("/") + 1);
        //Debug.LogError(projectName);
        return projectName;
    }

    /// <summary>
    /// 获取Assets目录下某一类资源路径包含Packages目录
    /// </summary>
    public static List<string> GetAssetsPaths<T>()
    {
        return GetAssetsPaths(typeof(T));
    }

    /// <summary>
    /// 获取Assets目录下某一类资源路径
    /// </summary>
    /// <param name="needResType">需要获取的资源类型 如 Texture2D ****不是importerType*** </param>
    /// <returns></returns>
    public static List<string> GetAssetsPaths(Type needResType)
    {
        List<string> assetsPathList = new List<string>();

        foreach (var assetsPath in AssetDatabase.GetAllAssetPaths())
        {
            if (!assetsPath.StartsWith("Assets"))
            {
                continue;
            }

            var resType = AssetDatabase.GetMainAssetTypeAtPath(assetsPath);

            if (resType == needResType)
            {
                assetsPathList.Add(assetsPath);
            }
        }

        return assetsPathList;
    }

    /// <summary>
    /// 通过后缀获取一个文件路径
    /// </summary>
    public static string FindFilePathBySuffix(string dirABSPath, string suffix, bool isRecursive = true)
    {
        if (string.IsNullOrEmpty(dirABSPath) || string.IsNullOrEmpty(suffix))
        {
            return string.Empty;
        }

        DirectoryInfo di = new DirectoryInfo(dirABSPath);

        if (!di.Exists)
        {
            return string.Empty;
        }

        FileInfo[] files = di.GetFiles();
        foreach (FileInfo fi in files)
        {
            if (fi.FullName.EndsWith(suffix, System.StringComparison.CurrentCultureIgnoreCase))
            {
                return fi.FullName;
            }
        }

        if (!isRecursive)
        {
            return string.Empty;
        }

        DirectoryInfo[] dirs = di.GetDirectories();

        foreach (DirectoryInfo d in dirs)
        {
            if (d.Name.Contains(".svn"))
            {
                continue;
            }

            var result = FindFilePathBySuffix(d.FullName, suffix, isRecursive);

            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }
        }

        return string.Empty;
    }

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
        return Path.GetExtension(absOrAssetsPath);
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

    public static void MoveFile(string srcAbsPath, string dstAbsPath, bool isDeleteDst = true)
    {
        //Debugger.LogDebugF("MoveFile {0} {1}", srcAbsPath, dstAbsPath);

        if (isDeleteDst && File.Exists(dstAbsPath))
        {
            File.Delete(dstAbsPath);
        }

        File.Move(srcAbsPath, dstAbsPath);
    }

    public static void MoveAssetFile(string srcAssetPath, string dstAssetPath, bool isDeleteDst = true)
    {
        string srcAbsPath = LREditorUtils.AssetsPath2ABSPath(srcAssetPath);
        string dstAbsPath = LREditorUtils.AssetsPath2ABSPath(dstAssetPath);

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
            LREditorUtils.BuildErrorLog(string.Format("FileContentReplace Failed path: {0} oldStr: {1} newStr: {2}", filePath, oldStr, newStr));
        }

        File.WriteAllText(filePath, newFileContent, new System.Text.UTF8Encoding(false));
    }
}
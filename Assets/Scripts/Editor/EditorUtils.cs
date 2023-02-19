using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using Object = UnityEngine.Object;

public class EditorUtils
{
    public static void BuildErrorLog(string errInfo)
    {
        Debug.LogError("\n----BuildErrorOutput----\n" + errInfo + "\n----EndBuildErrorOutput----\n");
    }

    public static string AssetsPath2ABSPath(string assetsPath)
    {
        string assetRootPath = System.IO.Path.GetFullPath(Application.dataPath);
        return assetRootPath.Substring(0, assetRootPath.Length - 6) + assetsPath;
    }

    public static void SafeRemoveAsset(string assetsPath)
    {
        Debug.Log("SafeRemoveAsset " + assetsPath);
        Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetsPath);

        if (obj != null)
        {
            AssetDatabase.DeleteAsset(assetsPath);
        }
    }

    static public bool DrawHeader(string text) { return DrawHeader(text, text, false, false); }

    static public bool DrawHeader(string text, string key, bool forceOn, bool minimalistic)
    {
        bool state = EditorPrefs.GetBool(key, true);

        if (!minimalistic) GUILayout.Space(3f);
        if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.BeginHorizontal();
        GUI.changed = false;

        if (minimalistic)
        {
            if (state) text = "\u25BC" + (char)0x200a + text;
            else text = "\u25BA" + (char)0x200a + text;

            GUILayout.BeginHorizontal();
            GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
            if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f))) state = !state;
            GUI.contentColor = Color.white;
            GUILayout.EndHorizontal();
        }
        else
        {
            text = "<b><size=11>" + text + "</size></b>";
            if (state) text = "\u25BC " + text;
            else text = "\u25BA " + text;
            if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
        }

        if (GUI.changed) EditorPrefs.SetBool(key, state);

        if (!minimalistic) GUILayout.Space(2f);
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        if (!forceOn && !state) GUILayout.Space(3f);
        return state;
    }

    public static string GetDirPath(string absOrAssetsPath)
    {
        string name = absOrAssetsPath.Replace("\\", "/");
        int lastIndex = name.LastIndexOf("/");
        return name.Substring(0, lastIndex + 1);
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
}

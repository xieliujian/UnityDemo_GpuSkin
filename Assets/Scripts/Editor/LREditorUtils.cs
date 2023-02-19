using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class LREditorUtils
{
    static List<Renderer> m_TempRenderList = new List<Renderer>();
    static List<MeshFilter> m_TempMeshFilterList = new List<MeshFilter>();
    static List<Texture> m_TempTextureList = new List<Texture>();

    //选中文件夹无效:需要选中某个文件才行
    public static string CurrentSelectPath
    {
        get
        {
            if (Selection.activeObject == null)
            {
                return null;
            }
            return AssetDatabase.GetAssetPath(Selection.activeObject);
        }
    }

    public static string[] CurrentSelectPaths
    {
        get
        {
            if (Selection.assetGUIDs == null)
            {
                return null;
            }
            string[] paths = new string[Selection.assetGUIDs.Length];
            for (int i = 0; i < paths.Length; ++i)
            {
                paths[i] = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[i]);
            }
            return paths;
        }
    }

    /// <summary>
    /// 获取资源大小
    /// </summary>
    public static long GetResSize<T>(string assetsPath) where T : Object
    {
        long resSize = 0;
        var data = AssetDatabase.LoadAssetAtPath<T>(assetsPath);

        if (data != null)
        {
            resSize = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(data);
        }

        return resSize;
    }

    /// <summary>
    /// 获取资源大小文本
    /// </summary>
    public static string GetResSizeStr<T>(string assetsPath) where T : Object
    {
        return EditorUtility.FormatBytes((long)GetResSize<T>(assetsPath));
    }

    public static string AssetsPath2ABSPath(string assetsPath)
    {
        string assetRootPath = System.IO.Path.GetFullPath(Application.dataPath);
        return assetRootPath.Substring(0, assetRootPath.Length - 6) + assetsPath;
    }

    public static List<string> AssetsPathList2ABSPathList(List<string> assetPathList)
    {
        List<string> absPathList = new List<string>();

        for (int i = 0; i < assetPathList.Count; ++i)
        {
            absPathList.Add(AssetsPath2ABSPath(assetPathList[i]));
        }

        return absPathList;
    }

    public static string ABSPath2AssetsPath(string absPath)
    {
        string assetRootPath = System.IO.Path.GetFullPath(Application.dataPath);
        return "Assets" + System.IO.Path.GetFullPath(absPath).Substring(assetRootPath.Length).Replace("\\", "/");
    }

    public static List<string> ABSPathList2AssetsPathList(List<string> absPathList)
    {
        List<string> assetPathList = new List<string>();

        for (int i = 0; i < absPathList.Count; ++i)
        {
            assetPathList.Add(ABSPath2AssetsPath(absPathList[i]));
        }

        return assetPathList;
    }

    public static bool ExcuteCmd(string toolName, string args, bool isThrowExcpetion = true)
    {
        Debug.LogError("ExcuteCmd: " + toolName + " " + args);
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        process.StartInfo.FileName = toolName;
        process.StartInfo.Arguments = args;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.Start();
        OuputProcessLog(process, isThrowExcpetion);
        return true;
    }

    public static void OuputProcessLog(System.Diagnostics.Process p, bool isThrowExcpetion)
    {
        string standardError = string.Empty;
        p.BeginErrorReadLine();

        p.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler((sender, outLine) =>
        {
            standardError += outLine.Data;
        });

        string standardOutput = string.Empty;
        p.BeginOutputReadLine();
        p.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler((sender, outLine) =>
        {
            standardOutput += outLine.Data;
        });

        p.WaitForExit();
        p.Close();

        if (!string.IsNullOrEmpty(standardOutput))
        {
            Debug.Log(standardOutput);
        }

        if (standardError.Length > 0)
        {
            if (isThrowExcpetion)
            {
                Debug.LogError(standardError);
                throw new Exception(standardError);
            }
            else
            {
                Debug.Log(standardError);
            }
        }
    }

    public static Dictionary<string, string> ParseArgs(string argString)
    {
        int curPos = argString.IndexOf('-');
        Dictionary<string, string> result = new Dictionary<string, string>();

        while (curPos != -1 && curPos < argString.Length)
        {
            int nextPos = argString.IndexOf('-', curPos + 1);
            string item = string.Empty;

            if (nextPos != -1)
            {
                item = argString.Substring(curPos + 1, nextPos - curPos - 1);
            }
            else
            {
                item = argString.Substring(curPos + 1, argString.Length - curPos - 1);
            }

            item = StringTrim(item);
            int splitPos = item.IndexOf(' ');

            if (splitPos == -1)
            {
                string key = StringTrim(item);
                result[key] = "";
            }
            else
            {
                string key = StringTrim(item.Substring(0, splitPos));
                string value = StringTrim(item.Substring(splitPos + 1, item.Length - splitPos - 1));
                result[key] = value;
            }

            curPos = nextPos;
        }

        return result;
    }

    public static string GetFileMD5Value(string absPath)
    {
        if (!File.Exists(absPath))
            return "";

        MD5CryptoServiceProvider md5CSP = new MD5CryptoServiceProvider();
        FileStream file = new FileStream(absPath, System.IO.FileMode.Open);
        byte[] retVal = md5CSP.ComputeHash(file);
        file.Close();
        string result = "";

        for (int i = 0; i < retVal.Length; i++)
        {
            result += retVal[i].ToString("x2");
        }

        return result;
    }

    public static string GetStrMD5Value(string str)
    {
        MD5CryptoServiceProvider md5CSP = new MD5CryptoServiceProvider();
        byte[] retVal = md5CSP.ComputeHash(Encoding.UTF8.GetBytes(str));
        string retStr = "";

        for (int i = 0; i < retVal.Length; i++)
        {
            retStr += retVal[i].ToString("x2");
        }

        return retStr;
    }

    public static List<Object> GetDirSubAssetsList(string dirAssetsPath, bool isRecursive = true, string suffix = "", bool isLoadAll = false)
    {
        string dirABSPath = ABSPath2AssetsPath(dirAssetsPath);
        List<string> assetsABSPathList = LREditorFileUtils.GetDirSubFilePathList(dirABSPath, isRecursive, suffix);
        List<Object> resultObjectList = new List<Object>();

        for (int i = 0; i < assetsABSPathList.Count; ++i)
        {
            if (isLoadAll)
            {
                Object[] objs = AssetDatabase.LoadAllAssetsAtPath(ABSPath2AssetsPath(assetsABSPathList[i]));
                resultObjectList.AddRange(objs);
            }
            else
            {
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(ABSPath2AssetsPath(assetsABSPathList[i]));
                resultObjectList.Add(obj);
            }
        }

        return resultObjectList;
    }

    public static List<T> GetDirSubAssetsList<T>(string dirAssetsPath, bool isRecursive = true, string suffix = "", bool isLoadAll = false) where T : Object
    {
        List<T> result = new List<T>();
        List<Object> objectList = GetDirSubAssetsList(dirAssetsPath, isRecursive, suffix, isLoadAll);

        for (int i = 0; i < objectList.Count; ++i)
        {
            if (objectList[i] is T)
            {
                result.Add(objectList[i] as T);
            }
        }

        return result;
    }

    public static string GetSelectedDirAssetsPath()
    {
        string path = string.Empty;

        foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
                break;
            }
        }

        if (path.EndsWith("\\") || path.EndsWith("/"))
        {
            return path;
        }

        if (path.Length == 0)
        {
            return path;
        }

        return path + "/";
    }

    public static string StringTrim(string str, params char[] trimer)
    {
        int startIndex = 0;
        int endIndex = str.Length;

        for (int i = 0; i < str.Length; ++i)
        {
            if (!IsInCharArray(trimer, str[i]))
            {
                startIndex = i;
                break;
            }
        }

        for (int i = str.Length - 1; i >= 0; --i)
        {
            if (!IsInCharArray(trimer, str[i]))
            {
                endIndex = i;
                break;
            }
        }

        if (startIndex == 0 && endIndex == str.Length)
        {
            return string.Empty;
        }
        return str.Substring(startIndex, endIndex - startIndex + 1);
    }

    public static string StringTrim(string str)
    {
        return StringTrim(str, new char[] { ' ', '\t' });
    }

    static bool IsInCharArray(char[] array, char c)
    {
        for (int i = 0; i < array.Length; ++i)
        {
            if (array[i] == c)
            {
                return true;
            }
        }
        return false;
    }

    public static void ClearAssetBundlesName()
    {
        int length = AssetDatabase.GetAllAssetBundleNames().Length;
        string[] oldAssetBundleNames = new string[length];

        for (int i = 0; i < length; i++)
        {
            oldAssetBundleNames[i] = AssetDatabase.GetAllAssetBundleNames()[i];
        }

        for (int j = 0; j < oldAssetBundleNames.Length; j++)
        {
            AssetDatabase.RemoveAssetBundleName(oldAssetBundleNames[j], true);
        }

        length = AssetDatabase.GetAllAssetBundleNames().Length;
        AssetDatabase.SaveAssets();
    }

    public static bool IsValidAssetBundleName(string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName))
        {
            return false;
        }

        for (int i = 0; i < bundleName.Length; ++i)
        {
            char c = bundleName[i];
            if (!(char.IsLetterOrDigit(c) || c == '_' || c == '/'))
            {
                return false;
            }

            if (char.IsLetter(c) && !char.IsLower(c))
            {
                return false;
            }
        }

        return true;
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

    public static void Abort(string errMsg)
    {
        Debug.LogError("BatchMode Abort Exit " + errMsg);
        System.Threading.Thread.CurrentThread.Abort();
        System.Diagnostics.Process.GetCurrentProcess().Kill();

        System.Environment.ExitCode = 1;
        System.Environment.Exit(1);

        EditorApplication.Exit(1);
    }

    public static void GetComponentsRecursive<T>(GameObject obj, List<T> result) where T : Component
    {
        if (obj == null)
        {
            return;
        }

        Component[] components = obj.GetComponents(typeof(Component));
        foreach (Component component in components)
        {
            if ((component as T) != null)
            {
                result.Add((T)component);
            }
        }

        for (int i = 0; i < obj.transform.childCount; ++i)
        {
            GetComponentsRecursive<T>(obj.transform.GetChild(i).gameObject, result);
        }
    }

    public static void GetComponentsRecursive(string typename, GameObject obj, List<Component> result)
    {
        if (string.IsNullOrEmpty(typename) || obj == null || result == null)
        {
            return;
        }

        string typeString;
        Component component;
        Component[] components = obj.GetComponents(typeof(Component));

        for (int i = 0; i < components.Length; ++i)
        {
            component = components[i];
            if (component == null)
            {
                continue;
            }

            typeString = component.GetType().ToString();
            if (typeString.Contains(typename))
            {
                result.Add(component);
            }
        }

        for (int i = 0; i < obj.transform.childCount; ++i)
        {
            GetComponentsRecursive(typename, obj.transform.GetChild(i).gameObject, result);
        }
    }

    public static void GetChildGameObjectRecursive(GameObject obj, List<GameObject> result)
    {
        result.Add(obj);

        for (int i = 0; i < obj.transform.childCount; ++i)
        {
            GetChildGameObjectRecursive(obj.transform.GetChild(i).gameObject, result);
        }
    }

    public static bool IsGameObjectHasStandardShader(string assetPath, bool logError = true)
    {
        GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (go == null)
        {
            return false;
        }

        List<Renderer> renderers = new List<Renderer>();
        GetComponentsRecursive<Renderer>(go, renderers);
        bool hasStnadardShader = false;

        foreach (Renderer renderer in renderers)
        {
            foreach (Material m in renderer.sharedMaterials)
            {
                if (m != null && m.shader != null)
                {
                    if (m.shader.name.Contains("Standard"))
                    {
                        hasStnadardShader = true;
                    }
                }
            }
        }

        if (!hasStnadardShader)
        {
            return false;
        }

        Debug.LogError("IsGameObjectHasStandardShader Reimport " + assetPath);

        AssetImporter ai = AssetImporter.GetAtPath(assetPath);
        EditorUtility.SetDirty(ai);
        ai.SaveAndReimport();

        go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (go == null)
        {
            return false;
        }

        hasStnadardShader = false;
        renderers.Clear();
        GetComponentsRecursive<Renderer>(go, renderers);

        foreach (Renderer renderer in renderers)
        {
            foreach (Material m in renderer.sharedMaterials)
            {
                if (m != null && m.shader != null)
                {
                    if (m.shader.name.Contains("Standard"))
                    {
                        hasStnadardShader = true;
                        if (logError)
                        {
                            BuildErrorLog(string.Format("path:{0} gameobject:{1} mat:{2} use Standard Shader", assetPath, renderer.gameObject.name, m.name));
                        }
                    }
                }
            }
        }

        return false;
    }

    public static bool IsMatHasStandardShader(string assetPath, bool logError = true)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        if (mat == null || mat.shader == null)
        {
            return false;
        }

        if (mat.shader.name.Contains("Standard"))
        {
            if (logError)
            {
                BuildErrorLog(string.Format("path:{0} mat:{1} use Standard Shader", assetPath, mat.name));
            }

            return true;
        }

        return false;
    }

    public static string[] GetDepends(string assetPath)
    {
        string[] assetPaths = new string[1];
        assetPaths[0] = assetPath;
        string[] depends = AssetDatabase.GetDependencies(assetPaths, true);

        foreach (string depend in depends)
        {
            if (depend.EndsWith(".mat"))
            {
                IsMatHasStandardShader(depend);
            }

            if (depend.EndsWith(".prefab") || depend.ToLower().EndsWith(".fbx"))
            {
                IsGameObjectHasStandardShader(depend);
            }
        }

        return depends;
    }

    public static List<string> GetDependsExclude(List<string> assetPaths)
    {
        string[] depends = AssetDatabase.GetDependencies(assetPaths.ToArray(), true);
        List<string> result = new List<string>();

        foreach (string depend in depends)
        {
            if (depend.Contains("/LingRen/") || depend.ToLower().EndsWith(".cs"))
            {
                continue;
            }

            if (assetPaths.Contains(depend))
            {
                continue;
            }

            if (depend.EndsWith(".mat"))
            {
                IsMatHasStandardShader(depend);
            }
            else if (depend.ToLower().EndsWith(".prefab") || depend.ToLower().EndsWith(".fbx"))
            {
                IsGameObjectHasStandardShader(depend);
            }

            result.Add(depend);
        }

        return result;
    }

    public static string[] RemoveLingRenDepends(string[] depends)
    {
        List<string> result = new List<string>();

        foreach (string str in depends)
        {
            if (!str.StartsWith("Assets/LingRen/"))
            {
                result.Add(str);
            }
        }

        return result.ToArray();
    }

    public static string[] RemoveScriptDepends(string[] depends)
    {
        List<string> result = new List<string>();

        foreach (string str in depends)
        {
            if (!str.ToLower().EndsWith(".cs"))
            {
                result.Add(str);
            }
        }

        return result.ToArray();
    }

    public static void CopyCompoment<T>(GameObject src, GameObject dst) where T : Component
    {
        T component = src.GetComponent<T>();

        if (component == null)
        {
            return;
        }

        UnityEditorInternal.ComponentUtility.CopyComponent(component);
        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(dst);
    }

    public static void GetChildObjDict(GameObject root, string rootPath, Dictionary<string, GameObject> resultDict)
    {
        if (root == null)
        {
            return;
        }

        resultDict[rootPath + root.name] = root;
        string newRootPath = rootPath + root.name + "/";

        for (int i = 0; i < root.transform.childCount; ++i)
        {
            GetChildObjDict(root.transform.GetChild(i).gameObject, newRootPath, resultDict);
        }
    }

    public static void BuildErrorLog(string errInfo)
    {
        Debug.LogError("\n----BuildErrorOutput----\n" + errInfo + "\n----EndBuildErrorOutput----\n");
    }

    public static void ImportDirAsset(string dirAbsPath)
    {
        string[] files = Directory.GetFiles(dirAbsPath, "*", SearchOption.AllDirectories);

        foreach (string file in files)
        {
            if (file.Contains(".svn") || file.Contains(".meta"))
            {
                continue;
            }

            AssetDatabase.ImportAsset(ABSPath2AssetsPath(file), ImportAssetOptions.ForceSynchronousImport);
        }
    }

    public static long GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds);
    }

    public static void SaveRenderTextureToPNG(RenderTexture rt, string absPath)
    {
        RenderTexture tempRt = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D png = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        png.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        byte[] bytes = png.EncodeToPNG();
        FileStream file = File.Open(absPath, FileMode.Create);
        BinaryWriter writer = new BinaryWriter(file);
        writer.Write(bytes);
        file.Close();
        Texture2D.DestroyImmediate(png);
        RenderTexture.active = tempRt;
    }

    public static void SaveTexture2DToPNG(Texture2D texture, string absPath)
    {
        byte[] bytes = texture.EncodeToPNG();
        FileStream file = File.Open(absPath, FileMode.Create);
        BinaryWriter writer = new BinaryWriter(file);
        writer.Write(bytes);
        file.Close();
    }

    public static string GetCurSceneName()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        string name = LREditorFileUtils.GetFileNameWithoutExtend(scene.path);
        return name;
    }

    public static string GetCurSceneParentDirAbsPath()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        string path = string.Empty;
        path = AssetsPath2ABSPath(scene.path);
        path = LREditorFileUtils.GetDirPath(path);
        return path;
    }

    public static void CreateCurSceneMapDataDir()
    {
        string sceneName = GetCurSceneName();
        string sceneAbsDir = GetCurSceneParentDirAbsPath();
        string mapDataDir = sceneAbsDir + "map/";
        string mapDir = mapDataDir + sceneName + "/";
        if (!Directory.Exists(mapDataDir))
        {
            Directory.CreateDirectory(mapDataDir);
        }
        if (!Directory.Exists(mapDir))
        {
            Directory.CreateDirectory(mapDir);
        }
    }

    public static void CreateSceneMapDataDir(string sceneName)
    {
        string sceneAbsDir = AssetsPath2ABSPath("Assets/Resources/scene/" + sceneName + "/");
        string mapDataDir = sceneAbsDir + "map/";
        string mapDir = mapDataDir + sceneName + "/";
        if (!Directory.Exists(mapDataDir))
        {
            Directory.CreateDirectory(mapDataDir);
        }
        if (!Directory.Exists(mapDir))
        {
            Directory.CreateDirectory(mapDir);
        }
    }

    public static string GetCurSceneMapDataDirAbsPath()
    {
        string sceneName = GetCurSceneName();
        string sceneAbsDir = GetCurSceneParentDirAbsPath();
        string mapDataDir = sceneAbsDir + "map/";
        string mapDir = mapDataDir + sceneName + "/";
        return mapDir;
    }

    public static string GetSceneMapDataDirAbsPath(string sceneName)
    {
        string sceneAbsDir = AssetsPath2ABSPath("Assets/Resources/scene/" + sceneName + "/");
        string mapDataDir = sceneAbsDir + "map/";
        string mapDir = mapDataDir + sceneName + "/";
        return mapDir;
    }

    public static void SetTextureImporterMaxSizeAndFormat(TextureImporter ti, int maxSize, TextureImporterFormat standaloneFormat,
        TextureImporterFormat iPhoneFormat, TextureImporterFormat androidFormat)
    {
        TextureImporterPlatformSettings settingStandalone = ti.GetPlatformTextureSettings("Standalone");
        TextureImporterPlatformSettings settingiPhone = ti.GetPlatformTextureSettings("iPhone");
        TextureImporterPlatformSettings settingAndroid = ti.GetPlatformTextureSettings("Android");

        settingStandalone.overridden = true;
        settingiPhone.overridden = true;
        settingAndroid.overridden = true;

        settingStandalone.maxTextureSize = maxSize;
        settingiPhone.maxTextureSize = maxSize;
        settingAndroid.maxTextureSize = maxSize;

        settingStandalone.format = standaloneFormat;
        settingiPhone.format = iPhoneFormat;
        settingAndroid.format = androidFormat;

        ti.SetPlatformTextureSettings(settingStandalone);
        ti.SetPlatformTextureSettings(settingiPhone);
        ti.SetPlatformTextureSettings(settingAndroid);
    }

    public static void SetTextureImporterMaxSize(TextureImporter ti, int maxSize)
    {
        TextureImporterPlatformSettings settingStandalone = ti.GetPlatformTextureSettings("Standalone");
        TextureImporterPlatformSettings settingiPhone = ti.GetPlatformTextureSettings("iPhone");
        TextureImporterPlatformSettings settingAndroid = ti.GetPlatformTextureSettings("Android");

        settingStandalone.overridden = true;
        settingiPhone.overridden = true;
        settingAndroid.overridden = true;

        settingStandalone.maxTextureSize = maxSize;
        settingiPhone.maxTextureSize = maxSize;
        settingAndroid.maxTextureSize = maxSize;

        ti.SetPlatformTextureSettings(settingStandalone);
        ti.SetPlatformTextureSettings(settingiPhone);
        ti.SetPlatformTextureSettings(settingAndroid);
    }

    public static void LineLog(string str)
    {
        string[] lines = str.Split('\n');
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < lines.Length; ++i)
        {
            sb.AppendLine(lines[i]);
            if (i % 150 == 0 && i != 0)
            {
                Debug.LogError(sb.ToString());
                sb.Length = 0;
            }
        }

        if (sb.Length != 0)
        {
            Debug.LogError(sb.ToString());
        }
    }

    public static void LineLog(List<string> lines)
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < lines.Count; ++i)
        {
            sb.AppendLine(lines[i]);
            if (i % 150 == 0 && i != 0)
            {
                Debug.LogError(sb.ToString());
                sb.Length = 0;
            }
        }

        if (sb.Length != 0)
        {
            Debug.LogError(sb.ToString());
        }
    }

    public static void PostProcessFBXGoSetMat(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; ++i)
        {
            Renderer renderer = renderers[i];
            Material[] mats = renderer.sharedMaterials;

            for (int j = 0; j < mats.Length; ++j)
            {
                //Debug.LogError("PostProcessFBXGoSetMat " + renderer.name + " mat " + mats[j].name);
                if (mats[j] != null && mats[j].name == "Default-Material")
                {
                    mats[j] = AssetDatabase.LoadAssetAtPath<Material>("Assets/LingRen/Shader/Res/FBXDefaultMat.mat");
                }

                ReplaceStandardShader(mats[j]);
            }

            renderer.sharedMaterials = mats;
        }
    }

    public static void ReplaceStandardShaderMaterials(string assetFolder)
    {
        if (!Directory.Exists(assetFolder))
        {
            return;
        }

        string absPath = AssetsPath2ABSPath(assetFolder);

        var paths = LREditorFileUtils.GetDirSubFilePathList(absPath, true, ".mat");
        foreach (var path in paths)
        {
            string assetPath = ABSPath2AssetsPath(path);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            ReplaceStandardShader(mat);
        }
    }

    public static void ReplaceStandardShader(Material mat)
    {
        if (!mat || !mat.shader)
        {
            return;
        }

        if (mat.shader.name.Contains("Standard"))
        {
            mat.shader = Shader.Find("Lingren/Scene/MobileDiffuse");
        }
    }
    public static int GetTextureOriginalMaxSize(TextureImporter ti)
    {
        int width = 0;
        int height = 0;

        GetTextureOriginalSize(ti, out width, out height);

        return Mathf.Max(width, height);
    }

    public static void GetTextureOriginalSize(TextureImporter ti, out int width, out int height)
    {
        if (ti == null)
        {
            width = 0;
            height = 0;
            return;
        }

        object[] args = new object[2] { 0, 0 };
        MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
        mi.Invoke(ti, args);

        width = (int)args[0];
        height = (int)args[1];
    }

    public static void SetLayerRecursively(Transform parent, int layer)
    {
        if (null == parent)
        {
            return;
        }

        parent.gameObject.layer = layer;

        for (int i = 0; i < parent.childCount; ++i)
        {
            Transform child = parent.GetChild(i);
            SetLayerRecursively(child, layer);
        }
    }

    public static bool CheckIsHasDefaultAsset(string assetPath, StringBuilder errorMsg = null)
    {
        Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        if (obj == null)
        {
            return true;
        }

        if (obj is GameObject)
        {
            bool hasDefalutAsset = IsGameObjectHasUnityDefaultAsset(assetPath, obj as GameObject, errorMsg);
            string[] depends = AssetDatabase.GetDependencies(assetPath, true);
            return hasDefalutAsset;
        }
        else if (obj is Material)
        {
            return IsMatrialHasUnityDefaultAsset(obj as Material, assetPath, errorMsg);
        }

        if (IsObjectHasUnityDefaultAsset(obj))
        {
            if (errorMsg != null)
            {
                errorMsg.Append(assetPath + " has default asset\n");
            }

            return true;
        }

        return false;
    }

    static bool IsObjectHasUnityDefaultAsset(Object obj)
    {
        if (obj == null)
        {
            return false;
        }

        string path = AssetDatabase.GetAssetPath(obj);
        return path == "Resources/unity_builtin_extra" || path == "Library/unity default resources";
    }

    static bool IsMatrialHasUnityDefaultAsset(Material mat, string name, StringBuilder sb)
    {
        if (mat == null)
        {
            return false;
        }

        bool hasDefaultAsset = false;

        if (IsObjectHasUnityDefaultAsset(mat))
        {
            hasDefaultAsset = true;
            if (sb != null)
            {
                sb.Append(name + " has default mat " + mat.name + "\n");
            }
        }

        if (IsObjectHasUnityDefaultAsset(mat.shader))
        {
            hasDefaultAsset = true;
            if (sb != null)
            {
                sb.Append(name + " has default shader " + mat.shader.name + "\n");
            }
        }

        List<Texture> dependTextures = GetMaterialTextures(mat);
        foreach (Texture tex in dependTextures)
        {
            if (IsObjectHasUnityDefaultAsset(tex))
            {
                hasDefaultAsset = true;

                if (sb != null)
                {
                    sb.Append(name + " has default texture " + tex.name + "\n");
                }
            }
        }

        return hasDefaultAsset;
    }

    static List<Texture> GetMaterialTextures(Material mat)
    {
        m_TempTextureList.Clear();

        if (mat == null)
        {
            return m_TempTextureList;
        }

        Shader shader = mat.shader;
        for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
        {
            if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
            {
                Texture texture = mat.GetTexture(ShaderUtil.GetPropertyName(shader, i));
                m_TempTextureList.Add(texture);
            }
        }

        return m_TempTextureList;
    }

    static bool IsGameObjectHasUnityDefaultAsset(string assetPath, GameObject go, StringBuilder errorMsg)
    {
        m_TempRenderList.Clear();
        m_TempMeshFilterList.Clear();

        GetComponentsRecursive<Renderer>(go, m_TempRenderList);
        GetComponentsRecursive<MeshFilter>(go, m_TempMeshFilterList);

        bool hasDefaultAsset = false;

        foreach (Renderer renderer in m_TempRenderList)
        {
            foreach (Material mat in renderer.sharedMaterials)
            {
                if (IsMatrialHasUnityDefaultAsset(mat, assetPath + " renderer " + renderer.name, errorMsg))
                {
                    hasDefaultAsset = true;
                }
            }
        }

        foreach (MeshFilter meshFilter in m_TempMeshFilterList)
        {
            if (IsObjectHasUnityDefaultAsset(meshFilter.sharedMesh))
            {
                if (errorMsg != null)
                {
                    errorMsg.Append(assetPath + " meshfilter " + meshFilter.name + " has default mesh\n");
                }

                hasDefaultAsset = true;
            }
        }

        return hasDefaultAsset;
    }

    public static void ForceRefreshGUID(string metaFilePath)
    {
        if (!File.Exists(metaFilePath))
        {
            return;
        }

        string[] lines = File.ReadAllLines(metaFilePath);
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < lines.Length; ++i)
        {
            if (lines[i].StartsWith("guid: "))
            {
                string[] items = lines[i].Split(' ');

                if (items.Length == 2)
                {
                    lines[i] = "guid: " + items[1].Substring(0, 16) + "0000000000000000";
                }
            }
        }

        File.Delete(metaFilePath);
        File.WriteAllLines(metaFilePath, lines);
    }

    public static string StringList2String(List<string> strList, string spliter = "\n")
    {
        if (strList == null)
        {
            return string.Empty;
        }

        StringBuilder sb = new StringBuilder();

        foreach (string str in strList)
        {
            sb.Append(str);
            sb.Append(spliter);
        }

        return sb.ToString();
    }

    public static bool IsInStringArray(string[] array, string val)
    {
        for (int i = 0; i < array.Length; ++i)
        {
            if (array[i] == val)
            {
                return true;
            }
        }

        return false;
    }

    public static double CurrentTime()
    {
        return (DateTime.Now.Ticks - 621355968000000000) / 10000000.0;
    }

    /// <summary>
    /// 获得数据方法
    /// </summary>
    public static T Get<T>(this IList<T> dataList, int index, T defaultData = default(T))
    {
        if (index < 0 || dataList == null || index >= dataList.Count)
        {
            return defaultData;
        }

        return dataList[index];
    }

    /// <summary>
    /// 获取泛型类的所有实现(不包含自己)
    /// </summary>
    /// <param name="generic">泛型接口类型，传入 typeof(AAA<>)</param>
    /// <param name="result"> 泛型类的所有实现 </param>
    internal static void GetGenericImpAll([NotNull] this Type generic, List<Type> result)
    {
        if (result == null)
        {
            return;
        }

        result.Clear();

        if (generic == null)
        {
            throw new ArgumentNullException(nameof(generic));
        }

        Module[] modules = generic.Assembly.GetModules();

        foreach (Module model in modules)
        {
            Type[] types = model.GetTypes();

            foreach (Type type in types)
            {
                if (type != generic && type.HasImplementedRawGeneric(generic))
                {
                    result.Add(type);
                }
            }
        }
    }

    /// <summary>
    /// 判断指定的类型 <paramref name="type"/> 是否是指定泛型类型的子类型，或实现了指定泛型接口。
    /// </summary>
    /// <param name="type">需要测试的类型。</param>
    /// <param name="generic">泛型接口类型，传入 typeof(AAA<>)</param>
    /// <returns>如果是泛型接口的子类型，则返回 true，否则返回 false。</returns>
    internal static bool HasImplementedRawGeneric([NotNull] this Type type, [NotNull] Type generic)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (generic == null)
        {
            throw new ArgumentNullException(nameof(generic));
        }

        // 检查接口。
        var isTheRawGenericType = type.GetInterfaces().Any(IsTheRawGenericType);

        if (isTheRawGenericType)
        {
            return true;
        }

        // 检查类型。
        while (type != null && type != typeof(object))
        {
            isTheRawGenericType = IsTheRawGenericType(type);

            if (isTheRawGenericType)
            {
                return true;
            }

            type = type.BaseType;
        }

        // 没有找到任何匹配的接口或类型。
        return false;

        // 测试某个类型是否是指定的原始接口。
        bool IsTheRawGenericType(Type test)
        {
            return generic == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);
        }
    }
}
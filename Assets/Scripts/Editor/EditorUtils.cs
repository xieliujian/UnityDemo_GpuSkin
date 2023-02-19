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
    static List<Renderer> m_TempRenderList = new List<Renderer>();
    static List<MeshFilter> m_TempMeshFilterList = new List<MeshFilter>();
    static List<Texture> m_TempTextureList = new List<Texture>();
    static MD5CryptoServiceProvider m_MD5CSP = new MD5CryptoServiceProvider();
    
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
            absPathList.Add(EditorUtils.AssetsPath2ABSPath(assetPathList[i]));
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
            assetPathList.Add(EditorUtils.ABSPath2AssetsPath(absPathList[i]));
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

            item = EditorUtils.StringTrim(item);
            int splitPos = item.IndexOf(' ');

            if (splitPos == -1)
            {
                string key = EditorUtils.StringTrim(item);
                result[key] = "";
            }
            else
            {
                string key = EditorUtils.StringTrim(item.Substring(0, splitPos));
                string value = EditorUtils.StringTrim(item.Substring(splitPos + 1, item.Length - splitPos - 1));
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

        FileStream file = new FileStream(absPath, System.IO.FileMode.Open);
        byte[] retVal = m_MD5CSP.ComputeHash(file);
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
        byte[] retVal = m_MD5CSP.ComputeHash(Encoding.UTF8.GetBytes(str));
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
        List<string> assetsABSPathList = EditorFileUtils.GetDirSubFilePathList(dirABSPath, isRecursive, suffix);
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

    public static List<string> AssetDataBaseFindAsset(string filter, string assetPath)
    {
        string[] guids = AssetDatabase.FindAssets(filter, new string[] {assetPath});
        List<string> assetPathList = new List<string>();

        foreach (var guid in guids)
        {
            assetPathList.Add(AssetDatabase.GUIDToAssetPath(guid));
        }

        return assetPathList;
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

    public static bool SetAssetBundleName(string assetsPath, string bundleName, ref AssetBundleBuild build)
    {
        if (!IsValidAssetBundleName(bundleName))
        {
            BuildErrorLog(string.Format("InValid bundleName {0}, assetsPath {1}", bundleName, assetsPath));
            return false;
        }

        build = EditorUtils.CreateAssetBundleBuild(assetsPath, bundleName);
        //Builder.AddBuild(build);
        return true;
    }

    public static bool SetAssetBundleName(string assetsPath, string bundleName)
    {
        if (!IsValidAssetBundleName(bundleName))
        {
            BuildErrorLog(string.Format("InValid bundleName {0}, assetsPath {1}", bundleName, assetsPath));
            return false;
        }

        //Builder.AddBuild(EditorUtils.CreateAssetBundleBuild(assetsPath, bundleName));
        return true;
    }

    public static void SetDirAssetBundleName(string dirAssetPath, string bundleName)
    {
        List<string> absPathList = EditorFileUtils.GetDirSubFilePathList(EditorUtils.AssetsPath2ABSPath(dirAssetPath));

        foreach (string absPath in absPathList)
        {
            string assetPath = EditorUtils.ABSPath2AssetsPath(absPath);
            if (AssetDatabase.LoadAssetAtPath<Object>(assetPath) != null)
            {
                SetAssetBundleName(assetPath, bundleName);
            }
        }
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
            if (!(char.IsLetterOrDigit(c) || c == '_' || c == '/' || c == '-'))
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

    public static void SafeDeleteFile(string assetsOrABSPath)
    {
        if (File.Exists(assetsOrABSPath))
        {
            File.Delete(assetsOrABSPath);
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

    public static void GetComponentsRecursiveByName(string name, GameObject obj, List<Component> result)
    {
        if (string.IsNullOrEmpty(name) || obj == null || result == null)
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

            typeString = component.name;
            if (typeString == name)
            {
                result.Add(component);
            }
        }

        for (int i = 0; i < obj.transform.childCount; ++i)
        {
            GetComponentsRecursiveByName(name, obj.transform.GetChild(i).gameObject, result);
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
        return false;
    }

    public static bool IsMatHasStandardShader(string assetPath, bool logError = true)
    {
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

            AssetDatabase.ImportAsset(EditorUtils.ABSPath2AssetsPath(file), ImportAssetOptions.ForceSynchronousImport);
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
        string name = EditorFileUtils.GetFileNameWithoutExtend(scene.path);
        return name;
    }

    public static string GetCurSceneParentDirAbsPath()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        string path = string.Empty;
        path = EditorUtils.AssetsPath2ABSPath(scene.path);
        path = EditorFileUtils.GetDirPath(path);
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
        string sceneAbsDir = EditorUtils.AssetsPath2ABSPath("Assets/Resources/scene/" + sceneName + "/");
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
        string sceneAbsDir = EditorUtils.AssetsPath2ABSPath("Assets/Resources/scene/" + sceneName + "/");
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

    public static void SetTextureImporterFormat(TextureImporter ti, TextureImporterFormat format)
    {
        TextureImporterPlatformSettings settingStandalone = ti.GetPlatformTextureSettings("Standalone");
        TextureImporterPlatformSettings settingiPhone = ti.GetPlatformTextureSettings("iPhone");
        TextureImporterPlatformSettings settingAndroid = ti.GetPlatformTextureSettings("Android");

        settingStandalone.overridden = true;
        settingiPhone.overridden = true;
        settingAndroid.overridden = true;

        settingStandalone.format = format;
        settingiPhone.format = format;
        settingAndroid.format = format;

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
                if (mats[j] != null)
                {
                    mats[j] = AssetDatabase.LoadAssetAtPath<Material>("Assets/LingRen/Shader/Res/FBXDefaultMat.mat");
                }
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

        var paths = EditorFileUtils.GetDirSubFilePathList(absPath, true, ".mat");
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

        for (int i = 0; i < strList.Count; ++i)
        {
            sb.Append(strList[i]);

            if (i != strList.Count - 1)
            {
                sb.Append(spliter);
            }
        }
        
        return sb.ToString();
    }

    public static List<string> String2StringList(string str, char spliter = '\n')
    {
        if (string.IsNullOrEmpty(str))
        {
            return new List<string>();
        }

        string[] items = str.Split(spliter);
        List<string> result = new List<string>();
        result.AddRange(items);

        return result;
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

    public static AssetBundleBuild CreateAssetBundleBuild(string assetPath, string abName)
    {
        if (assetPath == "Assets/character/g202/mat/g202_bodyp_t1.mat")
        {
            Debug.Log(string.Format("[EditorUtils:CreateAssetBundleBuild] {0} {1}", assetPath, abName));
        }
        
        AssetBundleBuild build = new AssetBundleBuild();
        build.assetBundleName = abName + ".assetbundle";
        build.assetNames = new string[] {assetPath};

        if (!IsValidAssetBundleName(abName))
        {
            BuildErrorLog(build.assetBundleName + " ab name is error path " + assetPath);
        }

        return build;
    }

    public static void BuildAssetBundles(List<AssetBundleBuild> builds)
    {
        float startTime = Time.realtimeSinceStartup;
        BuildAssetBundleOptions options = BuildAssetBundleOptions.ChunkBasedCompression |
                                            BuildAssetBundleOptions.DeterministicAssetBundle;
        //BuildPipeline.BuildAssetBundles(Builder.RES_OUT_PUT_DIR_PATH, builds.ToArray(), options, Builder.BUILD_TARGET);
        Debug.LogError("BuildAssetBundles cost " + (Time.realtimeSinceStartup - startTime));
    }
    
    public static bool IsTexture(string assetPath)
    {
        assetPath = assetPath.ToLower();
        if (assetPath.EndsWith(".png") || assetPath.EndsWith(".tga") || assetPath.EndsWith(".tif") ||
            assetPath.EndsWith(".hdr") || assetPath.EndsWith(".psd") || assetPath.EndsWith(".tga") ||
            assetPath.EndsWith(".jpg") || assetPath.EndsWith("bmp") || assetPath.EndsWith(".cubemap"))
        {
            return true;
        }

        return false;
    }
    
    public static bool IsMaterial(string assetPath)
    {
        assetPath = assetPath.ToLower();
        if (assetPath.EndsWith(".mat"))
        {
            return true;
        }

        return false;
    }
        
    public static bool IsFBX(string assetPath)
    {
        assetPath = assetPath.ToLower();
        if (assetPath.EndsWith(".fbx"))
        {
            return true;
        }

        return false;
    }
    
    public static bool IsPrefab(string assetPath)
    {
        assetPath = assetPath.ToLower();
        if (assetPath.EndsWith(".prefab"))
        {
            return true;
        }

        return false;
    }

    public static bool ShaderHasProp(Shader shader, ShaderUtil.ShaderPropertyType propType, string propName)
    {
        int count = ShaderUtil.GetPropertyCount(shader);

        for (int index = 0; index < count; ++index)
        {
            ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType(shader, index);

            string name = ShaderUtil.GetPropertyName(shader, index);

            if (propType == type && name == propName)
            {
                return true;
            }
        }

        return false;
    }

    public static bool CheckIsDependMissing(string assetPath, bool dumpError = true)
    {
        string[] depends = AssetDatabase.GetDependencies(new string[] { assetPath }, true);
        bool hasMissDepend = false;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(string.Format("{0} depend is missing:", assetPath));
        //depends = new string[] { assetPath };

        foreach (string depend in depends)
        {
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(depend);
            if (obj == null)
            {
                continue;
            }

            if (obj is GameObject && CheckIsGameObjectPropMissing(obj as GameObject, sb))
            {
                hasMissDepend = true;
            }
            else if (obj is Material && CheckIsMatPropMissing(obj as Material, sb))
            {
                hasMissDepend = true;
            }
        }

        if (hasMissDepend)
        {
            BuildErrorLog(sb.ToString());
        }

        return hasMissDepend;
    }

    public static List<AssetBundleBuild> MergeBuildList(List<AssetBundleBuild> buildList)
    {
        List<AssetBundleBuild> resultList = new List<AssetBundleBuild>();
        Dictionary<string, List<string>> resultMap = new Dictionary<string, List<string>>();

        foreach (AssetBundleBuild build in buildList)
        {
            List<string> assetPathList = null;
            string assetBundleName = build.assetBundleName;

            if (!resultMap.TryGetValue(assetBundleName, out assetPathList))
            {
                assetPathList = new List<string>();
                resultMap[assetBundleName] = assetPathList;
            }

            assetPathList.AddRange(build.assetNames);
        }

        foreach (KeyValuePair<string, List<string>> kv in resultMap)
        {
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = kv.Key;
            build.assetNames = kv.Value.ToArray();
            resultList.Add(build);
        }

        return resultList;
    }

    static bool CheckIsGameObjectPropMissing(GameObject go, StringBuilder sb)
    {
        if (go == null)
        {
            return false;
        }

        bool hasMissProp = false;
        List<Component> cpList = new List<Component>();
        cpList.AddRange(go.GetComponents<Component>());
        cpList.AddRange(go.GetComponentsInChildren<Component>());
        for (int i = 0; i < cpList.Count; i++)
        {
            Component cp = cpList[i];
            if (cp == null)
            {
                sb.AppendLine("  script: missing");
                hasMissProp = true;
                continue;
            }

            SerializedObject so = new SerializedObject(cp);
            var sp = so.GetIterator();
            var objRefValueMethod = typeof(SerializedProperty).GetProperty("objectReferenceStringValue", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            while (sp.Next(true))
            {
                if (sp.propertyType == SerializedPropertyType.ObjectReference)
                {
                    string objectReferenceStringValue = string.Empty;

                    if (objRefValueMethod != null)
                    {
                        objectReferenceStringValue = (string)objRefValueMethod.GetGetMethod(true).Invoke(sp, new object[] { });
                    }

                    if (sp.objectReferenceValue == null
                        && (sp.objectReferenceInstanceIDValue != 0 || objectReferenceStringValue.StartsWith("Missing")))
                    {
                        // PSR只有Mesh渲染模式才检查Mesh丢失
                        if (cp is UnityEngine.ParticleSystemRenderer && (!string.IsNullOrEmpty(sp.name) && sp.name.StartsWith("m_Mesh"))  &&
                            (cp as UnityEngine.ParticleSystemRenderer).renderMode != ParticleSystemRenderMode.Mesh)
                        {
                            continue;
                        }

                        // PS不检查Mesh丢失
                        if (cp is UnityEngine.ParticleSystem && (!string.IsNullOrEmpty(sp.name) && sp.name.StartsWith("m_Mesh")))
                        {
                            continue;
                        }

                        hasMissProp = true;
                        sb.AppendLine(string.Format("  component: {0} {1}", cp.name, objectReferenceStringValue));
                    }
                }
            }
        }

        return hasMissProp;
    }

    static bool CheckIsMatPropMissing(Material mat, StringBuilder sb)
    {
        if (mat == null)
        {
            return false;
        }

        if (mat.shader == null)
        {
            sb.AppendLine(string.Format("  shader: {0} is missing", mat.name));
            return true;
        }

        bool hasMissProp = false;

        SerializedObject so = new SerializedObject(mat);
        var sp = so.GetIterator();
        var objRefValueMethod = typeof(SerializedProperty).GetProperty("objectReferenceStringValue", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Dictionary<string, string> texPropertyPathMap = new Dictionary<string, string>();

        while (sp.Next(true))
        {
            string texIndexStr = GetTexEnvsIndexStr(sp.propertyPath);
            if (string.IsNullOrEmpty(texIndexStr))
            {
                continue;
            }

            // 保存Tex属性名
            if (sp.propertyPath.EndsWith("].first"))
            {
                texPropertyPathMap[texIndexStr] = sp.stringValue;
                continue;
            }

            if (!sp.propertyPath.EndsWith("].second.m_Texture") ||
                !texPropertyPathMap.ContainsKey(texIndexStr) ||
                !ShaderHasProp(mat.shader, ShaderUtil.ShaderPropertyType.TexEnv, texPropertyPathMap[texIndexStr]))
            {
                continue;
            }

            if (sp.propertyType == SerializedPropertyType.ObjectReference)
            {
                string objectReferenceStringValue = string.Empty;

                if (objRefValueMethod != null)
                {
                    objectReferenceStringValue = (string)objRefValueMethod.GetGetMethod(true).Invoke(sp, new object[] { });
                }

                if (sp.objectReferenceValue == null
                    && (sp.objectReferenceInstanceIDValue != 0 || objectReferenceStringValue.StartsWith("Missing")))
                {
                    hasMissProp = true;
                    sb.AppendLine(string.Format("  tex: material {0}, texture {1} is missing", mat.name, texPropertyPathMap[texIndexStr]));
                }
            }
        }

        string[] texNames = mat.GetTexturePropertyNames();

        foreach (string texName in texNames)
        {
            if (!ShaderHasProp(mat.shader, ShaderUtil.ShaderPropertyType.TexEnv, texName))
            {
                continue;
            }
        }

        return hasMissProp;
    }

    static string GetTexEnvsIndexStr(string propPath)
    {
        string texPropStartTag = "m_SavedProperties.m_TexEnvs.Array.data[";
        string texPropEndTag = "]";
        int tagStartPos = propPath.IndexOf(texPropStartTag);
        int tagEndPos = propPath.IndexOf(texPropEndTag);

        if (tagStartPos == -1 || tagEndPos == -1)
        {
            return string.Empty;
        }

        return propPath.Substring(tagStartPos + texPropStartTag.Length, tagEndPos - tagStartPos - texPropStartTag.Length);
    }
}
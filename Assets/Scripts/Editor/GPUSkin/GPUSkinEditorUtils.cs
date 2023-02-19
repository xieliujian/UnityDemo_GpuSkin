using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class GPUSkinEditorUtils
{
    public static GameObject GetDirModleGameObject(string dirAssetPath)
    {
        List<string> objAssetPathList = GetDirSubFileAssetPaths(dirAssetPath);

        foreach (string objAssetPath in objAssetPathList)
        {
            if (!objAssetPath.ToLower().EndsWith(".fbx") || objAssetPath.ToLower().EndsWith(".meta"))
            {
                continue;
            }

            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(objAssetPath);

            if (!objAssetPath.Contains("_") && obj != null)
            {
                return obj;
            }
        }

        return null;
    }

    public static bool IsOneMesh(string dirAssetPath)
    {
        GameObject obj = GetDirModleGameObject(dirAssetPath);

        Animator animator = obj.GetComponent<Animator>();
        SkinnedMeshRenderer[] smrs = obj.GetComponentsInChildren<SkinnedMeshRenderer>();

        if (animator == null || smrs.Length != 1)
        {
            return false;
        }

        return true;
    }

    public static List<Material> GetDirMaterials(string dirAssetPath)
    {
        List<Material> mats = new List<Material>();
        List<string> objAssetPathList = EditorUtils.GetDirSubFilePathList(dirAssetPath + "/mat/", true, "mat");

        for (int i = 0; i < objAssetPathList.Count; i++)
        {
            string objAssetPath = objAssetPathList[i];
            if (!objAssetPath.ToLower().EndsWith(".mat") || objAssetPath.ToLower().EndsWith(".meta") || !objAssetPath.Contains("_"))
            {
                continue;
            }

            objAssetPath = EditorUtils.GetAssetPathFromFullPath(objAssetPath);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(objAssetPath);

            var maintex = mat.GetTexture("_BaseMap");
            if (maintex != null)
            {
                if (mat != null && !mat.name.Contains("__preview__"))
                {
                    mats.Add(mat);
                }
            }
        }

        return mats;
    }
    
    public static List<AnimationClip> GetDirAnimationClips(string dirAssetPath)
    {
        List<AnimationClip> clips = new List<AnimationClip>();
        List<string> objAssetPathList =
            EditorUtils.GetDirSubFilePathList(dirAssetPath + "/animation/", true, "fbx");

        for (int i = 0; i < objAssetPathList.Count; i++)
        {
            string objAssetPath = objAssetPathList[i];
            if (!objAssetPath.ToLower().EndsWith(".fbx") || objAssetPath.ToLower().EndsWith(".meta") || !objAssetPath.Contains("_"))
            {
                continue;
            }

            objAssetPath = EditorUtils.GetAssetPathFromFullPath(objAssetPath);
            Object[] objs = AssetDatabase.LoadAllAssetsAtPath(objAssetPath);
            foreach (Object obj in objs)
            {
                AnimationClip clip = obj as AnimationClip;
                if (clip != null && !clip.name.Contains("__preview__"))
                {
                    clips.Add(clip);
                }
            }
        }

        return clips;
    }
    
    public static void BakeAnimatorFrame(Animator animator, string animationName, float frameRate, int frameCount)
    {
        animator.StopPlayback();
        animator.Play(animationName);

        animator.recorderStartTime = 0;

        float recorderStopTime = 1.0f / frameRate * frameCount;
        animator.recorderStopTime = recorderStopTime;

        animator.StartRecording(frameCount);
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        for (int i = 0; i < frameCount; ++i)
        {
            animator.Update(1.0f / frameRate);
        }

        animator.StopRecording();
        animator.StartPlayback();
    }

    public static void CreateAsset(UnityEngine.Object asset, string path)
    {
        string dirPath = EditorUtils.AssetsPath2ABSPath(EditorUtils.GetDirPath(path));
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        
        EditorUtils.SafeRemoveAsset(path);
        AssetDatabase.CreateAsset(asset, path);
    }

    public static void CreatePrefab(string dstMeshPath, string dstTexPath, string dstInfoPath, string shadername, bool isbonebake, 
        string srcpath, string dstpath, string dstSrcPath, string prefabname)
    {
        var matlist = GetDirMaterials(srcpath);
        if (matlist == null)
            return;

        var shader = Shader.Find(shadername);
        if (shader == null)
            return;

        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(dstMeshPath);
        if (mesh == null)
            return;

        var animtex = AssetDatabase.LoadAssetAtPath<Texture>(dstTexPath);
        if (animtex == null)
            return;

        // 清理dstPath路径所有的材质和prefab
        ClearPrefabPathAllMat(dstSrcPath);
        ClearPrefabPathAllPrefab(dstSrcPath);

        int index = 0;
        for (int i = 0; i < matlist.Count; i++)
        {
            var mat = matlist[i];
            if (mat == null)
                continue;

            var maintex = mat.GetTexture("_BaseMap");
            if (maintex == null)
                continue;

            index++;

            // mat
            var matpath = dstpath + "/mat" + index + ".mat";
            CreatePrefabMat(shader, maintex, isbonebake, animtex, matpath);

            // prefab
            var prefabpath = dstpath + "/" + prefabname + index + ".prefab";
            CreatePrefab(srcpath, mesh, matpath, isbonebake, dstInfoPath, prefabpath);
        }
    }

    static List<string> GetDirSubFileAssetPaths(string dirAssetPath)
    {
        List<string> result = new List<string>();
        string[] fileAbsPaths = Directory.GetFiles(dirAssetPath);

        foreach (string fileAbsPath in fileAbsPaths)
        {
            string fileAssetPath = ABSPath2AssetsPath(fileAbsPath);
            result.Add(fileAssetPath);
        }

        return result;
    }
    
    static string ABSPath2AssetsPath(string absPath)
    {
        string assetRootPath = System.IO.Path.GetFullPath(Application.dataPath);
        return "Assets" + System.IO.Path.GetFullPath(absPath).Substring(assetRootPath.Length).Replace("\\", "/");
    }

    static void CreatePrefabMat(Shader shader, Texture maintex, bool isbonebake, Texture animtex, string matpath)
    {
        Material newmat = new Material(shader);
        if (newmat == null)
            return;

        newmat.SetTexture("_MainTex", maintex);

        if (isbonebake)
        {
            newmat.SetTexture("_BoneAnimTex", animtex);
        }
        else
        {
            newmat.SetTexture("_VertexAnimTex", animtex);
        }

        newmat.enableInstancing = true;

        AssetDatabase.CreateAsset(newmat, matpath);
    }

    static void CreatePrefab(string srcpath, Mesh mesh, string matpath, bool isbonebake, string dstInfoPath, string prefabpath)
    {
        GameObject obj = GPUSkinEditorUtils.GetDirModleGameObject(srcpath);
        if (obj == null)
            return;

        SkinnedMeshRenderer[] smrs = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (smrs.Length != 1)
            return;

        SkinnedMeshRenderer smr = obj.GetComponentInChildren<SkinnedMeshRenderer>();
        if (smr == null)
            return;

        var localPos = smr.transform.localPosition;
        var localRot = smr.transform.localRotation;
        var localScale = smr.transform.localScale;

        GameObject newGo = new GameObject(obj.name);
        GameObject skinGo = new GameObject("mesh");
        if (newGo != null && skinGo != null)
        {
            skinGo.transform.SetParent(newGo.transform);
            skinGo.transform.localPosition = localPos;
            skinGo.transform.localRotation = localRot;
            skinGo.transform.localScale = localScale;

            var meshfilter = skinGo.AddComponent<MeshFilter>();
            if (meshfilter != null)
            {
                meshfilter.sharedMesh = mesh;
            }

            var meshrender = skinGo.AddComponent<MeshRenderer>();
            if (meshrender != null)
            {
                var newmat = AssetDatabase.LoadAssetAtPath<Material>(matpath);
                if (newmat != null)
                {
                    meshrender.sharedMaterial = newmat;
                }
            }

            if (meshrender != null)
            {
                if (isbonebake)
                {
                    var assetinfo = AssetDatabase.LoadAssetAtPath<GPUSkinBoneInfo>(dstInfoPath);
                    if (assetinfo != null)
                    {
                        var script = newGo.AddComponent<GPUSkinBonePlayer>();
                        if (script != null)
                        {
                            script.info = assetinfo;
                            script.m_MeshRenderer = meshrender;
                        }
                    }
                }
                else
                {
                    var assetinfo = AssetDatabase.LoadAssetAtPath<GPUSkinVertexInfo>(dstInfoPath);
                    if (assetinfo != null)
                    {
                        var script = newGo.AddComponent<GPUSkinVertexPlayer>();
                        if (script != null)
                        {
                            script.info = assetinfo;
                            script.m_MeshRenderer = meshrender;
                        }
                    }
                }
            }

            PrefabUtility.SaveAsPrefabAsset(newGo, prefabpath);

            GameObject.DestroyImmediate(newGo);
        }
    }

    /// <summary>
    /// 清理dstPath路径所有的材质和prefab
    /// </summary>
    /// <param name="dstpath"></param>
    static void ClearPrefabPathAllMat(string dstpath)
    {
        List<string> objAssetPathList = EditorUtils.GetDirSubFilePathList(dstpath, true, "mat");

        for (int i = 0; i < objAssetPathList.Count; i++)
        {
            string objAssetPath = objAssetPathList[i];
            objAssetPath = EditorUtils.GetAssetPathFromFullPath(objAssetPath);

            EditorUtils.SafeRemoveAsset(objAssetPath);
        }
    }

    /// <summary>
    /// 清理dstPath路径所有的材质和prefab
    /// </summary>
    /// <param name="dstpath"></param>
    static void ClearPrefabPathAllPrefab(string dstpath)
    {
        List<string> objAssetPathList = EditorUtils.GetDirSubFilePathList(dstpath, true, "prefab");

        for (int i = 0; i < objAssetPathList.Count; i++)
        {
            string objAssetPath = objAssetPathList[i];
            objAssetPath = EditorUtils.GetAssetPathFromFullPath(objAssetPath);

            EditorUtils.SafeRemoveAsset(objAssetPath);
        }
    }
}

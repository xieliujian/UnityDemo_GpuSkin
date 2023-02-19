using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

public class GPUSkinBoneTool : GPUSkinTool
{
    #region Defines

    const int FRAME_RAME_MULTI = 1;

    static string GPU_SKIN_BONE_DIR
    {
        get { return "/GPUSkinBone/"; }
    }

    static string SHADER_NAME
    {
        get { return "Character/GPUSkinBone"; }
    }

    #endregion

    public static GPUSkinBoneInfo CreateGPUSkinBoneInfo(GameObject obj, List<AnimationClip> clips)
    {
        if (obj == null || clips == null || clips.Count == 0)
        {
            return null;
        }

        obj = GameObject.Instantiate(obj);

        Animator animator = obj.GetComponent<Animator>();
        SkinnedMeshRenderer[] smrs = obj.GetComponentsInChildren<SkinnedMeshRenderer>();

        if (animator == null || smrs.Length != 1)
        {
            GameObject.DestroyImmediate(obj);
            return null;
        }

        AnimatorController controller = GenerateAnimatorController(clips);
        animator.runtimeAnimatorController = controller;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        GPUSkinBoneInfo info = ScriptableObject.CreateInstance<GPUSkinBoneInfo>();
        info.mesh = CreateNewMesh(smrs[0]);
        CreateAnimationInfo(info, clips);

        BakeAnimation2Tex(animator, smrs[0], info);
        GameObject.DestroyImmediate(obj);

        return info;
    }

    static void BakeAnimation2Tex(Animator animator, SkinnedMeshRenderer smr, GPUSkinBoneInfo boneAnimationInfo)
    {
        int boneCount = smr.bones.Length;
        int texWidth = boneAnimationInfo.texWidth;
        int texHeight = boneAnimationInfo.texHeight;

        Vector3[] orignalMeshVertexes = smr.sharedMesh.vertices;
        BoneWeight[] orignalBoneWeights = smr.sharedMesh.boneWeights;
        Matrix4x4[] orignalBindPoses = smr.sharedMesh.bindposes;
        Transform[] bones = null;

        foreach (GPUSkinInfo info in boneAnimationInfo.infoList)
        {
            int totalCount = 0;
            GPUSkinEditorUtils.BakeAnimatorFrame(animator, info.name, info.frameRate, info.frameCount);

            for (int frameIndex = 0; frameIndex < info.frameCount; ++frameIndex)
            {
                float playbackTime = (frameIndex) * (1.0f / info.frameRate);

                // 防止生成警告
                if (frameIndex == info.frameCount - 1)
                {
                    playbackTime = (frameIndex - 1) * (1.0f / info.frameRate);
                }

                //Debug.Log("BakeAnimation2Tex Bone : " + info.name + " : " + playbackTime);

                animator.playbackTime = playbackTime;
                animator.Update(0);

                bones = smr.bones;

                for (int boneIndex = 0; boneIndex < bones.Length; ++boneIndex)
                {
                    Transform bone = bones[boneIndex];
                    Matrix4x4 m = smr.worldToLocalMatrix * bone.localToWorldMatrix * orignalBindPoses[boneIndex];
                    totalCount += 4;

                    int pixelCol = (info.startPixelIndex + (frameIndex * boneCount + boneIndex) * 4 + 0) % texWidth;
                    int pixelRow = (info.startPixelIndex + (frameIndex * boneCount + boneIndex) * 4 + 0) / texWidth;
                    boneAnimationInfo.texture.SetPixel(pixelCol, pixelRow, new Color(m.m00, m.m01, m.m02, m.m03));

                    pixelCol = (info.startPixelIndex + (frameIndex * boneCount + boneIndex) * 4 + 1) % texWidth;
                    pixelRow = (info.startPixelIndex + (frameIndex * boneCount + boneIndex) * 4 + 1) / texWidth;
                    boneAnimationInfo.texture.SetPixel(pixelCol, pixelRow, new Color(m.m10, m.m11, m.m12, m.m13));

                    pixelCol = (info.startPixelIndex + (frameIndex * boneCount + boneIndex) * 4 + 2) % texWidth;
                    pixelRow = (info.startPixelIndex + (frameIndex * boneCount + boneIndex) * 4 + 2) / texWidth;
                    boneAnimationInfo.texture.SetPixel(pixelCol, pixelRow, new Color(m.m20, m.m21, m.m22, m.m23));

                    pixelCol = (info.startPixelIndex + (frameIndex * boneCount + boneIndex) * 4 + 3) % texWidth;
                    pixelRow = (info.startPixelIndex + (frameIndex * boneCount + boneIndex) * 4 + 3) / texWidth;
                    boneAnimationInfo.texture.SetPixel(pixelCol, pixelRow, new Color(m.m30, m.m31, m.m32, m.m33));
                }
            }
        }
    }

    static void CreateAnimationInfo(GPUSkinBoneInfo info, List<AnimationClip> clips)
    {
        int vertexCount = info.mesh.vertexCount;
        int boneCount = info.mesh.bindposes.Length;
        int texWidth = 0;
        int texHeight = 0;
        int pixelCount = 0;

        for (int i = 0; i < clips.Count; ++i)
        {
            AnimationClip clip = clips[i];
            int frameCount = (int)(clip.length * clip.frameRate * FRAME_RAME_MULTI);
            pixelCount += frameCount * 4 * boneCount;
        }

        if (!GetTexture2DSize(pixelCount, ref texWidth, ref texHeight))
        {
            return;
        }

        info.texWidth = texWidth;
        info.texHeight = texHeight;
        info.boneCount = boneCount;
        info.texture = new Texture2D(texWidth, texHeight, TextureFormat.RGBAHalf, false);
        info.texture.filterMode = FilterMode.Point;
        info.texture.anisoLevel = 0;

        int totalPixelCount = 0;

        info.infoList = new GPUSkinInfo[clips.Count];
        for (int i = 0; i < clips.Count; ++i)
        {
            AnimationClip clip = clips[i];
            int frameCount = (int)(clip.length * clip.frameRate * FRAME_RAME_MULTI);
            pixelCount = frameCount * 4 * boneCount;

            GPUSkinInfo animationInfo = new GPUSkinInfo();
            animationInfo.name = clip.name;
            animationInfo.startPixelIndex = totalPixelCount;
            animationInfo.totalPixelCount = pixelCount;
            animationInfo.frameRate = clip.frameRate * FRAME_RAME_MULTI;
            animationInfo.frameCount = frameCount;
            animationInfo.length = clip.length;

            info.infoList[i] = animationInfo;
            totalPixelCount += pixelCount;
        }
    }

    static Mesh CreateNewMesh(SkinnedMeshRenderer smr)
    {
        Mesh newMesh = Object.Instantiate(smr.sharedMesh);
        BoneWeight[] boneWeights = newMesh.boneWeights;

        float[] weightArray = new float[4];
        int[] indexArray = new int[4];

        Vector2[] uv2 = new Vector2[newMesh.vertexCount];
        Vector2[] uv3 = new Vector2[newMesh.vertexCount];
        Vector3[] vertexes = newMesh.vertices;

        for (int i = 0; i < boneWeights.Length; ++i)
        {
            BoneWeight b = boneWeights[i];
            weightArray[0] = b.weight0;
            weightArray[1] = b.weight1;
            weightArray[2] = b.weight2;
            weightArray[3] = b.weight3;

            indexArray[0] = b.boneIndex0;
            indexArray[1] = b.boneIndex1;
            indexArray[2] = b.boneIndex2;
            indexArray[3] = b.boneIndex3;

            float maxWeight = -1;
            int maxIndex = -1;
            int maxIndexIndex = -1;

            for (int j = 0; j < weightArray.Length; ++j)
            {
                if (weightArray[j] > maxWeight)
                {
                    maxWeight = weightArray[j];
                    maxIndex = indexArray[j];
                    maxIndexIndex = j;
                }
            }

            float secondWeight = -1;
            int secondIndex = -1;

            for (int j = 0; j < weightArray.Length; ++j)
            {
                if (j != maxIndexIndex && weightArray[j] > secondWeight)
                {
                    secondWeight = weightArray[j];
                    secondIndex = indexArray[j];
                }
            }

            uv2[i] = new Vector2(maxIndex, secondIndex);
            uv3[i] = new Vector2(maxWeight / (maxWeight + secondWeight), secondWeight / (maxWeight + secondWeight));
        }

        newMesh.boneWeights = null;
        newMesh.uv2 = uv2;
        newMesh.uv3 = uv3;

        return newMesh;
    }

    static AnimatorController GenerateAnimatorController(List<AnimationClip> clips)
    {
        AnimatorController controller = new AnimatorController();
        controller.AddLayer("Action");

        for (int i = 0; i < clips.Count; ++i)
        {
            string clipName = clips[i].name;
            int layerIndex = 0;
            string stateName = clipName;

            AnimatorControllerLayer layer = controller.layers[layerIndex];
            UnityEditor.Animations.AnimatorState state = layer.stateMachine.AddState(stateName);
            state.motion = clips[i];
        }

        return controller;
    }

    static bool GetTexture2DSize(int pixelCount, ref int width, ref int height)
    {
        width = (int)(Mathf.Sqrt(pixelCount) + 1);
        width = (width % 2 == 0) ? width : width + 1;
        height = width;
        return true;
    }

    [MenuItem("Assets/Art/Char/Tools/CreateGPUSkinBoneBake")]
    static void ToolsGPUSkinBoneBake()
    {
        //string dirAssetPath = AssetDatabase.GetAssetPath(Selection.activeObject) + "/";

        string dirAssetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        dirAssetPath = Path.GetDirectoryName(dirAssetPath) + "/";

        GPUSkinBoneBake(dirAssetPath);
    }
    
    public static void GPUSkinBoneBake(string dirAssetPath)
    {
        GameObject obj = GPUSkinEditorUtils.GetDirModleGameObject(dirAssetPath);
        List<AnimationClip> clips = GPUSkinEditorUtils.GetDirAnimationClips(dirAssetPath);
        GPUSkinBoneInfo info = CreateGPUSkinBoneInfo(obj, clips);

        string outputDir = dirAssetPath + GPU_SKIN_BONE_DIR;

        GPUSkinEditorUtils.CreateAsset(info.mesh, outputDir + MESH_NAME);
        GPUSkinEditorUtils.CreateAsset(info.texture, outputDir + TEX_NAME);
        GPUSkinEditorUtils.CreateAsset(info, outputDir + INFO_NAME);
    }

    public static void GPUSkinBoneBake(string srcpath, string dstpath)
    {
        GameObject obj = GPUSkinEditorUtils.GetDirModleGameObject(srcpath);
        List<AnimationClip> clips = GPUSkinEditorUtils.GetDirAnimationClips(srcpath);
        GPUSkinBoneInfo info = CreateGPUSkinBoneInfo(obj, clips);

        if (obj == null || clips == null || info == null)
            return;

        string outputSrcDir = dstpath + GPU_SKIN_DIR;
        var outputDir = outputSrcDir.ToLower();

        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        string dstMeshPath = outputDir + MESH_NAME;
        dstMeshPath = dstMeshPath.ToLower();

        string dstTexPath = outputDir + TEX_NAME;
        dstTexPath = dstTexPath.ToLower();

        string dstInfoPath = outputDir + INFO_NAME;
        dstInfoPath = dstInfoPath.ToLower();

        GPUSkinEditorUtils.CreateAsset(info.mesh, dstMeshPath);
        GPUSkinEditorUtils.CreateAsset(info.texture, dstTexPath);
        GPUSkinEditorUtils.CreateAsset(info, dstInfoPath);

        string prefabname = (obj.name + "_" + PREFAB_NAME).ToLower();

        var shadername = SHADER_NAME;

        GPUSkinEditorUtils.CreatePrefab(dstMeshPath, dstTexPath, dstInfoPath, shadername, true, srcpath, outputDir, outputSrcDir, prefabname);
    }
}














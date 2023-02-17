using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public abstract class GPUSkinPlayerBase : MonoBehaviour
{
    protected string STR_CUR_FRAME = "_CurFrame";
    protected string STR_CUR_FRAME_PIXEL_INDEX = "_CurFramePixelIndex";
    protected string STR_CUR_FRAME_COUNT = "_CurFrameCount";

    public MeshRenderer m_MeshRenderer;

    [HideInInspector]
    public int m_AnimationIndex;

    protected float m_LastFrameIndex;

    GPUSkinInfo[] m_Infos;
    Dictionary<string, int> m_ActionNameToIndexDic = new Dictionary<string, int>(10);
    float m_LastFrameTime;
    GPUSkinInfo m_CurrentPlayInfo;

    MaterialPropertyBlock m_Block;

#if !SOUL_ENGINE
    protected abstract void OnEditorValidate(MeshFilter mf, MeshRenderer renderer);

    protected abstract void OnUpdateSkin(MeshRenderer renderer, GPUSkinInfo currentInfo, MaterialPropertyBlock block);
#endif

    protected abstract void OnSetPropertyBlock(MeshRenderer renderer, GPUSkinInfo currentInfo, MaterialPropertyBlock block);

    protected abstract GPUSkinInfo[] GetInfoList();



    public void GetInfos(List<GPUSkinInfo> list)
    {
        if (list == null)
        {
            return;
        }
        
        list.Clear();

        if (m_Infos != null)
        {
            list.AddRange(m_Infos);
        }
    }
    
    public void Play(string actionName)
    {
        if (m_Infos == null || string.IsNullOrEmpty(actionName))
        {
            return;
        }

        if (!m_ActionNameToIndexDic.TryGetValue(actionName, out int index))
        {
            return;
        }

        Play(index);
    }

    public void Play(int index)
    {
        m_AnimationIndex = index;

        if (m_Infos == null || !GetInfo(m_AnimationIndex, out m_CurrentPlayInfo))
        {
            m_AnimationIndex = -1;
        }

        OnSetPropertyBlock(m_MeshRenderer, m_CurrentPlayInfo, m_Block);
    }

    public int RandomPlay()
    {
        int index = Random.Range(0, m_Infos.Length);
        Play(index);
        return index;
    }

    public bool GetInfo(int index, out GPUSkinInfo gpuSkinInfo)
    {
        gpuSkinInfo = default;

        if (m_Infos == null || index < 0 || index >= m_Infos.Length)
        {
            return false;
        }

        gpuSkinInfo = m_Infos[index];
        return true;
    }

    void OnEnable()
    {
        m_LastFrameTime = Time.time;
        m_LastFrameIndex = Random.Range(0, 100);

        if (m_Block == null)
        {
            m_Block = new MaterialPropertyBlock();
        }

        InitAnimationNameList();
        Play(0);
    }

#if !SOUL_ENGINE

    void OnValidate()
    {
        MeshFilter mf = gameObject.GetComponentInChildren<MeshFilter>();
        m_MeshRenderer = gameObject.GetComponentInChildren<MeshRenderer>();

        if (mf != null && m_MeshRenderer != null)
        {
            OnEditorValidate(mf, m_MeshRenderer);
        }
    }

    void Update()
    {
        if (m_AnimationIndex < 0 || m_MeshRenderer == null)
            return;

        float frameTime = 1.0f / m_CurrentPlayInfo.frameRate;

        if (Time.time - m_LastFrameTime >= frameTime)
        {
            m_LastFrameTime = Time.time;
            m_LastFrameIndex = (m_LastFrameIndex + 1) % m_CurrentPlayInfo.frameCount;

            OnUpdateSkin(m_MeshRenderer, m_CurrentPlayInfo, m_Block);
        }
    }
#endif

    protected void InitAnimationNameList()
    {
        m_Infos = GetInfoList();
        m_ActionNameToIndexDic.Clear();
        if (m_Infos == null)
        {
            return;
        }

        for (int i = 0; i < m_Infos.Length; i++)
        {
            GPUSkinInfo info = m_Infos[i];
            m_ActionNameToIndexDic[info.name] = i;
        }
    }
}

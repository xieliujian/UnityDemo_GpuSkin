using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace gtm.Scene.GPUSkin
{
    [ExecuteInEditMode]
    public class GPUSkinVertexPlayer : GPUSkinPlayerBase
    {
        public GPUSkinVertexInfo info;

        protected override GPUSkinInfo[] GetInfoList()
        {
            if (info == null)
                return null;

            return info.infoList;
        }

#if !GAME_ENGINE
        protected override void OnEditorValidate(MeshFilter mf, MeshRenderer renderer)
        {
            if (info == null)
                return;

            if (renderer.sharedMaterial != null)
            {
                renderer.sharedMaterial.SetTexture("_VertexAnimTex", info.texture);
                renderer.sharedMaterial.SetFloat("_VertexAnimTexWidth", info.texWidth);
                renderer.sharedMaterial.SetFloat("_VertexAnimTexHeight", info.texHeight);
                renderer.sharedMaterial.SetFloat("_VertexCount", info.vertexCount);
            }

            InitAnimationNameList();

            if (info.infoList == null)
            {
                Play(-1);
                return;
            }

            if (m_AnimationIndex >= info.infoList.Length)
            {
                Play(info.infoList.Length - 1);
            }
            else if (m_AnimationIndex < 0)
            {
                Play(0);
            }
        }

        protected override void OnUpdateSkin(MeshRenderer renderer, GPUSkinInfo currentInfo, MaterialPropertyBlock block)
        {
            OnSetPropertyBlock(renderer, currentInfo, block);
        }
#endif

        protected override void OnSetPropertyBlock(MeshRenderer renderer, GPUSkinInfo currentInfo, MaterialPropertyBlock block)
        {
            if (renderer == null || block == null)
                return;

            renderer.GetPropertyBlock(block);
            block.SetFloat(STR_CUR_FRAME, m_LastFrameIndex);
            block.SetFloat(STR_CUR_FRAME_PIXEL_INDEX, currentInfo.startPixelIndex);
            block.SetFloat(STR_CUR_FRAME_COUNT, currentInfo.frameCount);
            renderer.SetPropertyBlock(block);
        }
    }
}


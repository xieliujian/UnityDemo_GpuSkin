using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace gtm.Scene.GPUSkin
{
    public class GPUSkinMgr
    {
        static GPUSkinMgr s_Instance;

        public static GPUSkinMgr instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new GPUSkinMgr();
                }

                return s_Instance;
            }
        }

        /// <summary>
        /// .
        /// </summary>
        static readonly int GLOBAL_FRAME_INDEX = Shader.PropertyToID("g_GpuSkinFrameIndex");

        /// <summary>
        /// .
        /// </summary>
        const int FRAME_RATE = 30;

        /// <summary>
        /// .
        /// </summary>
        float m_LastFrameTime;

        /// <summary>
        /// 全局帧索引
        /// </summary>
        int m_GlobalFrameIndex;

        /// <summary>
        /// .
        /// </summary>
        public void OnUpdate()
        {
            RefreshGlobalFrameIndex();
        }

        /// <summary>
        /// 刷新shader的全局帧索引
        /// </summary>
        void RefreshGlobalFrameIndex()
        {
            float frameTime = 1.0f / FRAME_RATE;
            if (Time.time - m_LastFrameTime >= frameTime)
            {
                m_LastFrameTime = Time.time;
                Shader.SetGlobalInt(GLOBAL_FRAME_INDEX, m_GlobalFrameIndex++);
            }
        }
    }
}

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GPUSkinVertexPlayer), true)]
public class GPUSkinVertexPlayerInspector : Editor
{
    List<GPUSkinInfo> m_Infos = new List<GPUSkinInfo>();
    GPUSkinVertexPlayer m_Target
    {
        get { return target as GPUSkinVertexPlayer; }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        DrawPlayBtn();
    }

    /// <summary>
    /// 绘制播放按钮
    /// </summary>
    void DrawPlayBtn()
    {
        if (EditorUtils.DrawHeader("Play Animation"))
        {
            EditorGUILayout.BeginVertical("Box", GUILayout.MinHeight(10f));

            m_Infos.Clear();
            if (m_Target != null)
            {
                m_Target.GetInfos(m_Infos);
            }

            for (int i = 0; i < m_Infos.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                GPUSkinInfo info = m_Infos[i];

                string actionName = info.name;

                GUILayout.Label(actionName, GUILayout.Width(180));
                GUILayout.Label(info.length.ToString(), GUILayout.Width(100));
                
                if (GUILayout.Button("Play"))
                {
                    m_Target.Play(actionName);
                }

                EditorGUILayout.EndHorizontal();
            }


            EditorGUILayout.EndVertical();
        }

    }
}
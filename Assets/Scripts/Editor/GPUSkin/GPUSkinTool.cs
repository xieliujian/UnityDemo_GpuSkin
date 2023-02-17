using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

public class GPUSkinTool
{
    #region Defines

    public static string GPU_SKIN_DIR
    {
        get { return "/GPUSkin/"; }
    }

    protected static string MESH_NAME
    {
        get { return "Mesh.asset"; }
    }

    protected static string TEX_NAME
    {
        get { return "Tex.asset"; }
    }

    protected static string INFO_NAME
    {
        get { return "Info.asset"; }
    }

    public static string PREFAB_NAME
    {
        get { return "GpuSkin"; }
    }

    #endregion
}

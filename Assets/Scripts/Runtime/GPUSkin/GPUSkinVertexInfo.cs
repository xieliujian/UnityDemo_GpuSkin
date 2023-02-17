using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUSkinVertexInfo : ScriptableObject
{
    public Mesh mesh;
    public GPUSkinInfo[] infoList;
    public int texWidth;
    public int texHeight;
    public Texture2D texture;
    
    public int vertexCount;
}
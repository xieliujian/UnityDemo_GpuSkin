using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace gtm.Scene.GPUSkin
{
    public class GPUSkinVertexInfo : ScriptableObject
    {
        public Mesh mesh;
        public GPUSkinInfo[] infoList;
        public int texWidth;
        public int texHeight;
        public Texture2D texture;

        public int vertexCount;
    }
}

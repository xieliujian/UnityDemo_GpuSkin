using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class GPUSkinVertexDebug : MonoBehaviour
{
    public Mesh gpuMesh;
    public Texture2D boneAnimaTex;
    public int vertexCount;
    public int curFram;
    public int startPixelIndex;

    public static Mesh[] bakeMeshs = new Mesh[10000];

    Mesh newMesh;
    MeshFilter meshFilter;

    private void OnEnable()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();
    }

    private void OnValidate()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();

        UpdateNewMesh();
    }

    private void UpdateNewMesh()
    {
        if (gpuMesh == null || boneAnimaTex == null)
        {
            return;
        }

        newMesh = Object.Instantiate(gpuMesh);

        Vector3[] meshVertexs = gpuMesh.vertices;
        Vector3[] newMeshVertexs = newMesh.vertices;
        Vector2[] newMeshBoneIndexes = newMesh.uv2;
        Vector2[] newMeshWeights = newMesh.uv3;

        for (int i = 0; i < meshVertexs.Length; ++i)
        {
            int x = (startPixelIndex + vertexCount * curFram + i) % boneAnimaTex.width;
            int y = (startPixelIndex + vertexCount * curFram + i) / boneAnimaTex.width;
            Color c0 = boneAnimaTex.GetPixel(x, y);

            newMeshVertexs[i] = new Vector3(c0.r, c0.g, c0.b);
        }

        newMesh.vertices = newMeshVertexs;

        /*
        Vector3[] bakeMeshVertexes = bakeMeshs[curFram].vertices;
        for (int i = 0; i < bakeMeshVertexes.Length; ++i)
        {
            if ((bakeMeshVertexes[i] - newMeshVertexs[i]).sqrMagnitude >= 0.05f * 0.05f )
            {

            }
        }*/

        if (meshFilter != null)
        {
            meshFilter.sharedMesh = newMesh;
        }
    }
}

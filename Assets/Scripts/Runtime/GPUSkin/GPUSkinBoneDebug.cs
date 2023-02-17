using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class GPUSkinBoneDebug : MonoBehaviour
{
    public Mesh gpuMesh;
    public Texture2D boneAnimaTex;
    public int boneCount;
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
            int bone0Index = (int)newMeshBoneIndexes[i].x;
            int bone1Index = (int)newMeshBoneIndexes[i].y;
            float bone0Weight = newMeshWeights[i].x;
            float bone1Weight = newMeshWeights[i].y;
            Matrix4x4 m0 = new Matrix4x4();
            Matrix4x4 m1 = new Matrix4x4();

            int x = (startPixelIndex + (boneCount * curFram + bone0Index) * 4 + 0) % boneAnimaTex.width;
            int y = (startPixelIndex + (boneCount * curFram + bone0Index) * 4 + 0) / boneAnimaTex.width;
            Color c0 = boneAnimaTex.GetPixel(x, y);

            x = (startPixelIndex + (boneCount * curFram + bone0Index) * 4 + 1) % boneAnimaTex.width;
            y = (startPixelIndex + (boneCount * curFram + bone0Index) * 4 + 1) / boneAnimaTex.width;
            Color c1 = boneAnimaTex.GetPixel(x, y);

            x = (startPixelIndex + (boneCount * curFram + bone0Index) * 4 + 2) % boneAnimaTex.width;
            y = (startPixelIndex + (boneCount * curFram + bone0Index) * 4 + 2) / boneAnimaTex.width;
            Color c2 = boneAnimaTex.GetPixel(x, y);

            x = (startPixelIndex + (boneCount * curFram + bone0Index) * 4 + 3) % boneAnimaTex.width;
            y = (startPixelIndex + (boneCount * curFram + bone0Index) * 4 + 3) / boneAnimaTex.width;
            Color c3 = boneAnimaTex.GetPixel(x, y);

            m0.m00 = c0.r; m0.m01 = c0.g; m0.m02 = c0.b; m0.m03 = c0.a;
            m0.m10 = c1.r; m0.m11 = c1.g; m0.m12 = c1.b; m0.m13 = c1.a;
            m0.m20 = c2.r; m0.m21 = c2.g; m0.m22 = c2.b; m0.m23 = c2.a;
            m0.m30 = c3.r; m0.m31 = c3.g; m0.m32 = c3.b; m0.m33 = c3.a;

            x = (startPixelIndex + (boneCount * curFram + bone1Index) * 4 + 0) % boneAnimaTex.width;
            y = (startPixelIndex + (boneCount * curFram + bone1Index) * 4 + 0) / boneAnimaTex.width;
            c0 = boneAnimaTex.GetPixel(x, y);

            x = (startPixelIndex + (boneCount * curFram + bone1Index) * 4 + 1) % boneAnimaTex.width;
            y = (startPixelIndex + (boneCount * curFram + bone1Index) * 4 + 1) / boneAnimaTex.width;
            c1 = boneAnimaTex.GetPixel(x, y);

            x = (startPixelIndex + (boneCount * curFram + bone1Index) * 4 + 2) % boneAnimaTex.width;
            y = (startPixelIndex + (boneCount * curFram + bone1Index) * 4 + 2) / boneAnimaTex.width;
            c2 = boneAnimaTex.GetPixel(x, y);

            x = (startPixelIndex + (boneCount * curFram + bone1Index) * 4 + 3) % boneAnimaTex.width;
            y = (startPixelIndex + (boneCount * curFram + bone1Index) * 4 + 3) / boneAnimaTex.width;
            c3 = boneAnimaTex.GetPixel(x, y);

            m1.m00 = c0.r; m1.m01 = c0.g; m1.m02 = c0.b; m1.m03 = c0.a;
            m1.m10 = c1.r; m1.m11 = c1.g; m1.m12 = c1.b; m1.m13 = c1.a;
            m1.m20 = c2.r; m1.m21 = c2.g; m1.m22 = c2.b; m1.m23 = c2.a;
            m1.m30 = c3.r; m1.m31 = c3.g; m1.m32 = c3.b; m1.m33 = c3.a;

            newMeshVertexs[i] = m0.MultiplyPoint(meshVertexs[i]) * bone0Weight + m1.MultiplyPoint(meshVertexs[i]) * bone1Weight;
        }

        newMesh.vertices = newMeshVertexs;

        Vector3[] bakeMeshVertexes = bakeMeshs[curFram].vertices;
        for (int i = 0; i < bakeMeshVertexes.Length; ++i)
        {
            if ((bakeMeshVertexes[i] - newMeshVertexs[i]).sqrMagnitude >= 0.05f * 0.05f )
            {

            }
        }

        if (meshFilter != null)
        {
            meshFilter.sharedMesh = newMesh;
        }
    }
}

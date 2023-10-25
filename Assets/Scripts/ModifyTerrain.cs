using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifyTerrain : MonoBehaviour
{
    public TerrainData terrainData;
    public MeshFilter meshFilter;
    Mesh obj_mesh;

    public List<Vector3> vertecises;
    public float[] a;
    public void UpdateTerrain()
    {

        vertecises.Clear();
        obj_mesh = meshFilter.sharedMesh;


        int heightmapWidth = terrainData.heightmapResolution;
        int heightmapHeight = terrainData.heightmapResolution;
        float[,] heights = terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);


        
        //Vector3 firstVertex = meshFilter.mesh.vertices[0];
        for(int i =0; i < obj_mesh.vertices.Length; i++)
        {
            Vector3 selectedVertex = obj_mesh.vertices[i];
            //if (selectedVertex.y < firstVertex.y)
            //{
                vertecises.Add(selectedVertex);
            //}
        }

        Matrix4x4 localToWorld = meshFilter.transform.localToWorldMatrix;

        for(int i = 0; i < vertecises.Count; i++)
        {
            Vector3 worldVert = localToWorld.MultiplyPoint3x4(vertecises[i]);//meshFilter.transform.TransformPoint(vertecises[i]);

            int z = (int)worldVert.x;
            int x = (int)worldVert.z;

            heights[x/2, z/ 2] = worldVert.y / 600;
        }

        terrainData.SetHeights(0, 0, heights);
    }
}

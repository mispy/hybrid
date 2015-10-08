using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TileChunk : PoolBehaviour {
    public static int width = 32;
    public static int height = 32;
    
    [HideInInspector]
    public MeshRenderer renderer;
    [HideInInspector]
    public Mesh mesh;
    
    public Tile[] tileArray;
    private Vector3[] meshVertices;
    private Vector2[] meshUV;
    private int[] meshTriangles;
    
    private bool needMeshUpdate = false;
    
    public override void OnCreate() {
        renderer = GetComponent<MeshRenderer>();
        mesh = GetComponent<MeshFilter>().mesh;
        tileArray = new Tile[width*height];
        meshVertices = new Vector3[width*height*4];
        meshUV = new Vector2[width*height*4];
        meshTriangles = new int[width*height*6];
    }
    
    void Start() {
        StartCoroutine("MonitorMesh");
    }
    
    public IEnumerable<Vector3> GetVertices(Tile tile, int x, int y) {
        var lx = x * Tile.worldSize;
        var ly = y * Tile.worldSize;
		var o = Tile.worldSize / 2f;
        
        yield return new Vector3(lx - o, ly + o, 0);
        yield return new Vector3(lx + o, ly + o, 0);
        yield return new Vector3(lx + o, ly - o, 0);
        yield return new Vector3(lx - o, ly - o, 0);
    }
    
    public void AttachToMesh(Tile tile, int x, int y, int i) {    
        meshTriangles[i*6] = i*4;
        meshTriangles[i*6+1] = (i*4)+1;
        meshTriangles[i*6+2] = (i*4)+3;
        meshTriangles[i*6+3] = (i*4)+1;
        meshTriangles[i*6+4] = (i*4)+2;
        meshTriangles[i*6+5] = (i*4)+3;

        var j = 0;
        foreach (var vert in GetVertices(tile, x, y)) {
            meshVertices[i*4+j] = vert;
            j += 1;
        }

        var uvs = tile.uvs;
        meshUV[i*4] = uvs[0];
        meshUV[i*4+1] = uvs[1];
        meshUV[i*4+2] = uvs[2];
        meshUV[i*4+3] = uvs[3];
    }
    
    public void ClearMeshPos(int i) {
        meshVertices[i*4] = Vector3.zero;
        meshVertices[i*4+1] = Vector3.zero;
        meshVertices[i*4+2] = Vector3.zero;
        meshVertices[i*4+3] = Vector3.zero;
    }
    
    public IEnumerable<Tile> AllTiles {
        get {
            for (var i = 0; i < width; i++) {
                for (var j = 0; j < height; j++) {
                    var tile = tileArray[width * j + i];
                    if (tile != null) yield return tile;
                }
            }
        }
    }
    
    public Tile this[int x, int y] {
        get {
            var i = width * y + x;
            return tileArray[i];
        }
        
        set {
            Profiler.BeginSample("TileChunk[x,y]");
            
            var i = width * y + x;
            tileArray[i] = value;
            
            if (value == null) {
                ClearMeshPos(i);
            } else {
                AttachToMesh(value, x, y, i);
            }
            
            QueueMeshUpdate();
            Profiler.EndSample();
        }
    }    
    
    public void QueueMeshUpdate() {
        needMeshUpdate = true;
    }
    
    public void UpdateMesh() {
        if (!needMeshUpdate) return;
        
        mesh.Clear();
        mesh.vertices = meshVertices;
        mesh.triangles = meshTriangles;
        mesh.uv = meshUV;
        mesh.Optimize();
        mesh.RecalculateNormals();    
        
        needMeshUpdate = false;
    }

    IEnumerator MonitorMesh() {
        while (true) {
            UpdateMesh();
            yield return null;
        }
    }
}

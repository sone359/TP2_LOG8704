using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class CowSpawner : MonoBehaviour
{
    public Transform trackableSurfaceParent;
    public GameObject prefab;
    public Transform avoidCenter;
    public float avoidRadius = 3f;
    public float heightOffset = 0.2f;
    public float spawnInterval = 5f;
    public int maxObjects = 10;

    List<GameObject> spawned = new List<GameObject>();
    float t;
    
    void Update()
    {
        t += Time.deltaTime;
        if (t < spawnInterval || spawned.Count >= maxObjects) return;
        t = spawnInterval * Math.Max(0.5f - (float)spawned.Count / maxObjects, 0f);

        Vector3 p = RandomPointOnMeshes(trackableSurfaceParent.GetComponentsInChildren<MeshCollider>());
        if (Vector3.Distance(p, avoidCenter.position) < avoidRadius) return;

        p += Vector3.up * heightOffset;
        spawned.Add(Instantiate(prefab, p, Quaternion.identity));
        spawned.RemoveAll(x => !x);
    }

    Vector3 RandomPointOnMeshes(MeshCollider[] colliders)
    {
        if (colliders.Length  == 0) return Vector3.down * 100; 
        var col = colliders[Random.Range(0, colliders.Length)];
        var m = col.sharedMesh;
        var tris = m.triangles;
        var verts = m.vertices;
        int i = Random.Range(0, tris.Length / 3) * 3;
        Vector3 a = col.transform.TransformPoint(verts[tris[i]]);
        Vector3 b = col.transform.TransformPoint(verts[tris[i + 1]]);
        Vector3 c = col.transform.TransformPoint(verts[tris[i + 2]]);
        float r1 = Random.value;
        float r2 = Random.value;
        return a + (b - a) * Mathf.Sqrt(r1) + (c - a) * (r2 * (1 - Mathf.Sqrt(r1)));
    }
}
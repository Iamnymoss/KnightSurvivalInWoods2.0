using UnityEngine;
using UnityEngine.AI;
using NavMeshPlus.Components;
using System.Collections;

public class NavMeshRebakeHelper : MonoBehaviour
{
    public static NavMeshRebakeHelper Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void RequestRebake()
    {
        StartCoroutine(RebakeNextFrame());
    }

    private IEnumerator RebakeNextFrame()
    {
        yield return null;

        NavMeshSurface surface = Object.FindAnyObjectByType<NavMeshSurface>();
        if (surface != null)
        {
            surface.BuildNavMesh();
        }
    }
}
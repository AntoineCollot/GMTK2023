using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IA : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 3;
    NavMeshPath path;

    Vector3[] corners;

    private void Start()
    {
        path = new NavMeshPath();
        corners = new Vector3[10];
    }

    private void Update()
    {
        UpdatePath();

        MoveAlongPath();
    }

    void MoveAlongPath()
    {
        path.GetCornersNonAlloc(corners);
        Vector2 toNextCorner = corners[1] - transform.position;

        transform.Translate(toNextCorner.normalized * moveSpeed * Time.deltaTime, Space.World);
    }

    void UpdatePath()
    {
        NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, path);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (path == null)
            return;
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, path.corners[1]);
    }
#endif
}

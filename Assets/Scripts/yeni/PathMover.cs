using UnityEngine;
using UnityEngine.AI;
using System;

[RequireComponent(typeof(NavMeshAgent))]
public class PathMover : MonoBehaviour
{
    public LineRenderer linePrefab;
    public float arriveTolerance = 0.2f;

    public event Action<Vector3[]> OnPathComputed;

    NavMeshAgent agent;
    NavMeshPath  navPath;          // 
    LineRenderer line;

    void Awake()
    {
        agent   = GetComponent<NavMeshAgent>();
        navPath = new NavMeshPath();              // 

        line = Instantiate(
               linePrefab ??
               new GameObject("PathLine").AddComponent<LineRenderer>(),
               Vector3.zero, Quaternion.identity);

        line.positionCount   = 0;
        line.widthMultiplier = 0.12f;
        line.material        = new Material(Shader.Find("Sprites/Default"));
        line.startColor = line.endColor = Color.cyan;
    }

    public void MoveTo(Vector3 worldPoint)
    {
        if (!NavMesh.SamplePosition(worldPoint, out var hit, 1.5f, NavMesh.AllAreas))
            return;

        agent.SetDestination(hit.position);

        if (agent.CalculatePath(hit.position, navPath) &&
            navPath.status == NavMeshPathStatus.PathComplete)
        {
            DrawPath(navPath);
            OnPathComputed?.Invoke(navPath.corners);        // 
        }
        else
            line.positionCount = 0;
    }

    void DrawPath(NavMeshPath p)
    {
        line.positionCount = p.corners.Length;
        line.SetPositions(p.corners);
    }

    void Update()
    {
        if (!agent.pathPending &&
            agent.remainingDistance <= arriveTolerance &&
            agent.hasPath)
        {
            agent.ResetPath();
            line.positionCount = 0;
        }
    }
}

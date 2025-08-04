#if UNITY_WEBGL
using UnityEngine;

static class PreserveColliders
{
    // Build-time engine stripping'in Box/Mesh Collider'ı atmamasını sağlar
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Keep()
    {
        _ = typeof(BoxCollider);
        _ = typeof(MeshCollider);
    }
}
#endif

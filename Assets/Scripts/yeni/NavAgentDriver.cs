// Assets/Scripts/Movement/NavAgentDriver.cs
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(CharacterController), typeof(NavMeshAgent))]
public class NavAgentDriver : MonoBehaviour
{
    [Header("Hareket")]
    [SerializeField, Range(30f,720f)]
    float turnDegPerSec = 360f;      

    [SerializeField]
    float gravity = -9.81f;

    CharacterController ctrl;
    NavMeshAgent        agent;
    Vector3             vel;        

    void Awake()
    {
        ctrl  = GetComponent<CharacterController>();
        agent = GetComponent<NavMeshAgent>();

        agent.updatePosition = false;   
        agent.updateRotation = false;   
        agent.autoBraking    = true;    
    }

    void Update()
    {
        /*  Ajanın istediği yön */
        if (agent.hasPath && agent.remainingDistance > agent.stoppingDistance)
        {
            Vector3 desired = agent.desiredVelocity;
            Vector3 hor     = new(desired.x, 0f, desired.z);

            /*  Dönüş */
            if (hor.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(hor, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                                          transform.rotation,
                                          targetRot,
                                          turnDegPerSec * Time.deltaTime);
            }

            /*  Adımı sınırla (fazla ilerleme olmasın) */
            float maxStep = Mathf.Max(agent.remainingDistance - agent.stoppingDistance, 0f);
            float stepLen = Mathf.Min(hor.magnitude * Time.deltaTime, maxStep);
            Vector3 step  = hor.normalized * stepLen;
            ctrl.Move(step);
        }

        /*  Yerçekimi */
        if (ctrl.isGrounded && vel.y < 0) vel.y = -2f;
        vel.y += gravity * Time.deltaTime;
        ctrl.Move(vel * Time.deltaTime);

        /* CharacterController*/
        agent.nextPosition = transform.position;

        
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.ResetPath();                
            agent.velocity = Vector3.zero;
        }
    }
}

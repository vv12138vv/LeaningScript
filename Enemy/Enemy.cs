using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private StateMachine stateMachine;
    private NavMeshAgent agent;
    public Path path;
    public string currentState;
    private GameObject player;
    private Vector3 lastKnowPos;
    public GameObject Player
    {
        get => player;
    }
    public NavMeshAgent Agent
    {
        get => agent;
    }

    public Vector3 LastKnowPos
    {
        get => lastKnowPos;
        set => lastKnowPos = value;
    }
    [Header("Sight Values")]
    public float sightDistance=20f;
    public float fieldOfView = 85f;
    public float eyeHeight=0.5f;
    [Header("Weapon Values")] 
    public Transform gunBarrel;

    [Range(0.1f, 10f)] public float fireRate;
    [SerializeField]
    private bool canSeePlayer;

    private void Start()
    {
        stateMachine = GetComponent<StateMachine>();
        agent = GetComponent<NavMeshAgent>();
        stateMachine.Initialise();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        canSeePlayer=CanSeePlayer();
        currentState = stateMachine.activeState.ToString();
    }

    public bool CanSeePlayer()
    {
        if (player != null)
        {
            if (Vector3.Distance(transform.position, player.transform.position) < sightDistance)
            {
                Vector3 targetDirection = (player.transform.position - transform.position);
                float angleToPlayer = Vector3.Angle(targetDirection, transform.forward);
                if (angleToPlayer >= -fieldOfView && angleToPlayer <= fieldOfView)
                {
                    Ray ray = new Ray(transform.position, targetDirection);
                    RaycastHit hitInfo = new RaycastHit();
                    if (Physics.Raycast(ray, out hitInfo, sightDistance))
                    {
                        if (hitInfo.transform.gameObject == player)
                        {
                            return true;
                        }
                    }
                    Debug.DrawRay(ray.origin,ray.direction*sightDistance,Color.red);
                }
            }
            
        }
        return false;
    }
    
}
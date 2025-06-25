using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity {

    public enum State { Idle, Chasing, Attacking };
    State currentState;

    NavMeshAgent pathfinder;
    Transform target;
    LivingEntity targetEntity;
    Material skinMaterial;

    Color originalColor;

    float attackDistanceTreshold = 0.5f;
    float timebetweenAttacks = 1;
    float damage = 1;

    float nextAttackTime;
    float myCollisionRadius;
    float targetCollisionRadius;

    bool hasTarget;

    protected override void Start() {
        base.Start();
        pathfinder = GetComponent<NavMeshAgent>();
        skinMaterial= GetComponent<Renderer>().material;
        originalColor = skinMaterial.color;

        if( GameObject.FindGameObjectWithTag( "Player" ) != null ) {
            currentState = State.Chasing;
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();
            targetEntity.OnDeath += OnTargetDeath;

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;

            StartCoroutine( UpdatePath() );
        }
    }
    
    void OnTargetDeath() {
        hasTarget = false;
        currentState = State.Idle;
    }

    void Update() {
        if( hasTarget ) {
            if( Time.time > nextAttackTime ) {
                float sqrDstToTarget = ( target.position - transform.position ).sqrMagnitude; // c^2 instead of c (pythagoras) because its cheaper than sqrroot and its the same in a relative camparison
                if( sqrDstToTarget < Mathf.Pow( attackDistanceTreshold + myCollisionRadius + targetCollisionRadius, 2 ) ) {
                    nextAttackTime = Time.time + timebetweenAttacks;
                    StartCoroutine( Attack() );
                }
            }
        }

    }

    IEnumerator Attack() {
        currentState = State.Attacking;
        pathfinder.enabled = false;
        Vector3 originalPosition = transform.position;
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - dirToTarget * myCollisionRadius;

        float attackSpeed = 3;
        float fraction = 0;

        skinMaterial.color = Color.red;
        bool hasAppliedDamage = false;

        while( fraction  <= 1 ) {

            if( fraction >= 0.5 && !hasAppliedDamage ) {
                hasAppliedDamage = true;
                targetEntity.TakeDamage( damage );
            }

            fraction += Time.deltaTime * attackSpeed;
            float interpolation = (-fraction * fraction + fraction) * 4;
            transform.position = Vector3.Lerp( originalPosition, attackPosition, interpolation );
            
            yield return null;
        }

        skinMaterial.color = originalColor;
        currentState = State.Chasing;
        pathfinder.enabled = true;
    }

    IEnumerator UpdatePath() {
        float refreshRate = 0.25f;

        while( hasTarget ) {
            if( currentState == State.Chasing ) {
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceTreshold/2);
                if( !dead ) {
                    pathfinder.SetDestination( targetPosition );
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}

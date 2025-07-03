using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity {

    public enum State { Idle, Chasing, Attacking };
    State currentState;

    public ParticleSystem deathEffect;

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

    void Awake() {
        base.Start();
        pathfinder = GetComponent<NavMeshAgent>();

        if( GameObject.FindGameObjectWithTag( "Player" ) != null ) {
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
        }
    }

    protected override void Start() {
        base.Start();

        if( hasTarget ) {
            currentState = State.Chasing;
            targetEntity.OnDeath += OnTargetDeath;

            StartCoroutine( UpdatePath() );
        }
    }

    void Update() {

        // Attack
        if( hasTarget ) {
            if( Time.time > nextAttackTime ) {
                float sqrDstToTarget = ( target.position - transform.position ).sqrMagnitude; // c^2 instead of c (pythagoras) because its cheaper than sqrroot and its the same in a relative camparison
                if( sqrDstToTarget < Mathf.Pow( attackDistanceTreshold + myCollisionRadius + targetCollisionRadius, 2 ) ) {
                    nextAttackTime = Time.time + timebetweenAttacks;
                    AudioManager.Instance.PlaySound( "Enemy Attack", transform.position );
                    StartCoroutine( Attack() );
                }
            }
        }

    }

    public void SetCharacteristics( float moveSpeed, int hitsToKillPlayer, float enemyHealth, Color skinColor ) {
        pathfinder.speed = moveSpeed;

        if( hasTarget ) {
            damage = Mathf.Ceil( targetEntity.startingHealth / hitsToKillPlayer );
        }
        startingHealth = enemyHealth;

        skinMaterial = GetComponent<Renderer>().material;
        skinMaterial.color = skinColor;
        originalColor = skinMaterial.color;
    }

    public override void TakeHit( float damage, Vector3 hitPoint, Vector3 hitDirection ) {
        AudioManager.Instance.PlaySound( "Impact", transform.position );
        if ( damage >= health ) {
            AudioManager.Instance.PlaySound( "Enemy Death", transform.position );
            
            GameObject effectInstance = Instantiate( deathEffect.gameObject, hitPoint, Quaternion.FromToRotation( Vector3.forward, hitDirection ) );

            ParticleSystemRenderer renderer = effectInstance.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material( renderer.material ); // Clone to avoid modifying the shared material
            renderer.material.color = originalColor;

            ParticleSystem particleSystem = effectInstance.GetComponent<ParticleSystem>();
            Destroy( effectInstance, particleSystem.main.duration + particleSystem.main.startLifetime.constantMax );
        }

        base.TakeHit( damage, hitPoint, hitDirection );
    }


    void OnTargetDeath() {
        hasTarget = false;
        currentState = State.Idle;
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

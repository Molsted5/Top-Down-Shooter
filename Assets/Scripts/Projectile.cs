using UnityEngine;

public class Projectile : MonoBehaviour {
    
    public LayerMask collisionMask;
    float speed = 10;
    float damage = 1;

    float lifetime = 1;
    float skinWidth = 0.1f;

    void Start() {
        Destroy( gameObject, lifetime );

        Collider[] initialCollisions = Physics.OverlapSphere( transform.position, 0.1f, collisionMask );
        if( initialCollisions.Length > 0 ) {
            OnHitObject( initialCollisions[0], transform.position );
        }

    }

    void Update() {
        float moveDistance = speed * Time.deltaTime;
        CheckCollisions( moveDistance );
        transform.Translate( Vector3.forward * moveDistance );
    }

    public void SetSpeed(float newSpeed ) {
        speed = newSpeed;
    }

    void CheckCollisions( float moveDistance ) {
        Ray ray = new Ray( transform.position, transform.forward );
        RaycastHit hit;

        if( Physics.Raycast( ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide ) ) {
            OnHitObject( hit.collider, hit.point );
        }
    }

    void OnHitObject( Collider c, Vector3 hitPoint ) {
        IDamageable damageableObject = c.GetComponent<IDamageable>();
        if( damageableObject != null ) {
            damageableObject.TakeHit( damage, hitPoint, transform.forward );
        }
        GameObject.Destroy( gameObject );
    }
}

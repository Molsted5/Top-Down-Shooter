using System.Collections;
using UnityEngine;

public class Gun: MonoBehaviour {

    public enum FireMode { Auto, Burst, Single };
    public FireMode fireMode;

    public Transform[] projectileSpawn;
    public Projectile projectile;
    public float msBetweenShots = 100;
    public float muzzleVelocity = 35;
    public int burstCount;
    public int projectilesPerMag;
    public float reloadTime = 0.3f;

    [Header( "Recoil" )]
    public Vector2 kickMinMax = new Vector2( 0.05f, 0.2f );
    public Vector2 recoilAngleMinMax = new Vector2( 3, 5 );
    public float recoilMoveSettleTime = 0.1f;
    public float recoilRotationSettleTime = 0.1f;

    [Header( "Effects" )]
    public Transform shell;
    public Transform shellEjection;
    public AudioClip shootAudio;
    public AudioClip reloadAudio;

    MuzzleFlash muzzleFlash;
    float nextShotTime;

    bool triggerReleasedSinceLastShot;
    int shotsRemainingInBurst;
    int projectilesRemainingInMag;
    bool isReloading;

    float recoilAngle;
    Vector3 recoilOffset;
    Vector3 recoilVelocity;
    float recoilAngleVelocity;

    Coroutine recoilRoutine;

    void Start() {
        muzzleFlash = GetComponent<MuzzleFlash>();
        shotsRemainingInBurst = burstCount;
        projectilesRemainingInMag = projectilesPerMag;
    }

    void Shoot() {
        if( !isReloading && Time.time > nextShotTime && projectilesRemainingInMag > 0 ) {
            if( fireMode == FireMode.Burst && shotsRemainingInBurst == 0 ) return;
            if( fireMode == FireMode.Burst ) shotsRemainingInBurst--;
            if( fireMode == FireMode.Single && !triggerReleasedSinceLastShot ) return;

            for( int i = 0; i < projectileSpawn.Length; i++ ) {
                if( projectilesRemainingInMag == 0 ) break;
                projectilesRemainingInMag--;
                nextShotTime = Time.time + msBetweenShots / 1000;
                Projectile newProjectile = Instantiate( projectile, projectileSpawn[i].position, projectileSpawn[i].rotation );
                newProjectile.SetSpeed( muzzleVelocity );
            }

            Instantiate( shell, shellEjection.position, shellEjection.rotation );
            muzzleFlash.Activate();

            recoilOffset -= Vector3.forward * Random.Range( kickMinMax.x, kickMinMax.y );
            recoilAngle = Mathf.Clamp( recoilAngle + Random.Range( recoilAngleMinMax.x, recoilAngleMinMax.y ), 0, 30 );

            if( recoilRoutine == null ) {
                recoilRoutine = StartCoroutine( RecoilAnimator() );
            }

            if( projectilesRemainingInMag == 0 ) {
                Reload();
            }

            AudioManager.Instance.PlaySound( shootAudio, transform.position );
        }
    }

    IEnumerator RecoilAnimator() {
        while( true ) {
            transform.localPosition = Vector3.SmoothDamp( transform.localPosition, Vector3.zero, ref recoilVelocity, recoilMoveSettleTime );
            recoilAngle = Mathf.SmoothDamp( recoilAngle, 0f, ref recoilAngleVelocity, recoilRotationSettleTime );
            transform.localEulerAngles = Vector3.left * recoilAngle;

            if( recoilAngle < 0.01f && transform.localPosition.magnitude < 0.001f ) break;

            yield return null;
        }

        recoilAngle = 0f;
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
        recoilRoutine = null;
    }

    public void Reload() {
        if( !isReloading && projectilesRemainingInMag != projectilesPerMag ) {
            shotsRemainingInBurst = burstCount;
            StartCoroutine( AnimateReload() );
            AudioManager.Instance.PlaySound( reloadAudio, transform.position );
        }
    }

    IEnumerator AnimateReload() {
        isReloading = true;

        float resetDuration = 0.1f;
        float t = 0f;
        float startAngle = recoilAngle;
        Vector3 startPos = transform.localPosition;

        if( recoilRoutine != null ) {
            StopCoroutine( recoilRoutine );
            recoilRoutine = null;
        }

        while( t < 1f ) {
            t += Time.deltaTime / resetDuration;
            float angle = Mathf.Lerp( startAngle, 0f, t );
            Vector3 pos = Vector3.Lerp( startPos, Vector3.zero, t );

            transform.localEulerAngles = Vector3.left * angle;
            transform.localPosition = pos;

            yield return null;
        }

        recoilAngle = 0f;
        recoilOffset = Vector3.zero;
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;

        yield return new WaitForSeconds( 0.2f );

        float fraction = 0f;
        float maxReloadAngle = 30f;
        Vector3 initialRot = transform.localEulerAngles;

        while( fraction < 1f ) {
            fraction += Time.deltaTime / reloadTime;
            float interpolation = (-fraction * fraction + fraction) * 4;
            float reloadAngle = Mathf.Lerp( 0, maxReloadAngle, interpolation );
            transform.localEulerAngles = initialRot + Vector3.left * reloadAngle;
            yield return null;
        }

        isReloading = false;
        projectilesRemainingInMag = projectilesPerMag;
    }

    public void Aim( Vector3 aimPoint ) {
        if( !isReloading ) {
            transform.LookAt( aimPoint );
        }
    }

    public void OnTriggerHold() {
        Shoot();
        triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerRelease() {
        triggerReleasedSinceLastShot = true;
        shotsRemainingInBurst = burstCount;
    }
}
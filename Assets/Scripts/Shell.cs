using System.Collections;
using UnityEngine;

public class Shell : MonoBehaviour {
    public Rigidbody myRigidbody;
    public float forceMin;
    public float forceMax;

    float lifetime = 4;
    float fadetime = 2;

    void Start()
    {
        float force = Random.Range( forceMin, forceMax );
        myRigidbody.AddForce( transform.right * force );
        myRigidbody.AddTorque( Random.insideUnitSphere * force );

        StartCoroutine( Fade() );
    }

    IEnumerator Fade() {
        yield return new WaitForSeconds( lifetime );

        float fraction = 0;
        Material mat = GetComponent<Renderer>().material;
        Color initialColor = mat.color;

        while(fraction < 1 ) {
            fraction += Time.deltaTime / fadetime;
            mat.color = Color.Lerp( initialColor, Color.clear, fraction );
            yield return null;
        }

        Destroy(gameObject);
    }

}

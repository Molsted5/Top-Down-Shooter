using UnityEngine;

public class Crosshairs : MonoBehaviour {

    public LayerMask targetMask;
    public SpriteRenderer dotRenderer;
    public Color dotHighlightColor;
    Color originalDotColor;

    void Start() {
        Cursor.visible = false;
        originalDotColor = dotRenderer.material.color;
    }

    void Update() {
        transform.Rotate( Vector3.forward * -40 * Time.deltaTime );
    }

    public void DetectTargets(Ray ray ) {
        if( Physics.Raycast( ray, 100, targetMask ) ) {
            dotRenderer.material.color = dotHighlightColor;
        }
        else {
            dotRenderer.material.color = originalDotColor;
        }
    }
}

using UnityEngine;

public class ExplodeTest : MonoBehaviour
{
    public float explosionForce = 50f;

    private Explodable explodable;

    void Start()
    {
        Debug.Log("Explode");
        explodable = GetComponent<Explodable>();

        Vector3 center = transform.position;
        explodable.explode();

        // explode() only splits the sprite into pieces; it applies no force,
        // so without this the fragments just sit assembled in place.
        foreach (GameObject frag in explodable.fragments)
        {
            Rigidbody2D rb = frag.GetComponent<Rigidbody2D>();
            Vector2 dir = ((Vector2)frag.transform.position - (Vector2)center).normalized;
            if (dir == Vector2.zero)
            {
                dir = Random.insideUnitCircle.normalized;
            }
            rb.AddForce(dir * explosionForce, ForceMode2D.Impulse);
        }
    }
}
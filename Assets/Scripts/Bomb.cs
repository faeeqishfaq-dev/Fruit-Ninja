using UnityEngine;

public class Bomb : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GetComponent<Collider>().enabled = false;
            
            // Freeze physics instantly
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
            else
            {
                Rigidbody2D rb2D = GetComponent<Rigidbody2D>();
                if (rb2D != null)
                {
                    rb2D.velocity = Vector2.zero;
                    rb2D.angularVelocity = 0f;
                    rb2D.isKinematic = true;
                }
            }

            // Capture visual center
            Renderer rend = GetComponentInChildren<Renderer>();
            Vector3 centerPos = rend != null ? rend.bounds.center : transform.position;

            // Trigger explosion
            GameManager.Instance.Explode(centerPos);
        }
    }

}

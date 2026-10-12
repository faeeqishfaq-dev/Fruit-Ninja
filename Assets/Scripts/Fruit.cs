using UnityEngine;

public class Fruit : MonoBehaviour
{
    [Header("Fruit Settings")]
    [SerializeField] private GameObject whole;
    [SerializeField] private GameObject sliced;

    [SerializeField] private int points = 1;

    [Header("Juice Settings")]
    [SerializeField] private Color juiceColor = Color.white;

    private Rigidbody fruitRigidbody;
    private Collider fruitCollider;
    private ParticleSystem juiceEffect;

    private void Awake()
    {
        fruitRigidbody = GetComponent<Rigidbody>();
        fruitCollider = GetComponent<Collider>();
        juiceEffect = GetComponentInChildren<ParticleSystem>();
    }

    private void Slice(Vector3 direction, Vector3 position, float force)
    {
        GameManager.Instance.IncreaseScore(points);

        if (JuiceSplatterManager.Instance != null)
        {
            JuiceSplatterManager.Instance.SpawnSplatter(transform.position, juiceColor);
        }

        fruitCollider.enabled = false;
        whole.SetActive(false);

        sliced.SetActive(true);
        juiceEffect.Play();

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        sliced.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Rigidbody[] slices = sliced.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody slice in slices)
        {
            slice.velocity = fruitRigidbody.velocity;
            slice.AddForceAtPosition(direction * force, position, ForceMode.Impulse);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Blade blade = other.GetComponent<Blade>();
            Slice(blade.Direction, blade.transform.position, blade.SliceForce);
        }
    }

}

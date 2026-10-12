using UnityEngine;
using UnityEngine.Events;

public class MenuFruit : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float ringRotationSpeed = 20f;

    [Header("Action")]
    public UnityEvent onSliced = new UnityEvent();
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem sliceEffect;
    public GameObject wholeModel;
    public GameObject slicedModel;

    private bool isSliced = false;

    [Header("Ring Reference")]
    public Transform ring;

    private void Awake()
    {
        // Dynamically find ring if not assigned in Inspector
        if (ring == null)
        {
            // If MenuFruit is on Watermelon(Clone), Ring is at parent/Canvas/Ring
            if (transform.parent != null)
            {
                ring = transform.parent.Find("Canvas/Ring");
            }
            
            // Fallback if MenuFruit is directly on CenterMode
            if (ring == null)
            {
                ring = transform.Find("Canvas/Ring") ?? transform.Find("Ring");
            }
        }
    }

    private void Update()
    {
        if (!isSliced)
        {
            // Watermelon/fruit independent rotation
            if (wholeModel != null)
            {
                wholeModel.transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime, Space.Self);
            }

            // Blue ORIGINAL ring continuous rotation (Z-axis)
            bool isCenterMode = gameObject.name == "CenterMode" || (transform.parent != null && transform.parent.name == "CenterMode");
            if (isCenterMode && ring != null)
            {
                // Rotate clockwise (negative Z) at 20 degrees/second
                ring.Rotate(0f, 0f, -ringRotationSpeed * Time.deltaTime, Space.Self);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isSliced) return;

        if (other.CompareTag("Player")) // Blade has Player tag
        {
            // Prevent single click/tap from slicing by ensuring blade is moving
            Blade blade = other.GetComponent<Blade>();
            if (blade != null && blade.Direction.magnitude > 0.05f)
            {
                Slice();
            }
        }
    }

    private void Slice()
    {
        isSliced = true;

        if (wholeModel != null) wholeModel.SetActive(false);
        if (slicedModel != null)
        {
            slicedModel.SetActive(true);
            
            // Add some simple explosion force to sliced parts
            Rigidbody[] parts = slicedModel.GetComponentsInChildren<Rigidbody>();
            foreach(var rb in parts)
            {
                rb.AddExplosionForce(200f, transform.position, 2f);
            }
        }

        if (sliceEffect != null) sliceEffect.Play();

        // Trigger the assigned menu action
        onSliced?.Invoke();
    }
}

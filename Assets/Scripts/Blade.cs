using UnityEngine;

public class Blade : MonoBehaviour
{
    [Header("Blade Settings")]
    [SerializeField] private float sliceForce = 5f;
    [SerializeField] private float minSliceVelocity = 0.01f;

    public Vector3 Direction { get; private set; }
    public bool Slicing { get; private set; }
    public float SliceForce => sliceForce;

    private Camera mainCamera;
    private Collider sliceCollider;
    private TrailRenderer sliceTrail;

    private void Awake()
    {
        mainCamera = Camera.main;
        sliceCollider = GetComponent<Collider>();
        sliceTrail = GetComponentInChildren<TrailRenderer>();
    }

    private void OnEnable()
    {
        StopSlice();
    }

    private void OnDisable()
    {
        StopSlice();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartSlice();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopSlice();
        }
        else if (Slicing)
        {
            ContinueSlice();
        }
    }

    private void StartSlice()
    {
        Vector3 position = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        position.z = 0f;
        transform.position = position;

        Slicing = true;
        sliceCollider.enabled = true;
        sliceTrail.enabled = true;
        sliceTrail.Clear();
    }

    private void StopSlice()
    {
        Slicing = false;
        sliceCollider.enabled = false;
        sliceTrail.enabled = false;
    }

    private void ContinueSlice()
    {
        Vector3 newPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        newPosition.z = 0f;
        Direction = newPosition - transform.position;

        float velocity = Direction.magnitude / Time.deltaTime;
        sliceCollider.enabled = velocity > minSliceVelocity;

        transform.position = newPosition;
    }

}

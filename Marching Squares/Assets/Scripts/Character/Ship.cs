using Procedural.Marching.Squares;
using UnityEngine;

public class Ship : MonoBehaviour
{
    [Header("Required components")]
    public VoxelMap map;
    public GameObject child;

    [Header("Movement settings")]
    public bool scrollMap;
    public float scrollSpeed = 0.1f;
    public bool applyVelocity;
    public float movementSpeed = 2f;
    public float rotationSpeed = 2f;

    [Header("Ship settings")]
    public bool godMode;

    private Vector2 inputDirection;
    private Rigidbody rigidbody;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        inputDirection = Vector2.right * Input.GetAxis("Horizontal") + Vector2.up * Input.GetAxis("Vertical");
        inputDirection.Normalize();

        if (map && inputDirection.sqrMagnitude != 0)
        {
            if(scrollMap)
                map.AddNoiseOffset(inputDirection * scrollSpeed);

            // Rotate with input direction
            RotateShipToward(inputDirection);
        }
    }

    private void FixedUpdate()
    {
        if(applyVelocity)
            rigidbody.velocity += (Vector3) (inputDirection * movementSpeed * Time.fixedDeltaTime);
    }

    private void RotateShipToward(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        child.transform.rotation = Quaternion.Lerp(child.transform.rotation, rotation, Time.deltaTime * rotationSpeed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!godMode)
        {
            Debug.Log("Game Over");
            Destroy(child);
        }
    }
}

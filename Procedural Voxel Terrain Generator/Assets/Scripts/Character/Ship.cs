using Procedural.Marching.Squares;
using UnityEngine;

public class Ship : MonoBehaviour
{
    [Header("Required components")]
    public VoxelMap map;
    public GameObject child;

    [Header("Ship settings")]
    public float rotationSpeed = 2f;

    private Vector2 inputDirection;
    
    private void Update()
    {
        inputDirection = Vector2.right * Input.GetAxis("Horizontal") + Vector2.up * Input.GetAxis("Vertical");
        inputDirection.Normalize();

        inputDirection *= 0.2f;

        if (map && inputDirection.sqrMagnitude != 0)
        {
            map.AddNoiseOffset(inputDirection);

            // Rotate with input direction
            RotateShipToward(inputDirection);
        }
    }

    private void RotateShipToward(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        child.transform.rotation = Quaternion.Lerp(child.transform.rotation, rotation, Time.deltaTime * rotationSpeed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Game Over");
        Destroy(gameObject);
    }
}

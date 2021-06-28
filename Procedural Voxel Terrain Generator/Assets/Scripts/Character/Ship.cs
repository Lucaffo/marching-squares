using Procedural.Marching.Squares;
using UnityEngine;

public class Ship : MonoBehaviour
{
    public VoxelMap map;

    private Vector2 inputDirection;
    
    private void Update()
    {
        inputDirection = Vector2.right * Input.GetAxis("Horizontal") + Vector2.up * Input.GetAxis("Vertical");
        inputDirection.Normalize();

        if (map)
        {
            map.AddNoiseOffset(inputDirection);
            Debug.Log(inputDirection);
        }
    }
}

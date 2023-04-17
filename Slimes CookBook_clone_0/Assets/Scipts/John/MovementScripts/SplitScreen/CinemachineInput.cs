using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using Cinemachine;

public class CinemachineInput : MonoBehaviour, AxisState.IInputAxisProvider
{

   
    public InputAction look;

    public float GetAxisValue(int axis)
    {
        switch (axis)
        {
            case 0: return look.ReadValue<Vector2>().x;
            case 1: return look.ReadValue<Vector2>().y;
            case 2: return look.ReadValue<float>();
        }

        return 0;
    }
}

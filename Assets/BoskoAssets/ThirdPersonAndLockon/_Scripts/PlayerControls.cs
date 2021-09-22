using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    [Header("Customizable Controls")]
    public string inputHorizontal = "Horizontal";
    public string inputVertical = "Vertical";
    public KeyCode grab = KeyCode.E;
    public KeyCode target = KeyCode.F;
    public KeyCode jump = KeyCode.Space;
    public KeyCode crouch = KeyCode.X;
    public KeyCode sprint = KeyCode.LeftShift;
}
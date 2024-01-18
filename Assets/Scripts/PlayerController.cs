using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float MinYaw = -360;
    public float MaxYaw = 360;
    public float MinPitch = -60;
    public float MaxPitch = 60;
    public float LookSensitivity = 1;

    public float MoveSpeed = 10;
    public float SprintSpeed = 30;
    private float currMoveSpeed = 0;

    protected CharacterController movementController;
    protected Camera playerCamera;

    protected bool isControlling;
    protected float yaw;
    protected float pitch;

    protected Vector3 velocity;


    protected virtual void Start()
    {

        movementController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();         

        isControlling = true;
    }

    protected virtual void Update()
    {

        if (Input.GetKeyDown(KeyCode.R))
            transform.position = new Vector3(3, 0, 0);

        Vector3 direction = Vector3.zero;
        direction += transform.forward * Input.GetAxisRaw("Vertical");
        direction += transform.right * Input.GetAxisRaw("Horizontal");

        direction.Normalize();

        if (movementController.isGrounded)
            velocity = Vector3.zero;
        else
            velocity += -transform.up * (9.81f * 10) * Time.deltaTime;

        if (Input.GetKey(KeyCode.LeftShift))
            currMoveSpeed = SprintSpeed;
        else
            currMoveSpeed = MoveSpeed;

        direction += velocity * Time.deltaTime;
        movementController.Move(direction * Time.deltaTime * currMoveSpeed);
    }

}
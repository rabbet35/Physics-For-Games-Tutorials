﻿using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private GameObject player = null;

    [SerializeField]
    private float accel = 10.0F;

    [SerializeField]
    private float jumpHeight = 1.0f;

    [SerializeField]
    private float mouseSensitivity = 3.5F;

    private Vector3 playerVelocity = new Vector3();

    private CharacterController controller = null;
    private Camera playerCam = null;
    private Transform playerBody = null;
    private Rigidbody playerCollider = null;
    private bool error = false;

    private float camRotX = 0.0F;
    private float camRotY = 0.0F;
    private float prevMouseX = 0.0F;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;

        if (player == null)
        {
            Debug.LogError("PlayerController is provided with null player object!");
            error = true;
            return;
        }

        controller = player.GetComponent<CharacterController>();

        if(controller == null)
        {
            Debug.LogError("PlayerController can not find character controller on provided player object!");
            error = true;
        }

        playerCam = player.GetComponentInChildren<Camera>();

        if(playerCam == null)
        {
            Debug.LogError("PlayerController can not find camera on provided player object!");
            error = true;
        }

        playerBody = player.GetComponent<Transform>();

        if (playerBody == null)
        {
            Debug.LogError("PlayerController can not find Transform on provided player object!");
            error = true;
        }

        playerCollider = player.GetComponent<Rigidbody>();

        if (playerCollider == null)
        {
            Debug.LogError("PlayerController can not find RigidBody on provided player object!");
            error = true;
        }
    }

    void Update()
    {
        if (error) return;

        updateCamera();

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        float moveJump = Input.GetAxis("Jump");

        Vector3 move = transform.right * moveHorizontal + transform.forward * moveVertical;

        playerVelocity += Physics.gravity * Time.deltaTime;

        if (controller.isGrounded)//If the player is on a solid surface
        {
            if (playerVelocity.y < 0.0F) playerVelocity.y = 0.0F;

            if (moveJump != 0.0F)//jump
            {
                playerVelocity.y += Mathf.Sqrt(jumpHeight * 3.0f * -Physics.gravity.y);
            }
        }
        controller.Move(move * accel * Time.deltaTime);
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //push other objects away to simulate physics from player, or player pushing objects
        collision.rigidbody.velocity = collision.rigidbody.velocity + (-collision.contacts[0].normal * 10.0F);
    }

    private void updateCamera()
    {
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        camRotX -= mouseY;
        // Clamp the camera to limit it from being able to look to far down, ending up looking behind the player
        // So we say you can look as far as straight up or straight down.
        camRotX = Mathf.Clamp(camRotX, -90f, 90f);

        Cursor.visible = false;
        playerCam.transform.localRotation = Quaternion.Euler(camRotX, 0f, 0f);
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;

        float deltaMouseX = mouseX - prevMouseX;
        camRotY += deltaMouseX;
        playerBody.Rotate(Vector3.up * camRotY);
        prevMouseX = mouseX;

    }
}
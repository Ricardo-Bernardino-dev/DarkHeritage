using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private float verticalVelocity;
    //private float groundedTimer;     //to allow rolling when going down ramps
    public float walkSpeed = 2.0f;
    public float runSpeed = 3.0f;
    public float jumpHeight = 1.0f;
    public float gravityValue = 9.81f;
    private float rollSpeedMultiplier = 2f; 
    private float rollDuration = 1f;        
    public Animator animator;
    public bool isRolling = false;
    private float rollTimer = 0f;
    private Vector3 rollDirection;
    private float storedSpeed;
    public Animator animator2;
    public CharacterController controller2;
    [SerializeField] private GameObject dwarf;
    private PlayerCombat playerCombat;
    public float rollCost = 20f;

    void Start()
    {
        controller = gameObject.GetComponentInParent<CharacterController>();
        playerCombat = GetComponent<PlayerCombat>();
    }

    void Update()
    {
        bool groundedPlayer = controller.isGrounded;
        /*if (groundedPlayer)
        {
            //cooldown interval to allow reliable rolling even when coming down ramps
            groundedTimer = 0.2f;
        }
        if (groundedTimer > 0)
        {
            groundedTimer -= Time.deltaTime;
        }*/

        //slam into the ground
        if (groundedPlayer && verticalVelocity < 0)
        {
            verticalVelocity = 0f;
        }

        //apply gravity
        verticalVelocity -= gravityValue * Time.deltaTime;

        Vector3 move;

        ClickRoll();

        if (!isRolling) //pnly update movement direction if not rolling
        {
            move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            if (move.magnitude > 1)
            {
                move.Normalize(); //normalize the move vector to ensure consistent speed
            }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                float speed;
                speed = dwarf.activeInHierarchy ? runSpeed - 0.5f : runSpeed;
                move *= speed;
            }
            else
            {
                float speed;
                speed = dwarf.activeInHierarchy ? walkSpeed - 0.5f : walkSpeed;
                move *= speed;
            }
            animator.SetFloat("Speed", move.magnitude);
            animator2.SetFloat("Speed", move.magnitude);

            //only align to motion if we are providing enough input
            if (move.magnitude > 0.3f)
            {
                gameObject.transform.forward = move;
            }
        }
        else
        {
            Roll();
            move = rollSpeedMultiplier * storedSpeed * rollDirection;
        }

        move.y = verticalVelocity;
        
        if (dwarf.activeInHierarchy)
        {
            controller2.Move(move * Time.deltaTime);
            controller.gameObject.transform.position = controller2.transform.position;
        } else {
            controller.Move(move * Time.deltaTime);
            controller2.gameObject.transform.position = controller.transform.position;
        }
    }

    void ClickRoll()
    {
        if (Input.GetButton("Jump") && !isRolling)
        {
            if (playerCombat.NoStaminaAlert(rollCost)) return;
            playerCombat.stamina -= rollCost;
            animator.SetTrigger("Roll");
            animator2.SetTrigger("Roll");
            isRolling = true;
            rollTimer = rollDuration;
            rollDirection = gameObject.transform.forward; 
            storedSpeed = dwarf.activeInHierarchy ? controller2.velocity.magnitude : controller.velocity.magnitude;
        }
    }

    void Roll()
    {
        rollTimer -= Time.deltaTime;
        rollSpeedMultiplier = 1f;
        if (rollTimer <= 0)
        {
            isRolling = false;
        }

        if (storedSpeed < 0.5f)
        {
            storedSpeed = 1.0f;
            if (rollTimer > 0.8f)
            {
                rollSpeedMultiplier = 0f;
            }
        }
        if (rollTimer <= 0.5f)
        {
            rollSpeedMultiplier = dwarf.activeInHierarchy ? 1f : 1.5f;
        }
        else if (rollTimer <= 0.8f && rollTimer >= 0.5f)
        {
            rollSpeedMultiplier = dwarf.activeInHierarchy ? 1.5f : 2.25f;
        }
    }
}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CameraManager cameraManager;
    public CharacterController controller;
    private float verticalVelocity;  
    public float walkSpeed = 2.0f;
    public float runSpeed = 3.0f;
    public float jumpHeight = 1.0f;
    private float gravityValue = 9.81f;
    private float rollSpeedMultiplier = 2f; 
    private float rollDuration = 1f;        
    public Animator animator;
    public bool isRolling = false;
    private float rollTimer = 0f;
    private Vector3 rollDirection;
    private float storedSpeed;
    public Animator animator2;
    public CharacterController controller2;
    public GameObject dwarf;
    private PlayerCombat playerCombat;
    public float rollCost = 20f;
    public Vector3 move;
    public bool isStunned = false;

    void Start()
    {
        controller = gameObject.GetComponentInParent<CharacterController>();
        playerCombat = GetComponent<PlayerCombat>();
    }

    void Update()
    {
        if (!playerCombat.isDead)
        {
            bool groundedPlayer = controller.isGrounded;
            bool groundedDwarf = controller2.isGrounded;

            //slam into the ground
            if (dwarf.activeInHierarchy)
            {
                if (groundedDwarf && verticalVelocity < 0)
                {
                    verticalVelocity = 0f;
                }
            }
            else
            {
                if (groundedPlayer && verticalVelocity < 0)
                {
                    verticalVelocity = 0f;
                }
            }
        
            //apply gravity
            verticalVelocity -= gravityValue * Time.deltaTime;

            ClickRoll();

            if (!cameraManager.isFreeCameraActive && !isStunned){

                if (!isRolling && !playerCombat.dwarfAttack)
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
                    if (move.magnitude > 0f)
                    {
                        gameObject.transform.forward = move;
                        controller2.gameObject.transform.forward = move;
                    }
                }
                else
                {
                    animator2.SetFloat("Speed", 0f);

                    if (isRolling)
                    {
                        Roll();
                        move = rollSpeedMultiplier * storedSpeed * rollDirection;
                    }
                }

                move.y = verticalVelocity;
                
                if (dwarf.activeInHierarchy)
                {
                    //if dwarf is active and not attacking move dwarf controller and copy the movement for the other one for consistency when switching character
                    if (!playerCombat.dwarfAttack) 
                    {
                        controller2.Move(move * Time.deltaTime);
                        controller.gameObject.transform.position = controller2.transform.position;
                    } 
                } 
                else 
                { // opposite of the above
                    controller.Move(move * Time.deltaTime);
                    controller2.gameObject.transform.position = controller.transform.position;
                }
            } 
            else 
            {
                //to allow rolling while free camera is active
                if (isRolling)
                {
                    Roll();
                    animator2.SetFloat("Speed", 0f);
                    move = rollSpeedMultiplier * storedSpeed * rollDirection;
                }
            }

            //if dwarf is attacking Move() is not used and the character controller follows the attack animation instead
            if (playerCombat.dwarfAttack) 
            {
                controller.gameObject.transform.position = controller2.transform.position;
                controller2.gameObject.transform.position = dwarf.transform.position;
            }
        }
        
    }

    void ClickRoll()
    {
        if (Input.GetButton("Jump") && !isRolling && !playerCombat.isAttacking && !isStunned)
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

        if (storedSpeed < 2f)
        {
            storedSpeed = 2.0f;
            if (rollTimer > 0.8f)
            {
                rollSpeedMultiplier = 0f;
            }
        }
        if (rollTimer <= 0.5f)
        {
            rollSpeedMultiplier = 1.5f;
        }
        else if (rollTimer <= 0.8f && rollTimer >= 0.5f)
        {
            rollSpeedMultiplier = 2.25f;
        }
    }

    public IEnumerator Stun(float duration, float damage)
    {
        if(!isStunned && !isRolling)
        {
            animator.SetFloat("Speed", 0f);
            playerCombat.health -= damage;
            isStunned = true;
            animator.SetTrigger("Stun");
            animator2.SetTrigger("Stun");
            StartCoroutine(playerCombat.blink.FlashWhite(0.5f));
            yield return new WaitForSeconds(duration+0.3f);
            isStunned = false;
            animator.SetTrigger("StunEnd");
            animator2.SetTrigger("StunEnd");
        }
        
    }
}



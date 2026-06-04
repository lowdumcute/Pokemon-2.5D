using UnityEngine;
using System;
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Encounter")]
    [SerializeField] [Range(1, 100)] private int encounterChance = 10;
    [SerializeField] private float encounterCooldown = 0.25f;

    private Rigidbody rb;

    private Vector2 input;
    private Vector3 moveDirection;
    private bool isInGrass;
    private float nextEncounterTime;

    // Lưu hướng cuối cùng
    private Vector2 lastMoveDirection = Vector2.down;
    
    [SerializeField] private LayerMask LongGrassLayer;
    public event Action OnEncounted;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (LongGrassLayer.value == 0)
        {
            LongGrassLayer = LayerMask.GetMask("GrassLayer", "Grasslayer");
        }

        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationZ;

        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void HandleUpdate()
    {
        HandleInput();
        HandleAnimation();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void LateUpdate()
    {
        Vector3 currentRotation = transform.rotation.eulerAngles;
        currentRotation.y = 0f;

        transform.rotation = Quaternion.Euler(currentRotation);
    }

    private void HandleInput()
    {
        input = PlayerController.Instance.MoveInput;

        moveDirection = new Vector3(
            input.x,
            0f,
            input.y
        );

        if (input != Vector2.zero)
        {
            lastMoveDirection = input;
        }
    }

    private void Move()
    {
        if (moveDirection != Vector3.zero)
        {
            rb.linearVelocity = new Vector3(
                moveDirection.x * moveSpeed,
                rb.linearVelocity.y,
                moveDirection.z * moveSpeed
            );
        }
        else
        {
            rb.linearVelocity = new Vector3(
                0f,
                rb.linearVelocity.y,
                0f
            );
        }
    }

    private void HandleAnimation()
    {
        // Giữ hướng cuối cùng khi idle, bao gồm góc chéo.
        animator.SetFloat("MoveX", lastMoveDirection.x);
        animator.SetFloat("MoveY", lastMoveDirection.y);

        // Chỉ dùng để chuyển state idle/move
        animator.SetFloat("Speed", input.sqrMagnitude);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (IsGrassCollider(other))
        {
            isInGrass = true;
            TryEncounter();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsGrassCollider(other))
        {
            isInGrass = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsGrassCollider(collision.collider))
        {
            isInGrass = true;
            TryEncounter();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (IsGrassCollider(collision.collider))
        {
            isInGrass = false;
        }
    }

    private bool IsGrassCollider(Collider other)
    {
        return other != null && ((1 << other.gameObject.layer) & LongGrassLayer) != 0;
    }

    private void TryEncounter()
    {
        if (!isInGrass || Time.time < nextEncounterTime)
            return;

        nextEncounterTime = Time.time + encounterCooldown;

        if (UnityEngine.Random.Range(1, 101) <= encounterChance)
        {
            Debug.Log("Encounter Triggered");
            OnEncounted?.Invoke();
        }
    }
}

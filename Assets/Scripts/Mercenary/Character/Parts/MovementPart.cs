using Akasha;
using UnityEngine;

/// <summary>
/// 캐릭터 이동을 담당하는 Part
/// </summary>
public class MovementPart<TEntity, TModel> : CharacterPartBase<TEntity, TModel>
    where TEntity : BaseEntity<TModel>
    where TModel : CharModel
{
    private CharacterController characterController;
    private Rigidbody rb;

    protected float currentMoveSpeed;
    protected Vector3 moveDirection;
    protected Vector3 velocity;

    // 중력
    protected float gravity = -9.81f;
    protected bool isGrounded;

    protected override void AtAwake()
    {
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
    }

    protected override void AtModelReady()
    {
        // MoveSpeed가 있는 경우 (PlayerModel 등)
        if (Model is PlayerModel playerModel)
        {
            currentMoveSpeed = playerModel.MoveSpeed.Value;
            playerModel.MoveSpeed.AddListener(OnMoveSpeedChanged);
        }
        else
        {
            // 기본 이동 속도
            currentMoveSpeed = 5f;
        }
    }

    private void OnMoveSpeedChanged(float newSpeed)
    {
        currentMoveSpeed = newSpeed;
    }

    /// <summary>
    /// 이동 처리 (CharacterController 사용)
    /// </summary>
    public virtual void Move(Vector3 direction)
    {
        if (!IsAlive || characterController == null)
            return;

        moveDirection = direction.normalized;

        // 중력 적용
        if (characterController.isGrounded)
        {
            velocity.y = -2f;
            isGrounded = true;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
            isGrounded = false;
        }

        // 이동
        Vector3 move = moveDirection * currentMoveSpeed + velocity;
        characterController.Move(move * Time.deltaTime);

        // 회전
        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    /// <summary>
    /// 특정 위치로 이동 (Rigidbody 사용)
    /// </summary>
    public virtual void MoveToPosition(Vector3 targetPosition)
    {
        if (!IsAlive || rb == null)
            return;

        Vector3 direction = (targetPosition - transform.position).normalized;
        rb.linearVelocity = direction * currentMoveSpeed;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    /// <summary>
    /// 정지
    /// </summary>
    public virtual void Stop()
    {
        moveDirection = Vector3.zero;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// 점프 (CharacterController 사용)
    /// </summary>
    public virtual void Jump(float jumpForce = 5f)
    {
        if (isGrounded && characterController != null)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    /// <summary>
    /// 텔레포트
    /// </summary>
    public virtual void Teleport(Vector3 position)
    {
        if (characterController != null)
        {
            characterController.enabled = false;
            transform.position = position;
            characterController.enabled = true;
        }
        else
        {
            transform.position = position;
        }
    }
}

using UnityEngine;

namespace ScienceProject.PlayerSystem
{
    /// <summary>SRP: 입력, 물리 이동, 시각 표현의 호출 순서를 조율한다.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInput), typeof(PlayerMove))]
    public sealed class Player : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private PlayerMove playerMove;
        [SerializeField] private PlayerAnimation playerAnimation;
        private Vector2 moveDirection;

        private void Reset()
        {
            FindComponents();
        }

        private void Awake()
        {
            FindComponents();
            if (playerInput == null || playerMove == null || playerAnimation == null)
            {
                Debug.LogError("Player의 입력/이동/애니메이션 연결을 확인해 주세요.", this);
                enabled = false;
            }
        }

        private void FindComponents()
        {
            if (playerInput == null) playerInput = GetComponent<PlayerInput>();
            if (playerMove == null) playerMove = GetComponent<PlayerMove>();
            if (playerAnimation == null) playerAnimation = GetComponentInChildren<PlayerAnimation>();
        }

        private void Update()
        {
            moveDirection = playerInput.ReadDirection();
            playerAnimation.SetMovement(moveDirection);
        }

        private void FixedUpdate()
        {
            playerMove.Move(moveDirection);
        }

        private void OnDisable()
        {
            moveDirection = Vector2.zero;
            if (playerMove != null) playerMove.Stop();
            if (playerAnimation != null) playerAnimation.SetMovement(Vector2.zero);
        }
    }
}

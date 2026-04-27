using UnityEngine;

namespace MacaRush
{
    public sealed class SimpleFollowCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 neckOffset = new Vector3(0f, 1.6f, 0f);

        [Header("Orbit")]
        [SerializeField] private float distance = 4.6f;
        [SerializeField] private float minPitch = -30f;
        [SerializeField] private float maxPitch = 62f;
        [SerializeField] private float mouseSensitivity = 2.1f;

        [Header("Smoothing")]
        [SerializeField] private float positionSmooth = 0.08f;

        [Header("Collision")]
        [SerializeField] private float collisionRadius = 0.24f;
        [SerializeField] private float collisionPadding = 0.16f;
        [SerializeField] private LayerMask collisionMask = ~0;

        [Header("Cursor")]
        [SerializeField] private bool lockCursor = true;

        private float yaw;
        private float pitch = 16f;
        private Vector3 velocity;
        private bool hasInitialized;

        private void Awake()
        {
            TryAcquireTarget();
            SetCursorState(true);
        }

        private void OnDisable()
        {
            SetCursorState(false);
        }

        private void LateUpdate()
        {
            if (target == null && !TryAcquireTarget()) return;

            if (lockCursor && Cursor.lockState != CursorLockMode.Locked)
            {
                SetCursorState(true);
            }

            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            var pivot = target.position + neckOffset;
            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            var idealPosition = pivot + rotation * new Vector3(0f, 0f, -distance);
            var finalPosition = ResolveCollision(pivot, idealPosition);

            if (!hasInitialized)
            {
                transform.position = finalPosition;
                hasInitialized = true;
            }
            else
            {
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    finalPosition,
                    ref velocity,
                    Mathf.Max(0.01f, positionSmooth));
            }

            transform.rotation = Quaternion.LookRotation(pivot - transform.position, Vector3.up);
        }

        public void Configure(Transform followTarget)
        {
            target = followTarget;
            if (target != null)
            {
                yaw = target.eulerAngles.y;
            }

            hasInitialized = false;
        }

        private bool TryAcquireTarget()
        {
            var player = FindFirstObjectByType<ThirdPersonPusherController>();
            if (player == null) return false;

            Configure(player.transform);
            return true;
        }

        private Vector3 ResolveCollision(Vector3 pivot, Vector3 desiredPosition)
        {
            var direction = desiredPosition - pivot;
            if (direction.sqrMagnitude < 0.0001f) return desiredPosition;

            var hits = Physics.SphereCastAll(
                pivot,
                collisionRadius,
                direction.normalized,
                direction.magnitude,
                collisionMask,
                QueryTriggerInteraction.Ignore);

            var closest = float.MaxValue;
            for (var i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (target != null && hit.transform.IsChildOf(target)) continue;
                if (hit.distance < closest)
                {
                    closest = hit.distance;
                }
            }

            if (closest == float.MaxValue) return desiredPosition;
            return pivot + direction.normalized * Mathf.Max(0.55f, closest - collisionPadding);
        }

        private void SetCursorState(bool locked)
        {
            if (!lockCursor) return;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}

using UnityEngine;

namespace MacaRush
{
    public sealed class SimpleFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 pivotOffset = new Vector3(0f, 1.8f, 0f);
        [SerializeField] private float distance = 5.2f;
        [SerializeField] private float minPitch = -25f;
        [SerializeField] private float maxPitch = 55f;
        [SerializeField] private float mouseSensitivity = 2.2f;
        [SerializeField] private float positionLerp = 12f;
        [SerializeField] private float collisionRadius = 0.28f;
        [SerializeField] private float collisionPadding = 0.2f;
        [SerializeField] private LayerMask collisionMask = ~0;
        [SerializeField] private float forwardLookAhead = 1.6f;
        [SerializeField] private float stretcherFrameWeight = 0.35f;
        [SerializeField] private float maxStretcherFrameDistance = 5.5f;

        private float yaw;
        private float pitch = 18f;
        private bool hasSnappedToTarget;

        private void Awake()
        {
            TryAcquireTarget();
        }

        private void LateUpdate()
        {
            if (target == null && !TryAcquireTarget()) return;

            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            var desiredPosition = CalculateDesiredPosition(out var pivot);

            transform.position = hasSnappedToTarget
                ? Vector3.Lerp(transform.position, desiredPosition, positionLerp * Time.deltaTime)
                : desiredPosition;
            transform.rotation = Quaternion.LookRotation(pivot - transform.position, Vector3.up);
            hasSnappedToTarget = true;
        }

        public void Configure(Transform followTarget)
        {
            target = followTarget;
            hasSnappedToTarget = false;

            if (target != null)
            {
                yaw = target.eulerAngles.y;
            }
        }

        private bool TryAcquireTarget()
        {
            var player = FindFirstObjectByType<ThirdPersonPusherController>();
            if (player == null) return false;

            Configure(player.transform);
            return true;
        }

        private Vector3 CalculateDesiredPosition(out Vector3 pivot)
        {
            pivot = BuildPivot();
            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            var desiredPosition = pivot + rotation * new Vector3(0f, 0f, -distance);
            var direction = desiredPosition - pivot;

            if (TryFindCameraCollision(pivot, direction, out var hitDistance))
            {
                desiredPosition = pivot + direction.normalized * Mathf.Max(0.5f, hitDistance - collisionPadding);
            }

            return desiredPosition;
        }

        private Vector3 BuildPivot()
        {
            var basePivot = target.position + pivotOffset + target.forward * forwardLookAhead;

            var stretcher = GameManager.Instance != null && GameManager.Instance.Stretcher != null
                ? GameManager.Instance.Stretcher.transform
                : null;

            if (stretcher == null) return basePivot;

            var distance = Vector3.Distance(target.position, stretcher.position);
            if (distance > maxStretcherFrameDistance) return basePivot;

            return Vector3.Lerp(basePivot, stretcher.position + Vector3.up * 0.8f, stretcherFrameWeight);
        }

        private bool TryFindCameraCollision(Vector3 pivot, Vector3 direction, out float hitDistance)
        {
            hitDistance = 0f;
            if (direction.sqrMagnitude <= 0.001f) return false;

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

            if (closest == float.MaxValue) return false;

            hitDistance = closest;
            return true;
        }
    }
}

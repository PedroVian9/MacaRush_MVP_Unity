using UnityEngine;

namespace MacaRush
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ThirdPersonPusherController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

        [Header("Movement")]
        [SerializeField] private float moveForce = 52f;
        [SerializeField] private float maxSpeed = 5f;
        [SerializeField] private float sprintMultiplier = 1.35f;
        [SerializeField] private float turnSpeed = 14f;
        [SerializeField] private float pushForce = 34f;
        [SerializeField] private float pushMassLimit = 40f;

        [Header("Stamina")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float sprintDrainPerSecond = 20f;
        [SerializeField] private float recoverPerSecond = 16f;
        [SerializeField] private float exhaustedThreshold = 12f;
        [SerializeField] private float exhaustedMoveMultiplier = 0.7f;

        [Header("Camera Relative")]
        [SerializeField] private Transform cameraPivot;

        private Rigidbody rb;
        private Vector3 rawInput;
        private Vector3 worldInput;
        private float stamina;
        private float externalControlMultiplier = 1f;
        private float externalControlTimer;

        public float Stamina01 => maxStamina <= 0f ? 0f : stamina / maxStamina;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            stamina = maxStamina;
        }

        private void Update()
        {
            ReadInput();
            UpdateStamina();
            UpdateExternalControl();
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

            var sprint = IsSprinting() ? sprintMultiplier : 1f;
            var exhaustedScale = stamina > exhaustedThreshold ? 1f : exhaustedMoveMultiplier;
            var finalScale = sprint * exhaustedScale * externalControlMultiplier;

            rb.AddForce(worldInput * (moveForce * finalScale), ForceMode.Acceleration);

            var flat = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            var speedCap = maxSpeed * finalScale;
            if (flat.magnitude > speedCap)
            {
                var limited = flat.normalized * speedCap;
                rb.velocity = new Vector3(limited.x, rb.velocity.y, limited.z);
            }

            if (worldInput.sqrMagnitude > 0.02f)
            {
                var target = Quaternion.LookRotation(worldInput, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, target, turnSpeed * Time.fixedDeltaTime);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (worldInput.sqrMagnitude < 0.05f) return;
            if (collision.rigidbody == null || collision.rigidbody.isKinematic) return;
            if (collision.rigidbody.mass > pushMassLimit) return;

            collision.rigidbody.AddForce(worldInput * pushForce, ForceMode.Acceleration);
        }

        public void Configure(Transform pivot)
        {
            cameraPivot = pivot;
        }

        public void ApplyTemporaryControlModifier(float multiplier, float duration)
        {
            externalControlMultiplier = Mathf.Min(externalControlMultiplier, Mathf.Clamp01(multiplier));
            externalControlTimer = Mathf.Max(externalControlTimer, duration);
        }

        private void ReadInput()
        {
            rawInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            if (rawInput.sqrMagnitude > 1f)
            {
                rawInput.Normalize();
            }

            var pivot = cameraPivot != null ? cameraPivot : Camera.main != null ? Camera.main.transform : null;
            if (pivot == null)
            {
                worldInput = rawInput;
                return;
            }

            var forward = pivot.forward;
            var right = pivot.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            worldInput = (forward * rawInput.z + right * rawInput.x);
            if (worldInput.sqrMagnitude > 1f)
            {
                worldInput.Normalize();
            }
        }

        private void UpdateStamina()
        {
            if (IsSprinting() && rawInput.sqrMagnitude > 0.01f)
            {
                stamina = Mathf.Max(0f, stamina - sprintDrainPerSecond * Time.deltaTime);
            }
            else
            {
                stamina = Mathf.Min(maxStamina, stamina + recoverPerSecond * Time.deltaTime);
            }
        }

        private bool IsSprinting()
        {
            return Input.GetKey(sprintKey) && stamina > exhaustedThreshold;
        }

        private void UpdateExternalControl()
        {
            if (externalControlTimer > 0f)
            {
                externalControlTimer -= Time.deltaTime;
                return;
            }

            externalControlMultiplier = Mathf.MoveTowards(externalControlMultiplier, 1f, 6f * Time.deltaTime);
        }
    }
}

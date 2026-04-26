using UnityEngine;

namespace MacaRush
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PlayerCarryController : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private int playerIndex = 1;
        [SerializeField] private float moveForce = 42f;
        [SerializeField] private float maxSpeed = 4.4f;
        [SerializeField] private float sprintMultiplier = 1.35f;
        [SerializeField] private float carryEfficiency = 0.88f;
        [SerializeField] private float exhaustedEfficiency = 0.55f;
        [SerializeField] private float lowStaminaThreshold = 0.25f;
        [SerializeField] private float turnSpeed = 12f;

        [Header("Stamina")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float sprintDrainPerSecond = 22f;
        [SerializeField] private float carryDrainPerSecond = 7f;
        [SerializeField] private float recoverPerSecond = 16f;
        [SerializeField] private float exhaustedSprintBlock = 8f;

        [Header("Carry")]
        [SerializeField] private MacaHandle currentHandle;
        [SerializeField] private KeyCode grabKey = KeyCode.E;
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

        [Header("Pushing")]
        [SerializeField] private float pushForce = 20f;
        [SerializeField] private float pushableMassLimit = 12f;
        [SerializeField] private float externalControlRecovery = 6f;

        [Header("Keyboard Input")]
        [SerializeField] private KeyCode upKey = KeyCode.W;
        [SerializeField] private KeyCode downKey = KeyCode.S;
        [SerializeField] private KeyCode leftKey = KeyCode.A;
        [SerializeField] private KeyCode rightKey = KeyCode.D;

        private Rigidbody rb;
        private float stamina;
        private Vector3 input;
        private float externalControlMultiplier = 1f;
        private float externalControlTimer;

        public int PlayerIndex => playerIndex;
        public float Stamina01 => maxStamina <= 0f ? 0f : stamina / maxStamina;
        public float CurrentStamina => stamina;
        public Vector3 MovementInput => input;
        public float EffectivePower01 => CalculateEfficiency();
        public bool IsHolding => currentHandle != null && currentHandle.Holder == this;
        public MacaHandle CurrentHandle => currentHandle;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
            stamina = maxStamina;
        }

        private void OnDisable()
        {
            if (IsHolding)
            {
                currentHandle.Release(this);
                currentHandle = null;
            }
        }

        private void Update()
        {
            ReadKeyboardInput();
            UpdateExternalControl();

            if (Input.GetKeyDown(grabKey))
            {
                if (IsHolding)
                {
                    currentHandle.Release(this);
                    currentHandle = null;
                }
                else
                {
                    TryGrabNearestHandle();
                }
            }

            UpdateStamina();
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

            var sprint = IsSprinting() ? sprintMultiplier : 1f;
            var efficiency = CalculateEfficiency();
            rb.AddForce(input * (moveForce * sprint * efficiency), ForceMode.Acceleration);

            var horizontalVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            var speedLimit = maxSpeed * sprint * Mathf.Lerp(0.75f, 1f, efficiency);
            if (horizontalVelocity.magnitude > speedLimit)
            {
                var limited = horizontalVelocity.normalized * speedLimit;
                rb.velocity = new Vector3(limited.x, rb.velocity.y, limited.z);
            }

            if (input.sqrMagnitude > 0.05f)
            {
                var targetRotation = Quaternion.LookRotation(input, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
            }
        }

        private void ReadKeyboardInput()
        {
            var x = 0f;
            var z = 0f;

            if (Input.GetKey(leftKey)) x -= 1f;
            if (Input.GetKey(rightKey)) x += 1f;
            if (Input.GetKey(downKey)) z -= 1f;
            if (Input.GetKey(upKey)) z += 1f;

            input = new Vector3(x, 0f, z);
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }
        }

        private void UpdateStamina()
        {
            var sprinting = IsSprinting();
            var draining = 0f;

            if (sprinting)
            {
                draining += sprintDrainPerSecond;
            }

            if (IsHolding && input.sqrMagnitude > 0.01f)
            {
                draining += carryDrainPerSecond;
            }

            if (draining > 0f)
            {
                stamina = Mathf.Max(0f, stamina - draining * Time.deltaTime);
            }
            else
            {
                stamina = Mathf.Min(maxStamina, stamina + recoverPerSecond * Time.deltaTime);
            }
        }

        private void UpdateExternalControl()
        {
            if (externalControlTimer > 0f)
            {
                externalControlTimer -= Time.deltaTime;
                return;
            }

            externalControlMultiplier = Mathf.MoveTowards(
                externalControlMultiplier,
                1f,
                externalControlRecovery * Time.deltaTime);
        }

        private bool IsSprinting()
        {
            return Input.GetKey(sprintKey) && stamina > exhaustedSprintBlock && input.sqrMagnitude > 0.01f;
        }

        private float CalculateEfficiency()
        {
            var staminaEfficiency = Stamina01 >= lowStaminaThreshold
                ? 1f
                : Mathf.Lerp(exhaustedEfficiency, 1f, Stamina01 / Mathf.Max(0.01f, lowStaminaThreshold));

            var holdingEfficiency = IsHolding ? carryEfficiency : 1f;
            return Mathf.Clamp01(staminaEfficiency * holdingEfficiency * externalControlMultiplier);
        }

        private void TryGrabNearestHandle()
        {
            var handles = FindObjectsByType<MacaHandle>(FindObjectsSortMode.None);

            MacaHandle best = null;
            var bestDistance = float.MaxValue;

            foreach (var handle in handles)
            {
                if (!handle.CanBeHeldBy(this)) continue;

                var distance = Vector3.Distance(transform.position, handle.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = handle;
                }
            }

            if (best != null)
            {
                best.Grab(this);
                currentHandle = best;
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (input.sqrMagnitude < 0.05f) return;
            if (collision.rigidbody == null || collision.rigidbody.isKinematic) return;
            if (collision.rigidbody.mass > pushableMassLimit) return;
            if (currentHandle != null && collision.rigidbody == currentHandle.MacaRigidbody) return;

            var pushDirection = new Vector3(input.x, 0f, input.z).normalized;
            collision.rigidbody.AddForce(pushDirection * pushForce, ForceMode.Acceleration);
        }

        public void ApplyTemporaryControlModifier(float multiplier, float duration)
        {
            externalControlMultiplier = Mathf.Min(externalControlMultiplier, Mathf.Clamp01(multiplier));
            externalControlTimer = Mathf.Max(externalControlTimer, duration);
        }

        public void Configure(
            int index,
            KeyCode up,
            KeyCode down,
            KeyCode left,
            KeyCode right,
            KeyCode grab,
            KeyCode sprint)
        {
            playerIndex = index;
            upKey = up;
            downKey = down;
            leftKey = left;
            rightKey = right;
            grabKey = grab;
            sprintKey = sprint;
        }
    }
}

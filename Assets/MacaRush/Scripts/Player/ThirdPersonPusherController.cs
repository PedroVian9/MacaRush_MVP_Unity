using UnityEngine;

namespace MacaRush
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ThirdPersonPusherController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode grabKey = KeyCode.E;

        [Header("Movement")]
        [SerializeField] private float moveForce = 38f;
        [SerializeField] private float maxSpeed = 3.8f;
        [SerializeField] private float sprintMultiplier = 1.2f;
        [SerializeField] private float turnSpeed = 14f;
        [SerializeField] private float pushForce = 34f;
        [SerializeField] private float pushMassLimit = 40f;
        [SerializeField] private float holdingMoveMultiplier = 0.82f;

        [Header("Stamina")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float sprintDrainPerSecond = 20f;
        [SerializeField] private float carryDrainPerSecond = 8f;
        [SerializeField] private float recoverPerSecond = 16f;
        [SerializeField] private float exhaustedThreshold = 12f;
        [SerializeField] private float exhaustedMoveMultiplier = 0.7f;

        [Header("Grab Stretcher")]
        [SerializeField] private float grabRadius = 2.1f;
        [SerializeField] private float snapDistance = 1.1f;
        [SerializeField] private float holdSlack = 0.45f;
        [SerializeField] private float holdSpring = 880f;
        [SerializeField] private float holdDamper = 74f;
        [SerializeField] private float holdForce = 3200f;
        [SerializeField] private float holdingDriveForce = 22f;

        [Header("Camera Relative")]
        [SerializeField] private Transform cameraPivot;

        private Rigidbody rb;
        private Vector3 rawInput;
        private Vector3 worldInput;
        private float stamina;
        private float externalControlMultiplier = 1f;
        private float externalControlTimer;
        private float pushingTimer;
        private ConfigurableJoint holdJoint;
        private Rigidbody heldStretcherBody;
        private MacaStretcher heldStretcher;

        public float Stamina01 => maxStamina <= 0f ? 0f : stamina / maxStamina;
        public Vector3 MoveDirection => worldInput;
        public float MoveAmount => Mathf.Clamp01(rawInput.magnitude);
        public bool IsPushing => pushingTimer > 0f || IsHolding;
        public bool IsHolding => holdJoint != null && heldStretcherBody != null;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            stamina = maxStamina;
        }

        private void OnDisable()
        {
            ReleaseStretcher();
        }

        private void Update()
        {
            if (Input.GetKeyDown(grabKey))
            {
                if (IsHolding)
                {
                    ReleaseStretcher();
                }
                else
                {
                    TryGrabNearestStretcher();
                }
            }

            ReadInput();
            UpdateStamina();
            UpdateExternalControl();
            pushingTimer = Mathf.Max(0f, pushingTimer - Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

            var sprint = IsSprinting() ? sprintMultiplier : 1f;
            var exhaustedScale = stamina > exhaustedThreshold ? 1f : exhaustedMoveMultiplier;
            var holdingScale = IsHolding ? holdingMoveMultiplier : 1f;
            var finalScale = sprint * exhaustedScale * externalControlMultiplier * holdingScale;

            rb.AddForce(worldInput * (moveForce * finalScale), ForceMode.Acceleration);

            if (IsHolding)
            {
                heldStretcherBody.AddForce(worldInput * (holdingDriveForce * finalScale), ForceMode.Acceleration);
            }

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
            if (IsHolding && collision.rigidbody == heldStretcherBody) return;

            collision.rigidbody.AddForce(worldInput * pushForce, ForceMode.Acceleration);
            pushingTimer = 0.15f;
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

        private void TryGrabNearestStretcher()
        {
            var stretchers = FindObjectsByType<MacaStretcher>(FindObjectsSortMode.None);
            MacaStretcher best = null;
            var bestDistance = float.MaxValue;

            for (var i = 0; i < stretchers.Length; i++)
            {
                var candidate = stretchers[i];
                if (candidate == null || candidate.Body == null) continue;

                var distance = Vector3.Distance(transform.position, candidate.transform.position);
                if (distance <= grabRadius && distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }

            if (best == null) return;

            var bestCollider = best.GetComponent<Collider>();
            var anchorWorld = bestCollider != null ? bestCollider.ClosestPoint(transform.position) : best.transform.position;
            var snapDirection = (anchorWorld - transform.position);
            snapDirection.y = 0f;

            if (snapDirection.sqrMagnitude > 0.0001f)
            {
                transform.position = anchorWorld - snapDirection.normalized * snapDistance + Vector3.up * 0.02f;
            }

            heldStretcher = best;
            heldStretcherBody = best.Body;
            CreateHoldJoint(anchorWorld);
            pushingTimer = 0.2f;
        }

        private void CreateHoldJoint(Vector3 anchorWorld)
        {
            holdJoint = gameObject.AddComponent<ConfigurableJoint>();
            holdJoint.connectedBody = heldStretcherBody;
            holdJoint.autoConfigureConnectedAnchor = false;
            holdJoint.anchor = Vector3.zero;
            holdJoint.connectedAnchor = heldStretcherBody.transform.InverseTransformPoint(anchorWorld);
            holdJoint.enableCollision = false;

            holdJoint.xMotion = ConfigurableJointMotion.Limited;
            holdJoint.yMotion = ConfigurableJointMotion.Limited;
            holdJoint.zMotion = ConfigurableJointMotion.Limited;
            holdJoint.angularXMotion = ConfigurableJointMotion.Locked;
            holdJoint.angularYMotion = ConfigurableJointMotion.Locked;
            holdJoint.angularZMotion = ConfigurableJointMotion.Locked;
            holdJoint.linearLimit = new SoftJointLimit { limit = holdSlack };

            var drive = new JointDrive
            {
                positionSpring = holdSpring,
                positionDamper = holdDamper,
                maximumForce = holdForce
            };

            holdJoint.xDrive = drive;
            holdJoint.yDrive = drive;
            holdJoint.zDrive = drive;
        }

        private void ReleaseStretcher()
        {
            if (holdJoint != null)
            {
                Destroy(holdJoint);
            }

            holdJoint = null;
            heldStretcherBody = null;
            heldStretcher = null;
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
            var draining = 0f;
            if (IsSprinting() && rawInput.sqrMagnitude > 0.01f)
            {
                draining += sprintDrainPerSecond;
            }

            if (IsHolding && rawInput.sqrMagnitude > 0.01f)
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

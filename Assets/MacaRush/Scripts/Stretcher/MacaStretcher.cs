using UnityEngine;

namespace MacaRush
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class MacaStretcher : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PatientHealth patientHealth;
        [SerializeField] private Transform patientAnchor;

        [Header("Body Tuning")]
        [SerializeField] private float mass = 26f;
        [SerializeField] private float linearDamping = 0.35f;
        [SerializeField] private float angularDamping = 1.45f;
        [SerializeField] private Vector3 centerOfMassOffset = new Vector3(0f, -0.22f, 0f);
        [SerializeField] private float maxAngularVelocity = 5.5f;

        [Header("Impact Damage")]
        [SerializeField] private float minimumImpactVelocity = 3.2f;
        [SerializeField] private float impactDamageMultiplier = 7.5f;
        [SerializeField] private float eventImpactCooldown = 0.45f;

        [Header("Tilt Damage")]
        [SerializeField] private float dangerousTiltAngle = 35f;
        [SerializeField] private float tiltDamagePerSecond = 8f;
        [SerializeField] private float flippedAngle = 82f;
        [SerializeField] private float flippedSecondsToDefeat = 2.4f;
        [SerializeField] private float patientFallTiltAngle = 68f;
        [SerializeField] private float patientFallSeconds = 2.8f;

        [Header("Assist")]
        [SerializeField] private float uprightAssist = 5.5f;
        [SerializeField] private float curveSwayTorque = 0.18f;
        [SerializeField] private float fallWorldY = -4f;

        private Rigidbody rb;
        private float flippedTimer;
        private float patientFallTimer;
        private float impactCooldownTimer;

        public Rigidbody Body => rb;
        public PatientHealth Patient => patientHealth;
        public float TiltAngle => Vector3.Angle(transform.up, Vector3.up);
        public bool IsDangerouslyTilted => TiltAngle >= dangerousTiltAngle;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            ConfigureRigidbody();
        }

        private void FixedUpdate()
        {
            if (!IsPlaying()) return;

            ApplyUprightAssist();
            ApplyCurveSway();
        }

        private void Update()
        {
            if (!IsPlaying()) return;

            impactCooldownTimer = Mathf.Max(0f, impactCooldownTimer - Time.deltaTime);
            UpdateTiltDamage();
            UpdateDefeatChecks();
        }

        private void OnValidate()
        {
            if (Application.isPlaying && rb != null)
            {
                ConfigureRigidbody();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (patientHealth == null) return;
            if (impactCooldownTimer > 0f) return;

            var impactVelocity = collision.relativeVelocity.magnitude;
            if (impactVelocity < minimumImpactVelocity) return;

            var damage = (impactVelocity - minimumImpactVelocity) * impactDamageMultiplier;
            patientHealth.ApplyDamage(damage, PatientDamageSource.Impact);
            impactCooldownTimer = eventImpactCooldown;
        }

        public void Configure(PatientHealth patient, Transform anchor)
        {
            patientHealth = patient;
            patientAnchor = anchor;
        }

        public void ApplyPatientMovementImpulse(float impulse, float damage)
        {
            if (rb == null) return;

            var lateral = transform.right * Random.Range(-1f, 1f);
            var twist = transform.forward * Random.Range(-1f, 1f);
            rb.AddForceAtPosition((lateral + twist).normalized * impulse, transform.position + Vector3.up * 0.35f, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * impulse * 0.35f, ForceMode.Impulse);

            if (patientHealth != null)
            {
                patientHealth.ApplyDamage(damage, PatientDamageSource.Event);
            }
        }

        private void ConfigureRigidbody()
        {
            rb.mass = mass;
            rb.centerOfMass = centerOfMassOffset;
            rb.maxAngularVelocity = maxAngularVelocity;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

#if UNITY_6000_0_OR_NEWER
            rb.linearDamping = linearDamping;
            rb.angularDamping = angularDamping;
#else
            rb.drag = linearDamping;
            rb.angularDrag = angularDamping;
#endif
        }

        private void UpdateTiltDamage()
        {
            if (patientHealth == null) return;

            var tilt = TiltAngle;
            if (tilt >= dangerousTiltAngle)
            {
                var severity = Mathf.InverseLerp(dangerousTiltAngle, flippedAngle, tilt);
                patientHealth.ApplyDamage(tiltDamagePerSecond * Mathf.Lerp(0.65f, 1.55f, severity) * Time.deltaTime, PatientDamageSource.Tilt);
            }
        }

        private void UpdateDefeatChecks()
        {
            var tilt = TiltAngle;

            flippedTimer = tilt >= flippedAngle ? flippedTimer + Time.deltaTime : 0f;
            if (flippedTimer >= flippedSecondsToDefeat)
            {
                DefeatIfPossible("A maca virou por tempo demais.");
                return;
            }

            patientFallTimer = tilt >= patientFallTiltAngle ? patientFallTimer + Time.deltaTime : 0f;
            if (patientFallTimer >= patientFallSeconds)
            {
                DefeatIfPossible("Paciente caiu da maca.");
                return;
            }

            if (patientAnchor != null && patientAnchor.position.y <= fallWorldY)
            {
                if (patientHealth != null)
                {
                    patientHealth.ApplyDamage(patientHealth.MaxHealth, PatientDamageSource.Fall);
                }

                DefeatIfPossible("Paciente caiu fora do mapa.");
            }
        }

        private void ApplyUprightAssist()
        {
            var tilt = TiltAngle;
            if (tilt <= 2f || tilt >= flippedAngle) return;

            var correctionAxis = Vector3.Cross(transform.up, Vector3.up);
            var assistScale = Mathf.InverseLerp(flippedAngle, 8f, tilt);
            rb.AddTorque(correctionAxis * (uprightAssist * assistScale), ForceMode.Acceleration);
        }

        private void ApplyCurveSway()
        {
            var localVelocity = transform.InverseTransformDirection(rb.velocity);
            var sway = Mathf.Clamp(localVelocity.x, -4f, 4f) * curveSwayTorque;
            rb.AddRelativeTorque(Vector3.forward * -sway, ForceMode.Acceleration);
        }

        private static bool IsPlaying()
        {
            return GameManager.Instance == null || GameManager.Instance.State == GameState.Playing;
        }

        private static void DefeatIfPossible(string reason)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Defeat(reason);
            }
        }
    }
}

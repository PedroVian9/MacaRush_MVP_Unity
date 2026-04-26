using UnityEngine;

namespace MacaRush
{
    [RequireComponent(typeof(Collider))]
    public sealed class MovingObstacle : MonoBehaviour
    {
        [SerializeField] private Vector3 localPointA;
        [SerializeField] private Vector3 localPointB = new Vector3(4f, 0f, 0f);
        [SerializeField] private float speed = 1.5f;
        [SerializeField] private bool active = true;
        [SerializeField] private bool startAtRandomPhase = true;

        private Rigidbody rb;
        private Vector3 origin;
        private float phaseOffset;
        private float forcedActiveTimer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            origin = transform.position;
            phaseOffset = startAtRandomPhase ? Random.Range(0f, 10f) : 0f;
        }

        private void FixedUpdate()
        {
            forcedActiveTimer = Mathf.Max(0f, forcedActiveTimer - Time.fixedDeltaTime);
            if (!active && forcedActiveTimer <= 0f) return;

            var t = (Mathf.Sin((Time.time + phaseOffset) * speed) + 1f) * 0.5f;
            var target = origin + Vector3.Lerp(localPointA, localPointB, t);

            if (rb != null && rb.isKinematic)
            {
                rb.MovePosition(target);
            }
            else
            {
                transform.position = target;
            }
        }

        public void TriggerBurst(float duration)
        {
            forcedActiveTimer = Mathf.Max(forcedActiveTimer, duration);
        }

        public void Configure(Vector3 pointA, Vector3 pointB, float moveSpeed, bool startsActive)
        {
            localPointA = pointA;
            localPointB = pointB;
            speed = moveSpeed;
            active = startsActive;
            origin = transform.position;
        }
    }
}

using UnityEngine;

namespace MacaRush
{
    [RequireComponent(typeof(Collider))]
    public sealed class BreakableGlass : MonoBehaviour
    {
        [Header("Break Settings")]
        [SerializeField] private float breakImpactThreshold = 4.5f;
        [SerializeField] private bool breakOnPlayerContact = true;
        [SerializeField] private bool breakOnStretcherContact = true;

        [Header("Shards")]
        [SerializeField] private int shardCount = 6;
        [SerializeField] private float shardLifetime = 2.4f;
        [SerializeField] private float shardImpulse = 3.2f;
        [SerializeField] private float shardUpwardImpulse = 1.35f;

        private bool isBroken;
        private Collider cachedCollider;
        private Renderer cachedRenderer;
        private Material shardMaterial;

        private void Awake()
        {
            cachedCollider = GetComponent<Collider>();
            cachedRenderer = GetComponent<Renderer>();
            shardMaterial = cachedRenderer != null ? cachedRenderer.sharedMaterial : null;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (isBroken || collision == null) return;
            if (!IsRelevant(collision.collider)) return;

            var impact = collision.relativeVelocity.magnitude;
            if (impact < breakImpactThreshold) return;

            var hitPoint = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
            Break(hitPoint, collision.relativeVelocity);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isBroken || other == null) return;
            if (!IsRelevant(other)) return;
            Break(transform.position, Vector3.zero);
        }

        public void Configure(float impactThreshold, int fragments, float impulse)
        {
            breakImpactThreshold = Mathf.Max(0.1f, impactThreshold);
            shardCount = Mathf.Clamp(fragments, 2, 18);
            shardImpulse = Mathf.Max(0.1f, impulse);
        }

        private bool IsRelevant(Collider other)
        {
            if (other == null) return false;
            if (breakOnPlayerContact && other.GetComponentInParent<ThirdPersonPusherController>() != null) return true;
            if (breakOnStretcherContact && other.GetComponentInParent<MacaStretcher>() != null) return true;
            return other.attachedRigidbody != null;
        }

        private void Break(Vector3 hitPoint, Vector3 impactVelocity)
        {
            isBroken = true;

            if (cachedCollider != null)
            {
                cachedCollider.enabled = false;
            }

            if (cachedRenderer != null)
            {
                cachedRenderer.enabled = false;
            }

            var baseDirection = impactVelocity.sqrMagnitude > 0.01f ? impactVelocity.normalized : transform.forward;
            SpawnShards(hitPoint, baseDirection);
        }

        private void SpawnShards(Vector3 hitPoint, Vector3 baseDirection)
        {
            var count = Mathf.Max(2, shardCount);
            var scale = transform.lossyScale;

            for (var i = 0; i < count; i++)
            {
                var shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.name = $"{name}_Shard_{i + 1}";
                shard.transform.position = transform.position + new Vector3(
                    Random.Range(-scale.x * 0.35f, scale.x * 0.35f),
                    Random.Range(-scale.y * 0.35f, scale.y * 0.35f),
                    Random.Range(-scale.z * 0.35f, scale.z * 0.35f));
                shard.transform.rotation = Random.rotation;
                shard.transform.localScale = new Vector3(
                    Mathf.Max(0.04f, scale.x * Random.Range(0.12f, 0.24f)),
                    Mathf.Max(0.04f, scale.y * Random.Range(0.12f, 0.24f)),
                    Mathf.Max(0.04f, scale.z * Random.Range(0.12f, 0.24f)));

                var renderer = shard.GetComponent<Renderer>();
                if (renderer != null && shardMaterial != null)
                {
                    renderer.sharedMaterial = shardMaterial;
                }

                var shardBody = shard.AddComponent<Rigidbody>();
                shardBody.mass = 0.08f;
                shardBody.collisionDetectionMode = CollisionDetectionMode.Continuous;

                var randomSpread = Random.insideUnitSphere * 0.6f;
                var direction = (baseDirection + randomSpread).normalized;
                var force = direction * shardImpulse + Vector3.up * shardUpwardImpulse;
                shardBody.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);

                Destroy(shard, shardLifetime);
            }

            Destroy(gameObject, 0.05f);
        }
    }
}

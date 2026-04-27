using UnityEngine;

namespace MacaRush
{
    [RequireComponent(typeof(Collider))]
    public sealed class SlipperyZone : MonoBehaviour
    {
        [SerializeField] private bool active = true;
        [SerializeField] private float playerControlMultiplier = 0.62f;
        [SerializeField] private float refreshDuration = 0.25f;
        [SerializeField] private float stretcherSideSlipForce = 2.5f;
        [SerializeField] private Material slipFxMaterial;

        private float activeTimer;
        private float fxTimer;

        public bool IsActive => active || activeTimer > 0f;

        private void Reset()
        {
            var zoneCollider = GetComponent<Collider>();
            zoneCollider.isTrigger = true;
        }

        private void Update()
        {
            activeTimer = Mathf.Max(0f, activeTimer - Time.deltaTime);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!IsActive) return;

            var carryPlayer = other.GetComponentInParent<PlayerCarryController>();
            if (carryPlayer != null)
            {
                carryPlayer.ApplyTemporaryControlModifier(playerControlMultiplier, refreshDuration);
                return;
            }

            var singlePlayer = other.GetComponentInParent<ThirdPersonPusherController>();
            if (singlePlayer != null)
            {
                singlePlayer.ApplyTemporaryControlModifier(playerControlMultiplier, refreshDuration);
                SpawnSlipFx(other.transform.position);
                return;
            }

            var stretcher = other.GetComponentInParent<MacaStretcher>();
            if (stretcher != null && stretcher.Body != null)
            {
                var side = Vector3.Cross(Vector3.up, stretcher.Body.velocity).normalized;
                stretcher.Body.AddForce(side * stretcherSideSlipForce, ForceMode.Acceleration);
            }
        }

        public void ActivateFor(float duration)
        {
            activeTimer = Mathf.Max(activeTimer, duration);
        }

        public void Configure(bool startsActive, float controlMultiplier)
        {
            active = startsActive;
            playerControlMultiplier = Mathf.Clamp01(controlMultiplier);
        }

        public void ConfigureFx(Material material)
        {
            slipFxMaterial = material;
        }

        private void SpawnSlipFx(Vector3 position)
        {
            fxTimer -= Time.deltaTime;
            if (fxTimer > 0f) return;

            fxTimer = 0.18f;
            var fx = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fx.name = "Slip FX";
            fx.transform.position = new Vector3(position.x, transform.position.y + 0.04f, position.z);
            fx.transform.localScale = new Vector3(0.35f, 0.015f, 0.35f);

            var collider = fx.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var renderer = fx.GetComponent<Renderer>();
            if (renderer != null && slipFxMaterial != null)
            {
                renderer.material = slipFxMaterial;
            }

            fx.AddComponent<TemporaryWorldFx>().Configure(0.38f, Vector3.up * 0.05f);
        }
    }
}

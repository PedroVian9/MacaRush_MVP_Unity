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

        private float activeTimer;

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

            var player = other.GetComponentInParent<PlayerCarryController>();
            if (player != null)
            {
                player.ApplyTemporaryControlModifier(playerControlMultiplier, refreshDuration);
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
    }
}

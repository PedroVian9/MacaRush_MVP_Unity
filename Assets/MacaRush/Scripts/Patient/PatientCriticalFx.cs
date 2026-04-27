using UnityEngine;

namespace MacaRush
{
    public sealed class PatientCriticalFx : MonoBehaviour
    {
        [SerializeField] private PatientHealth patient;
        [SerializeField] private Renderer indicatorRenderer;
        [SerializeField] private float pulseSpeed = 8f;
        [SerializeField] private float minScale = 0.75f;
        [SerializeField] private float maxScale = 1.25f;

        private Vector3 baseScale;

        private void Awake()
        {
            baseScale = transform.localScale;
        }

        private void Update()
        {
            if (patient == null || indicatorRenderer == null) return;

            var active = patient.State == PatientState.Critical || patient.State == PatientState.Dying;
            indicatorRenderer.enabled = active;
            if (!active) return;

            var pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            transform.localScale = baseScale * Mathf.Lerp(minScale, maxScale, pulse);
        }

        public void Configure(PatientHealth patientHealth, Renderer rendererOverride)
        {
            patient = patientHealth;
            indicatorRenderer = rendererOverride;
        }
    }
}

using UnityEngine;

namespace MacaRush
{
    public sealed class HazardDamageZone : MonoBehaviour
    {
        [SerializeField] private float damagePerSecond = 8f;
        [SerializeField] private bool damageOnlyWhenStretcherTouches = true;

        private void OnTriggerStay(Collider other)
        {
            var patient = other.GetComponentInParent<PatientHealth>();
            if (patient != null)
            {
                patient.ApplyDamage(damagePerSecond * Time.deltaTime, PatientDamageSource.Hazard);
                return;
            }

            if (!damageOnlyWhenStretcherTouches) return;

            var stretcher = other.GetComponentInParent<MacaStretcher>();
            if (stretcher != null && stretcher.Patient != null)
            {
                stretcher.Patient.ApplyDamage(damagePerSecond * Time.deltaTime, PatientDamageSource.Hazard);
            }
        }

        public void Configure(float damagePerSecondValue)
        {
            damagePerSecond = damagePerSecondValue;
        }
    }
}

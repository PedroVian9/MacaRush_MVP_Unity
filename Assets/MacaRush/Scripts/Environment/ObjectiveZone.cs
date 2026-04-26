using UnityEngine;

namespace MacaRush
{
    [RequireComponent(typeof(Collider))]
    public sealed class ObjectiveZone : MonoBehaviour
    {
        [SerializeField] private string objectiveText = "Continue levando a maca.";
        [SerializeField] private bool triggerOnce = true;

        private bool used;

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (used && triggerOnce) return;
            if (other.GetComponentInParent<MacaStretcher>() == null) return;

            used = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetObjective(objectiveText);
            }
        }

        public void Configure(string text)
        {
            objectiveText = text;
        }
    }
}

using UnityEngine;

namespace MacaRush
{
    public sealed class FinishZone : MonoBehaviour
    {
        [SerializeField] private string macaTag = "Maca";
        [SerializeField] private string objectiveOnEnter = "Paciente entregue.";

        private void OnTriggerEnter(Collider other)
        {
            if (!IsStretcher(other)) return;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetObjective(objectiveOnEnter);
                GameManager.Instance.Victory();
            }
        }

        private bool IsStretcher(Collider other)
        {
            if (other.GetComponentInParent<MacaStretcher>() != null) return true;
            if (other == null || string.IsNullOrWhiteSpace(macaTag)) return false;

            try
            {
                return string.Equals(other.gameObject.tag, macaTag, System.StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }
    }
}

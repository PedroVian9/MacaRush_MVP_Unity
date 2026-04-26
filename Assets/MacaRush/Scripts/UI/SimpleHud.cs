using UnityEngine;
using UnityEngine.UI;

namespace MacaRush
{
    public sealed class SimpleHud : MonoBehaviour
    {
        [SerializeField] private PatientHealth patient;
        [SerializeField] private Text statusText;
        [SerializeField] private Text alertText;
        [SerializeField] private Image healthFill;
        [SerializeField] private Image sirenOverlay;
        [SerializeField] private Color normalTextColor = Color.white;
        [SerializeField] private Color criticalTextColor = new Color(1f, 0.18f, 0.12f);

        private float alertPulse;

        public Image SirenOverlay => sirenOverlay;

        private void Update()
        {
            if (patient == null || statusText == null) return;

            var manager = GameManager.Instance;
            var gameState = manager != null ? manager.State : GameState.Playing;
            var time = manager != null ? manager.ElapsedTime : 0f;
            var remaining = manager != null ? manager.RemainingTime : 0f;
            var objective = manager != null ? manager.ObjectiveText : "Leve a maca ate a ambulancia.";

            statusText.color = IsPatientCritical() ? criticalTextColor : normalTextColor;
            statusText.text =
                $"Paciente: {patient.StateLabel} ({patient.Health:0}/{patient.MaxHealth:0})\n" +
                $"Vida: {patient.Health01 * 100f:0}%\n" +
                $"Tempo: {FormatTime(time)} / resta {FormatTime(remaining)}\n" +
                $"Objetivo: {objective}";

            if (healthFill != null)
            {
                healthFill.fillAmount = patient.Health01;
                healthFill.color = patient.StateColor;
            }

            UpdateAlert(gameState, manager);
        }

        public void Configure(PatientHealth patientHealth, Text mainText, Text alerts, Image fill, Image overlay)
        {
            patient = patientHealth;
            statusText = mainText;
            alertText = alerts;
            healthFill = fill;
            sirenOverlay = overlay;
        }

        private void UpdateAlert(GameState gameState, GameManager manager)
        {
            if (alertText == null) return;

            if (gameState == GameState.Victory)
            {
                alertText.color = Color.green;
                alertText.text = "VITORIA\nPaciente entregue vivo.\nR para reiniciar";
                return;
            }

            if (gameState == GameState.Defeat)
            {
                alertText.color = Color.red;
                var reason = manager != null ? manager.EndReason : "Derrota.";
                alertText.text = $"DERROTA\n{reason}\nR para reiniciar";
                return;
            }

            if (IsPatientCritical())
            {
                alertPulse += Time.deltaTime * 8f;
                var alpha = Mathf.Lerp(0.45f, 1f, (Mathf.Sin(alertPulse) + 1f) * 0.5f);
                alertText.color = new Color(1f, 0.08f, 0.05f, alpha);
                alertText.text = "PACIENTE CRITICO";
                return;
            }

            alertPulse = 0f;
            alertText.text = string.Empty;
        }

        private bool IsPatientCritical()
        {
            return patient != null && (patient.State == PatientState.Critical || patient.State == PatientState.Dying);
        }

        private static string FormatTime(float seconds)
        {
            seconds = Mathf.Max(0f, seconds);
            var minutes = Mathf.FloorToInt(seconds / 60f);
            var wholeSeconds = Mathf.FloorToInt(seconds % 60f);
            return $"{minutes:00}:{wholeSeconds:00}";
        }
    }
}

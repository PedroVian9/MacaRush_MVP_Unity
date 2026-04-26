using UnityEngine;

namespace MacaRush
{
    public enum PatientState
    {
        Stable,
        Injured,
        Critical,
        Dying,
        Dead
    }

    public enum PatientDamageSource
    {
        Time,
        Impact,
        Tilt,
        Fall,
        Event,
        Hazard
    }

    public sealed class PatientHealth : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float passiveDrainPerSecond = 1.2f;
        [SerializeField] private float passiveDrainDifficultyInfluence = 0.85f;

        [Header("State Thresholds")]
        [SerializeField] private float injuredThreshold = 76f;
        [SerializeField] private float criticalThreshold = 45f;
        [SerializeField] private float dyingThreshold = 18f;

        [Header("Feedback")]
        [SerializeField] private Renderer patientRenderer;
        [SerializeField] private Color stableColor = new Color(0.3f, 0.95f, 0.45f);
        [SerializeField] private Color injuredColor = new Color(1f, 0.84f, 0.25f);
        [SerializeField] private Color criticalColor = new Color(1f, 0.45f, 0.16f);
        [SerializeField] private Color dyingColor = new Color(1f, 0.08f, 0.08f);
        [SerializeField] private Color deadColor = new Color(0.18f, 0.18f, 0.18f);

        private float health;
        private PatientState lastState;
        private Material runtimeMaterial;

        public float Health => health;
        public float MaxHealth => maxHealth;
        public float Health01 => maxHealth <= 0f ? 0f : health / maxHealth;
        public bool IsDead => health <= 0f;
        public PatientState State
        {
            get
            {
                if (health <= 0f) return PatientState.Dead;
                if (health <= dyingThreshold) return PatientState.Dying;
                if (health <= criticalThreshold) return PatientState.Critical;
                if (health <= injuredThreshold) return PatientState.Injured;
                return PatientState.Stable;
            }
        }

        public string StateLabel
        {
            get
            {
                switch (State)
                {
                    case PatientState.Stable:
                        return "Estavel";
                    case PatientState.Injured:
                        return "Machucado";
                    case PatientState.Critical:
                        return "Critico";
                    case PatientState.Dying:
                        return "Morrendo";
                    case PatientState.Dead:
                        return "Morto";
                    default:
                        return State.ToString();
                }
            }
        }

        public Color StateColor
        {
            get
            {
                switch (State)
                {
                    case PatientState.Stable:
                        return stableColor;
                    case PatientState.Injured:
                        return injuredColor;
                    case PatientState.Critical:
                        return criticalColor;
                    case PatientState.Dying:
                        return dyingColor;
                    case PatientState.Dead:
                        return deadColor;
                    default:
                        return Color.white;
                }
            }
        }

        private void Awake()
        {
            health = Mathf.Max(1f, maxHealth);
            lastState = State;

            if (patientRenderer == null)
            {
                patientRenderer = GetComponentInChildren<Renderer>();
            }

            if (patientRenderer != null)
            {
                runtimeMaterial = patientRenderer.material;
            }

            UpdateVisuals(true);
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

            var difficulty = GameManager.Instance != null ? GameManager.Instance.DifficultyMultiplier : 1f;
            var passiveDrain = passiveDrainPerSecond * Mathf.Lerp(1f, difficulty, passiveDrainDifficultyInfluence);
            ApplyDamage(passiveDrain * Time.deltaTime, PatientDamageSource.Time);
            UpdateVisuals(false);
        }

        public void ApplyDamage(float amount)
        {
            ApplyDamage(amount, PatientDamageSource.Hazard);
        }

        public void ApplyDamage(float amount, PatientDamageSource source)
        {
            if (amount <= 0f || IsDead) return;

            health = Mathf.Max(0f, health - amount);
            UpdateVisuals(false);

            if (IsDead && GameManager.Instance != null)
            {
                GameManager.Instance.Defeat("Paciente morreu.");
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || IsDead) return;

            health = Mathf.Min(maxHealth, health + amount);
            UpdateVisuals(false);
        }

        public void ResetHealth()
        {
            health = Mathf.Max(1f, maxHealth);
            UpdateVisuals(true);
        }

        public void Configure(Renderer rendererOverride)
        {
            patientRenderer = rendererOverride;

            if (patientRenderer != null)
            {
                runtimeMaterial = patientRenderer.material;
            }

            UpdateVisuals(true);
        }

        private void UpdateVisuals(bool force)
        {
            var state = State;
            if (!force && state == lastState) return;

            lastState = state;

            if (runtimeMaterial != null)
            {
                runtimeMaterial.color = StateColor;
            }
        }
    }
}

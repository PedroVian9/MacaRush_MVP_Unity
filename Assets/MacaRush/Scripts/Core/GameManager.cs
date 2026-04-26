using UnityEngine;
using UnityEngine.SceneManagement;

namespace MacaRush
{
    public enum GameState
    {
        Playing,
        Victory,
        Defeat
    }

    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private PatientHealth patientHealth;
        [SerializeField] private MacaStretcher stretcher;

        [Header("Rules")]
        [SerializeField] private float maxMatchTime = 240f;
        [SerializeField] private string objectiveText = "Leve a maca ate a ambulancia.";
        [SerializeField] private AnimationCurve difficultyByProgress = AnimationCurve.EaseInOut(0f, 1f, 1f, 1.75f);

        [Header("Runtime")]
        [SerializeField] private GameState state = GameState.Playing;
        [SerializeField] private float elapsedTime;
        [SerializeField] private string endReason;

        public GameState State => state;
        public float ElapsedTime => elapsedTime;
        public float MaxMatchTime => maxMatchTime;
        public float RemainingTime => Mathf.Max(0f, maxMatchTime - elapsedTime);
        public string ObjectiveText => objectiveText;
        public string EndReason => endReason;
        public bool IsPlaying => state == GameState.Playing;
        public MacaStretcher Stretcher => stretcher;
        public float MatchProgress01 => maxMatchTime <= 0f ? 0f : Mathf.Clamp01(elapsedTime / maxMatchTime);
        public float DifficultyMultiplier => difficultyByProgress != null
            ? Mathf.Max(0.2f, difficultyByProgress.Evaluate(MatchProgress01))
            : 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            if (state != GameState.Playing)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    RestartScene();
                }

                return;
            }

            elapsedTime += Time.deltaTime;

            if (patientHealth != null && patientHealth.IsDead)
            {
                Defeat("Paciente morreu.");
                return;
            }

            if (maxMatchTime > 0f && elapsedTime >= maxMatchTime)
            {
                Defeat("Tempo maximo acabou.");
            }
        }

        public void Configure(PatientHealth patient, MacaStretcher stretcherReference)
        {
            patientHealth = patient;
            stretcher = stretcherReference;
        }

        public void SetObjective(string objective)
        {
            if (string.IsNullOrWhiteSpace(objective)) return;
            objectiveText = objective;
        }

        public void Victory()
        {
            if (state != GameState.Playing) return;

            if (patientHealth != null && patientHealth.IsDead)
            {
                Defeat("Paciente chegou morto.");
                return;
            }

            state = GameState.Victory;
            endReason = "Paciente entregue vivo.";
            Debug.Log($"VITORIA! Paciente entregue em {elapsedTime:0.0}s.");
        }

        public void Defeat(string reason)
        {
            if (state != GameState.Playing) return;

            state = GameState.Defeat;
            endReason = string.IsNullOrWhiteSpace(reason) ? "Derrota." : reason;
            Debug.Log($"DERROTA! {endReason}");
        }

        public void RestartScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}

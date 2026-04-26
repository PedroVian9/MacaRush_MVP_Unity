using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MacaRush
{
    public sealed class RandomEventDirector : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private bool eventsEnabled = true;
        [SerializeField] private float firstEventDelay = 10f;
        [SerializeField] private float minDelay = 9f;
        [SerializeField] private float maxDelay = 18f;
        [SerializeField] private float minDelayAtMaxDifficulty = 4.5f;
        [SerializeField] private float maxDelayAtMaxDifficulty = 8.5f;

        [Header("Event Toggles")]
        [SerializeField] private bool flickerLights = true;
        [SerializeField] private bool lockDoor = true;
        [SerializeField] private bool patientMoves = true;
        [SerializeField] private bool crossingObstacle = true;
        [SerializeField] private bool slipperyFloor = true;
        [SerializeField] private bool sirenConfusion = true;

        [Header("References")]
        [SerializeField] private MacaStretcher stretcher;
        [SerializeField] private AutoDoor[] doors;
        [SerializeField] private MovingObstacle[] movingObstacles;
        [SerializeField] private SlipperyZone[] slipperyZones;
        [SerializeField] private Light[] eventLights;
        [SerializeField] private AudioSource sirenSource;
        [SerializeField] private Image sirenOverlay;

        [Header("Event Values")]
        [SerializeField] private float eventDuration = 4f;
        [SerializeField] private float patientJostleImpulse = 8f;
        [SerializeField] private float patientJostleDamage = 4f;
        [SerializeField] private float doorLockDuration = 3f;
        [SerializeField] private float eventIntensityInfluence = 0.85f;

        private float[] originalLightIntensities;

        private void Start()
        {
            AutoFillReferences();
            CacheLights();

            if (eventsEnabled)
            {
                StartCoroutine(EventLoop());
            }
        }

        public void Configure(
            MacaStretcher stretcherReference,
            Image overlay,
            AutoDoor[] doorReferences,
            MovingObstacle[] obstacleReferences,
            SlipperyZone[] slipperyReferences,
            Light[] lights)
        {
            stretcher = stretcherReference;
            sirenOverlay = overlay;
            doors = doorReferences;
            movingObstacles = obstacleReferences;
            slipperyZones = slipperyReferences;
            eventLights = lights;
            CacheLights();
        }

        private IEnumerator EventLoop()
        {
            yield return new WaitForSeconds(firstEventDelay);

            while (true)
            {
                if (GameManager.Instance == null || GameManager.Instance.State == GameState.Playing)
                {
                    TriggerRandomEvent();
                }

                var delayRange = GetScaledDelayRange();
                yield return new WaitForSeconds(Random.Range(delayRange.x, delayRange.y));
            }
        }

        private void TriggerRandomEvent()
        {
            for (var tries = 0; tries < 8; tries++)
            {
                var eventId = Random.Range(0, 6);
                if (TryRunEvent(eventId)) return;
            }
        }

        private bool TryRunEvent(int eventId)
        {
            switch (eventId)
            {
                case 0:
                    if (!flickerLights || eventLights == null || eventLights.Length == 0) return false;
                    StartCoroutine(FlickerLightsRoutine(GetScaledValue(eventDuration)));
                    SetObjective("Luzes piscando. Mantenham a maca no centro.");
                    return true;
                case 1:
                    if (!lockDoor || doors == null || doors.Length == 0) return false;
                    doors[Random.Range(0, doors.Length)].LockFor(GetScaledValue(doorLockDuration));
                    SetObjective("Porta travou. Segurem a maca.");
                    return true;
                case 2:
                    if (!patientMoves || stretcher == null) return false;
                    stretcher.ApplyPatientMovementImpulse(
                        GetScaledValue(patientJostleImpulse),
                        GetScaledValue(patientJostleDamage));
                    SetObjective("Paciente se mexeu na maca.");
                    return true;
                case 3:
                    if (!crossingObstacle || movingObstacles == null || movingObstacles.Length == 0) return false;
                    movingObstacles[Random.Range(0, movingObstacles.Length)].TriggerBurst(GetScaledValue(eventDuration));
                    SetObjective("Obstaculo atravessando.");
                    return true;
                case 4:
                    if (!slipperyFloor || slipperyZones == null || slipperyZones.Length == 0) return false;
                    slipperyZones[Random.Range(0, slipperyZones.Length)].ActivateFor(GetScaledValue(eventDuration + 2f));
                    SetObjective("Chao escorregadio temporario.");
                    return true;
                case 5:
                    if (!sirenConfusion) return false;
                    StartCoroutine(SirenRoutine(GetScaledValue(eventDuration)));
                    SetObjective("Sirene confundindo a equipe.");
                    return true;
                default:
                    return false;
            }
        }

        private IEnumerator FlickerLightsRoutine(float duration)
        {
            var timer = 0f;
            while (timer < duration)
            {
                timer += 0.12f;
                for (var i = 0; i < eventLights.Length; i++)
                {
                    if (eventLights[i] == null) continue;

                    var baseIntensity = GetOriginalIntensity(i);
                    eventLights[i].intensity = Random.value > 0.5f ? baseIntensity : baseIntensity * 0.18f;
                }

                yield return new WaitForSeconds(0.12f);
            }

            RestoreLights();
        }

        private IEnumerator SirenRoutine(float duration)
        {
            if (sirenSource != null && sirenSource.clip != null)
            {
                sirenSource.Play();
            }

            var timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;

                if (sirenOverlay != null)
                {
                    var alpha = Mathf.Lerp(0.05f, 0.28f, (Mathf.Sin(Time.time * 10f) + 1f) * 0.5f);
                    sirenOverlay.color = new Color(1f, 0f, 0f, alpha);
                }

                yield return null;
            }

            if (sirenOverlay != null)
            {
                sirenOverlay.color = Color.clear;
            }
        }

        private void AutoFillReferences()
        {
            if (stretcher == null)
            {
                var stretchers = FindObjectsByType<MacaStretcher>(FindObjectsSortMode.None);
                if (stretchers.Length > 0)
                {
                    stretcher = stretchers[0];
                }
            }

            if (doors == null || doors.Length == 0)
            {
                doors = FindObjectsByType<AutoDoor>(FindObjectsSortMode.None);
            }

            if (movingObstacles == null || movingObstacles.Length == 0)
            {
                movingObstacles = FindObjectsByType<MovingObstacle>(FindObjectsSortMode.None);
            }

            if (slipperyZones == null || slipperyZones.Length == 0)
            {
                slipperyZones = FindObjectsByType<SlipperyZone>(FindObjectsSortMode.None);
            }

            if (eventLights == null || eventLights.Length == 0)
            {
                eventLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            }
        }

        private void CacheLights()
        {
            if (eventLights == null)
            {
                originalLightIntensities = null;
                return;
            }

            originalLightIntensities = new float[eventLights.Length];
            for (var i = 0; i < eventLights.Length; i++)
            {
                originalLightIntensities[i] = eventLights[i] != null ? eventLights[i].intensity : 1f;
            }
        }

        private float GetOriginalIntensity(int index)
        {
            if (originalLightIntensities == null || index < 0 || index >= originalLightIntensities.Length)
            {
                return 1f;
            }

            return originalLightIntensities[index];
        }

        private void RestoreLights()
        {
            if (eventLights == null) return;

            for (var i = 0; i < eventLights.Length; i++)
            {
                if (eventLights[i] == null) continue;
                eventLights[i].intensity = GetOriginalIntensity(i);
            }
        }

        private static void SetObjective(string objective)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetObjective(objective);
            }
        }

        private Vector2 GetScaledDelayRange()
        {
            var t = GetDifficultyProgress();
            var scaledMin = Mathf.Lerp(minDelay, minDelayAtMaxDifficulty, t);
            var scaledMax = Mathf.Lerp(maxDelay, maxDelayAtMaxDifficulty, t);
            return new Vector2(Mathf.Max(0.5f, scaledMin), Mathf.Max(scaledMin + 0.1f, scaledMax));
        }

        private float GetScaledValue(float baseValue)
        {
            var difficulty = GameManager.Instance != null ? GameManager.Instance.DifficultyMultiplier : 1f;
            return baseValue * Mathf.Lerp(1f, difficulty, eventIntensityInfluence);
        }

        private static float GetDifficultyProgress()
        {
            if (GameManager.Instance == null) return 0f;
            return Mathf.Clamp01(GameManager.Instance.MatchProgress01);
        }
    }
}

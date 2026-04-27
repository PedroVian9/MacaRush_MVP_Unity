using UnityEngine;

namespace MacaRush
{
    public sealed class TemporaryWorldFx : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.45f;
        [SerializeField] private Vector3 drift = Vector3.up;

        private Renderer cachedRenderer;
        private Color startColor;
        private float age;

        private void Awake()
        {
            cachedRenderer = GetComponent<Renderer>();
            if (cachedRenderer != null)
            {
                startColor = cachedRenderer.material.color;
            }
        }

        private void Update()
        {
            age += Time.deltaTime;
            transform.position += drift * Time.deltaTime;

            var t = lifetime <= 0f ? 1f : Mathf.Clamp01(age / lifetime);
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, t * Time.deltaTime * 8f);

            if (cachedRenderer != null)
            {
                var color = startColor;
                color.a = Mathf.Lerp(startColor.a, 0f, t);
                cachedRenderer.material.color = color;
            }

            if (age >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        public void Configure(float duration, Vector3 driftDirection)
        {
            lifetime = duration;
            drift = driftDirection;
        }
    }
}

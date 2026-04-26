using UnityEngine;

namespace MacaRush
{
    public sealed class AutoDoor : MonoBehaviour
    {
        [SerializeField] private Transform door;
        [SerializeField] private Vector3 closedLocalPosition;
        [SerializeField] private Vector3 openLocalOffset = new Vector3(0f, 3f, 0f);
        [SerializeField] private float speed = 3f;
        [SerializeField] private float jamChance = 0.16f;
        [SerializeField] private float minJamDuration = 1.2f;
        [SerializeField] private float maxJamDuration = 2.8f;

        private int objectsInside;
        private float lockedTimer;
        private bool initialized;

        public bool IsLocked => lockedTimer > 0f;

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            Initialize();

            lockedTimer = Mathf.Max(0f, lockedTimer - Time.deltaTime);

            var shouldOpen = objectsInside > 0 && !IsLocked;
            var target = shouldOpen ? closedLocalPosition + openLocalOffset : closedLocalPosition;
            door.localPosition = Vector3.Lerp(door.localPosition, target, Time.deltaTime * speed);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsRelevant(other)) return;

            objectsInside++;
            if (Random.value <= jamChance)
            {
                LockFor(Random.Range(minJamDuration, maxJamDuration));
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsRelevant(other)) return;
            objectsInside = Mathf.Max(0, objectsInside - 1);
        }

        public void LockFor(float duration)
        {
            lockedTimer = Mathf.Max(lockedTimer, duration);
        }

        public void Configure(Transform doorTransform, Vector3 openOffset, float openSpeed, float randomJamChance)
        {
            door = doorTransform;
            openLocalOffset = openOffset;
            speed = openSpeed;
            jamChance = randomJamChance;
            initialized = false;
            Initialize();
        }

        private void Initialize()
        {
            if (initialized) return;

            if (door == null)
            {
                door = transform;
            }

            closedLocalPosition = door.localPosition;
            initialized = true;
        }

        private static bool IsRelevant(Collider other)
        {
            if (other.GetComponentInParent<PlayerCarryController>() != null) return true;
            if (other.GetComponentInParent<MacaStretcher>() != null) return true;
            return SafeCompareTag(other, "Player") || SafeCompareTag(other, "Maca");
        }

        private static bool SafeCompareTag(Component component, string tagName)
        {
            try
            {
                return component.CompareTag(tagName);
            }
            catch (UnityException)
            {
                return false;
            }
        }
    }
}

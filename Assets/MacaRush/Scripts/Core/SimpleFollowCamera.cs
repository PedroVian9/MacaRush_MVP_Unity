using UnityEngine;

namespace MacaRush
{
    public sealed class SimpleFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 12f, -10f);
        [SerializeField] private float followSpeed = 4f;
        [SerializeField] private float lookAhead = 2.5f;

        private Rigidbody targetBody;

        private void LateUpdate()
        {
            if (target == null) return;

            var velocity = targetBody != null ? targetBody.velocity : Vector3.zero;
            var lookTarget = target.position + new Vector3(velocity.x, 0f, velocity.z).normalized * lookAhead;
            var desiredPosition = lookTarget + offset;

            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followSpeed);
            transform.rotation = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);
        }

        public void Configure(Transform followTarget)
        {
            target = followTarget;
            targetBody = target != null ? target.GetComponent<Rigidbody>() : null;
        }
    }
}

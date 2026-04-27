using UnityEngine;

namespace MacaRush
{
    public sealed class SimpleFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 pivotOffset = new Vector3(0f, 1.8f, 0f);
        [SerializeField] private float distance = 5.2f;
        [SerializeField] private float minPitch = -25f;
        [SerializeField] private float maxPitch = 55f;
        [SerializeField] private float mouseSensitivity = 2.2f;
        [SerializeField] private float positionLerp = 12f;

        private float yaw;
        private float pitch = 18f;

        private void LateUpdate()
        {
            if (target == null) return;

            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            var pivot = target.position + pivotOffset;
            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            var desiredPosition = pivot + rotation * new Vector3(0f, 0f, -distance);

            transform.position = Vector3.Lerp(transform.position, desiredPosition, positionLerp * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(pivot - transform.position, Vector3.up);
        }

        public void Configure(Transform followTarget)
        {
            target = followTarget;

            if (target != null)
            {
                yaw = target.eulerAngles.y;
            }
        }
    }
}

using UnityEngine;

namespace MacaRush
{
    public sealed class PlayerVisualAnimator : MonoBehaviour
    {
        [SerializeField] private ThirdPersonPusherController controller;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform rightArm;
        [SerializeField] private Transform leftLeg;
        [SerializeField] private Transform rightLeg;
        [SerializeField] private float leanAngle = 10f;
        [SerializeField] private float pushLeanAngle = 18f;
        [SerializeField] private float limbSwing = 22f;
        [SerializeField] private float animationSpeed = 9f;

        private void Awake()
        {
            if (controller == null)
            {
                controller = GetComponentInParent<ThirdPersonPusherController>();
            }
        }

        private void Update()
        {
            if (controller == null || visualRoot == null) return;

            var move = controller.MoveAmount;
            var pushing = controller.IsPushing;
            var targetLean = pushing ? pushLeanAngle : leanAngle * move;
            visualRoot.localRotation = Quaternion.Lerp(
                visualRoot.localRotation,
                Quaternion.Euler(targetLean, 0f, 0f),
                Time.deltaTime * animationSpeed);

            var swing = Mathf.Sin(Time.time * animationSpeed) * limbSwing * move;
            var armPushOffset = pushing ? -32f : 0f;
            SetLocalX(leftArm, -swing + armPushOffset);
            SetLocalX(rightArm, swing + armPushOffset);
            SetLocalX(leftLeg, swing);
            SetLocalX(rightLeg, -swing);
        }

        public void Configure(
            ThirdPersonPusherController pusher,
            Transform root,
            Transform armLeft,
            Transform armRight,
            Transform legLeft,
            Transform legRight)
        {
            controller = pusher;
            visualRoot = root;
            leftArm = armLeft;
            rightArm = armRight;
            leftLeg = legLeft;
            rightLeg = legRight;
        }

        private static void SetLocalX(Transform target, float angle)
        {
            if (target == null) return;
            target.localRotation = Quaternion.Lerp(target.localRotation, Quaternion.Euler(angle, 0f, 0f), Time.deltaTime * 12f);
        }
    }
}

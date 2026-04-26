using UnityEngine;

namespace MacaRush
{
    public sealed class MacaHandle : MonoBehaviour
    {
        [Header("Handle")]
        [SerializeField] private int allowedPlayerIndex = 1;
        [SerializeField] private float grabDistance = 1.5f;
        [SerializeField] private Rigidbody macaRigidbody;
        [SerializeField] private Transform attachPoint;

        [Header("Joint")]
        [SerializeField] private float handleSlack = 0.28f;
        [SerializeField] private float spring = 760f;
        [SerializeField] private float damper = 72f;
        [SerializeField] private float maxForce = 2300f;
        [SerializeField] private float breakForce = 5200f;

        private ConfigurableJoint joint;
        private MacaHandleJointBreakRelay jointBreakRelay;

        public PlayerCarryController Holder { get; private set; }
        public int AllowedPlayerIndex => allowedPlayerIndex;
        public Rigidbody MacaRigidbody => macaRigidbody;

        private void Reset()
        {
            macaRigidbody = GetComponentInParent<Rigidbody>();
            attachPoint = transform;
        }

        private void OnDisable()
        {
            if (Holder != null)
            {
                Release(Holder);
            }
        }

        public bool CanBeHeldBy(PlayerCarryController player)
        {
            if (player == null) return false;
            if (Holder != null) return false;
            if (player.PlayerIndex != allowedPlayerIndex) return false;

            return Vector3.Distance(player.transform.position, transform.position) <= grabDistance;
        }

        public void Grab(PlayerCarryController player)
        {
            if (!CanBeHeldBy(player)) return;
            if (macaRigidbody == null) return;

            Holder = player;

            player.transform.position = attachPoint != null ? attachPoint.position : transform.position;

            joint = player.gameObject.AddComponent<ConfigurableJoint>();
            joint.connectedBody = macaRigidbody;
            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = Vector3.zero;
            joint.connectedAnchor = macaRigidbody.transform.InverseTransformPoint(transform.position);
            joint.enableCollision = false;
            joint.enablePreprocessing = true;
            joint.breakForce = breakForce;
            joint.breakTorque = breakForce;

            joint.xMotion = ConfigurableJointMotion.Limited;
            joint.yMotion = ConfigurableJointMotion.Limited;
            joint.zMotion = ConfigurableJointMotion.Limited;

            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            joint.linearLimit = new SoftJointLimit
            {
                limit = handleSlack
            };

            var drive = new JointDrive
            {
                positionSpring = spring,
                positionDamper = damper,
                maximumForce = maxForce
            };

            joint.xDrive = drive;
            joint.yDrive = drive;
            joint.zDrive = drive;

            jointBreakRelay = player.gameObject.AddComponent<MacaHandleJointBreakRelay>();
            jointBreakRelay.Configure(this, player);

            Debug.Log($"Player {player.PlayerIndex} segurou a alca {allowedPlayerIndex}.");
        }

        public void Release(PlayerCarryController player)
        {
            if (Holder != player) return;

            if (joint != null)
            {
                Destroy(joint);
            }

            if (jointBreakRelay != null)
            {
                Destroy(jointBreakRelay);
            }

            joint = null;
            jointBreakRelay = null;
            Holder = null;
            Debug.Log($"Player {player.PlayerIndex} soltou a alca {allowedPlayerIndex}.");
        }

        internal void NotifyJointBroken(PlayerCarryController player)
        {
            if (Holder != player) return;

            Holder = null;
            joint = null;
            jointBreakRelay = null;
        }

        public void Configure(int playerIndex, Rigidbody stretcherBody, Transform handleAttachPoint)
        {
            allowedPlayerIndex = playerIndex;
            macaRigidbody = stretcherBody;
            attachPoint = handleAttachPoint != null ? handleAttachPoint : transform;
        }
    }

    internal sealed class MacaHandleJointBreakRelay : MonoBehaviour
    {
        private MacaHandle handle;
        private PlayerCarryController player;

        public void Configure(MacaHandle owner, PlayerCarryController holder)
        {
            handle = owner;
            player = holder;
        }

        private void OnJointBreak(float breakForce)
        {
            if (handle != null)
            {
                handle.NotifyJointBroken(player);
            }

            Destroy(this);
        }
    }
}

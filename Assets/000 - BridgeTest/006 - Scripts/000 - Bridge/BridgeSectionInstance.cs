using UnityEngine;

namespace Bridge
{
    public class BridgeSectionInstance : MonoBehaviour
    {
        public Vector3 OriginalScale
        {
            get => originalScale;
            set => originalScale = value;
        }
        public Quaternion OriginalRotation
        {
            get => originalRotation;
            set => originalRotation = value;
        }
        public Vector3 LocalBridgeForward 
        {
            get => localBridgeForward; 
            set => localBridgeForward = value;
        }
        public BridgeSectionType SectionType => sectionType;

        //  =========================

        [SerializeField] private Material previewMaterial;
        [SerializeField] private Material previewErrorMaterial;
        [SerializeField] private Material realMaterial;
        [SerializeField] private Transform forwardMarker;

        [SerializeField] private Renderer renderers;
        [SerializeField] private Collider colliders;

        [Header("DEBUGGER")]
        [SerializeField] private BridgeSectionType sectionType;
        [SerializeField] private Vector3 originalScale;
        [SerializeField] private Quaternion originalRotation;
        [SerializeField] protected Vector3 localBridgeForward;

        //  =========================

        private void Awake()
        {
            OriginalScale = transform.localScale;
            OriginalRotation = transform.localRotation;
            LocalBridgeForward = forwardMarker != null ? transform.InverseTransformDirection(forwardMarker.forward).normalized : Vector3.forward;
        }

        public Quaternion GetPlacementRotation(Vector3 bridgeDir)
        {
            return Quaternion.LookRotation(bridgeDir, Vector3.up)
                   * Quaternion.Inverse(Quaternion.LookRotation(LocalBridgeForward, Vector3.up));
        }

        public void Initialize(BridgeSectionType type)
        {
            sectionType = type;
        }

        public void SetMode(BridgeSectionMode mode)
        {
            Material mat = mode switch
            {
                BridgeSectionMode.Preview => previewMaterial,
                BridgeSectionMode.Error   => previewErrorMaterial,
                _                         => realMaterial
            };

            renderers.sharedMaterial = mat;

            colliders.enabled = mode == BridgeSectionMode.Real;
        }

        public void ResetState()
        {
            transform.localScale = OriginalScale;
            transform.localRotation = OriginalRotation;
            transform.position = Vector3.zero;
        }
    }
}

using System;
using UnityEngine;

namespace Bridge
{
    public class PlacementInputHandler : MonoBehaviour
    {
        [SerializeField] private Camera placementCamera;
        [SerializeField] private LayerMask groundLayerMask;
        [SerializeField] private LayerMask anchorLayerMask;

        [Header("DEBUGGER")]
        [SerializeField] private bool isActive;

        //  =====================================

        public event Action<Vector3> OnFirstClick;
        public event Action<Vector3> OnDragUpdate;
        public event Action<Vector3> OnDragRelease;
        public event Action<Vector3, int> OnAnchorDragStarted;

        //  =====================================

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                HandleMouseDown();
            else if (Input.GetMouseButton(0) && isActive)
                HandleMouseHeld();
            else if (Input.GetMouseButtonUp(0) && isActive)
                HandleMouseUp();
        }

        private void HandleMouseDown()
        {
            if (TryGetAnchorPoint(out Vector3 anchorPos, out int anchorIndex))
            {
                isActive = true;
                OnAnchorDragStarted?.Invoke(anchorPos, anchorIndex);
                return;
            }

            if (!TryGetGroundPoint(out Vector3 worldPos)) return;

            isActive = true;
            OnFirstClick?.Invoke(worldPos);
        }

        private void HandleMouseHeld()
        {
            if (!TryGetGroundPoint(out Vector3 worldPos)) return;
            OnDragUpdate?.Invoke(worldPos);
        }

        private void HandleMouseUp()
        {
            isActive = false;
            if (!TryGetGroundPoint(out Vector3 worldPos)) return;
            OnDragRelease?.Invoke(worldPos);
        }

        private bool TryGetGroundPoint(out Vector3 worldPos)
        {
            worldPos = Vector3.zero;
            if (placementCamera == null) return false;

            Ray ray = placementCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayerMask))
            {
                worldPos = hit.point;
                return true;
            }
            return false;
        }

        private bool TryGetAnchorPoint(out Vector3 worldPos, out int pointIndex)
        {
            worldPos = Vector3.zero;
            pointIndex = -1;
            if (placementCamera == null || anchorLayerMask == 0) return false;

            Ray ray = placementCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, anchorLayerMask)) return false;

            var section = hit.collider.GetComponentInParent<BridgeSectionInstance>();
            if (section == null) return false;

            if (section.SectionType == BridgeSectionType.Start)
                pointIndex = 0;
            else if (section.SectionType == BridgeSectionType.End)
                pointIndex = 1;
            else
                return false;

            worldPos = hit.point;
            return true;
        }
    }
}

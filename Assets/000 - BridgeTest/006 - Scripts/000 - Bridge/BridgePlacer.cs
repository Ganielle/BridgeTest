using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Bridge
{
    public class BridgePlacer : MonoBehaviour
    {
        public enum PlacementState 
        { 
            PlacingA, 
            WaitingForB, 
            PlacingB, 
            Built, 
            DraggingA, 
            DraggingB 
        }

        private event EventHandler PlacementStateChange;
        public event EventHandler OnPlacementStateChange
        {
            add
            {
                if (PlacementStateChange == null || !PlacementStateChange.GetInvocationList().Contains(value))
                    PlacementStateChange += value;
            }
            remove { PlacementStateChange -= value; }
        }
        public PlacementState CurrentStates
        {
            get => state;
            set
            {
                state = value;
                PlacementStateChange?.Invoke(this, EventArgs.Empty);
            }
        }

        //  ======================

        [SerializeField] private BridgeConfig config;
        [SerializeField] private BridgeSectionPool pool;
        [SerializeField] private BridgePreview preview;
        [SerializeField] private PlacementInputHandler input;
        [SerializeField] private BridgeAnchorMarker anchorPrefab;

        [Space]
        [SerializeField] private Button clearBridgeBtn;

        [Header("DEBUGGER")]
        [SerializeField] private PlacementState state;
        [SerializeField] private Vector3 pointA;
        [SerializeField] private Vector3 pointB;
        [SerializeField] private BridgeLayout currentLayout;
        [SerializeField] private BridgeAnchorMarker anchorA;
        [SerializeField] private BridgeAnchorMarker anchorB;

        //  =========================

        private readonly List<BridgeSectionInstance> activeSections = new List<BridgeSectionInstance>();

        //  =========================

        private void OnEnable()
        {
            input.OnFirstClick += OnFirstClick;
            input.OnDragUpdate += OnDragUpdate;
            input.OnDragRelease += OnDragRelease;
            input.OnAnchorDragStarted += OnAnchorDragStarted;
            PlacementStateChange += OnPlacement;

            ClearBridgeCheckerBtn();
        }

        private void OnDisable()
        {
            input.OnFirstClick -= OnFirstClick;
            input.OnDragUpdate -= OnDragUpdate;
            input.OnDragRelease -= OnDragRelease;
            input.OnAnchorDragStarted -= OnAnchorDragStarted;
            PlacementStateChange -= OnPlacement;
        }

        private void Start()
        {
            preview.Initialize(pool, config);
        }

        private void OnPlacement(object sender, EventArgs e)
        {
            ClearBridgeCheckerBtn();
        }

        private void OnFirstClick(Vector3 worldPos)
        {
            if (CurrentStates == PlacementState.PlacingA)
            {
                pointA = worldPos;
                preview.ShowStartPreview(pointA);
                CurrentStates = PlacementState.WaitingForB;
                return;
            }

            if (CurrentStates == PlacementState.WaitingForB)
            {
                pointB = worldPos;
                preview.UpdatePreview(BridgeBuilder.CalculateLayout(pointA, pointB, config));
                CurrentStates = PlacementState.PlacingB;
            }
        }

        private void OnDragUpdate(Vector3 worldPos)
        {
            if (CurrentStates == PlacementState.PlacingB)
            {
                pointB = worldPos;
                preview.UpdatePreview(BridgeBuilder.CalculateLayout(pointA, pointB, config));
            }
            else if (CurrentStates == PlacementState.DraggingA)
            {
                pointA = worldPos;
                preview.UpdatePreview(BridgeBuilder.CalculateLayout(pointA, pointB, config));
            }
            else if (CurrentStates == PlacementState.DraggingB)
            {
                pointB = worldPos;
                preview.UpdatePreview(BridgeBuilder.CalculateLayout(pointA, pointB, config));
            }
        }

        private void OnDragRelease(Vector3 worldPos)
        {
            if (CurrentStates == PlacementState.PlacingB)
            {
                pointB = worldPos;
                Build();
            }
            else if (CurrentStates == PlacementState.DraggingA || state == PlacementState.DraggingB)
            {
                if (CurrentStates == PlacementState.DraggingA) pointA = worldPos;
                else pointB = worldPos;
                Build();
            }
        }

        private void OnAnchorDragStarted(Vector3 worldPos, int pointIndex)
        {
            if (CurrentStates != PlacementState.Built) return;

            ReturnActiveSections();
            CurrentStates = pointIndex == 0 ? PlacementState.DraggingA : PlacementState.DraggingB;
            preview.UpdatePreview(BridgeBuilder.CalculateLayout(pointA, pointB, config));
        }

        private void ClearBridgeCheckerBtn() => clearBridgeBtn.interactable = CurrentStates == PlacementState.Built;

        private void Build()
        {
            currentLayout = BridgeBuilder.CalculateLayout(pointA, pointB, config);
            if (!currentLayout.isValid)
            {
                ClearBridge();
                return;
            }

            preview.TransferToReal(activeSections);
            PlaceAnchors();
            CurrentStates = PlacementState.Built;
        }

        public void ClearBridge()
        {
            ReturnActiveSections();
            preview.HidePreview();
            CurrentStates = PlacementState.PlacingA;
        }

        private void PlaceAnchors()
        {
            DestroyAnchors();
            if (anchorPrefab == null) return;

            anchorA = Instantiate(anchorPrefab, pointA, Quaternion.identity);
            anchorA.PointIndex = 0;

            anchorB = Instantiate(anchorPrefab, pointB, Quaternion.identity);
            anchorB.PointIndex = 1;
        }

        private void DestroyAnchors()
        {
            if (anchorA != null) { Destroy(anchorA.gameObject); anchorA = null; }
            if (anchorB != null) { Destroy(anchorB.gameObject); anchorB = null; }
        }

        private void ReturnActiveSections()
        {
            foreach (var s in activeSections)
                pool.Return(s);
            activeSections.Clear();
            DestroyAnchors();
        }
    }
}

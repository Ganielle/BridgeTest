using System.Collections.Generic;
using UnityEngine;

namespace Bridge
{
    public class BridgePreview : MonoBehaviour
    {
        private BridgeSectionPool pool;
        private BridgeConfig config;

        private BridgeSectionInstance previewStart;
        private BridgeSectionInstance previewEnd;
        private readonly List<BridgeSectionInstance> previewMiddles = new List<BridgeSectionInstance>();
        private readonly List<BridgeSectionInstance> previewFillers = new List<BridgeSectionInstance>();

        public void Initialize(BridgeSectionPool sectionPool, BridgeConfig bridgeConfig)
        {
            pool = sectionPool;
            config = bridgeConfig;
        }

        public void ShowErrorState(Vector3 pointA, Vector3 pointB)
        {
            ReturnList(previewMiddles);
            ReturnList(previewFillers);

            if (previewStart == null)
                previewStart = pool.Borrow(BridgeSectionType.Start);
            if (previewEnd == null)
                previewEnd = pool.Borrow(BridgeSectionType.End);

            previewStart.SetMode(BridgeSectionMode.Error);
            previewEnd.SetMode(BridgeSectionMode.Error);

            Vector3 dir = (pointB - pointA).sqrMagnitude > 0.0001f ? (pointB - pointA).normalized : Vector3.forward;

            float cursor = 0f;
            Place(previewStart, pointA, dir, config.startSection.length, ref cursor);
            previewEnd.transform.SetPositionAndRotation(pointB - dir * (config.endSection.length * 0.5f), previewEnd.GetPlacementRotation(dir));
            previewEnd.transform.localScale = previewEnd.OriginalScale;
        }

        public void ShowStartPreview(Vector3 pointA)
        {
            ReturnAndClear(ref previewEnd);
            ReturnList(previewMiddles);
            ReturnList(previewFillers);

            if (previewStart == null)
            {
                previewStart = pool.Borrow(BridgeSectionType.Start);
                previewStart.SetMode(BridgeSectionMode.Preview);
            }

            previewStart.transform.SetPositionAndRotation(pointA, previewStart.GetPlacementRotation(Vector3.forward));
            previewStart.transform.localScale = previewStart.OriginalScale;
        }

        public void UpdatePreview(BridgeLayout layout)
        {
            if (!layout.isValid)
            {
                ShowErrorState(layout.origin, layout.destination);
                return;
            }

            EnsureStartEnd();
            SyncCount(previewMiddles, layout.middleCount, BridgeSectionType.Middle);
            SyncCount(previewFillers, layout.fillersPerSide * 2, BridgeSectionType.Filler);
            PositionSections(layout);
        }

        public void TransferToReal(List<BridgeSectionInstance> output)
        {
            Claim(ref previewStart, output);
            Claim(ref previewEnd, output);

            foreach (var m in previewMiddles) { m.SetMode(BridgeSectionMode.Real); output.Add(m); }
            foreach (var f in previewFillers) { f.SetMode(BridgeSectionMode.Real); output.Add(f); }

            previewMiddles.Clear();
            previewFillers.Clear();
        }

        public void HidePreview()
        {
            ReturnAndClear(ref previewStart);
            ReturnAndClear(ref previewEnd);
            ReturnList(previewMiddles);
            ReturnList(previewFillers);
        }

        private void EnsureStartEnd()
        {
            if (previewStart == null)
                previewStart = pool.Borrow(BridgeSectionType.Start);
            previewStart.SetMode(BridgeSectionMode.Preview);

            if (previewEnd == null)
                previewEnd = pool.Borrow(BridgeSectionType.End);
            previewEnd.SetMode(BridgeSectionMode.Preview);
        }

        private void SyncCount(List<BridgeSectionInstance> list, int target, BridgeSectionType type)
        {
            target = Mathf.Max(0, target);
            while (list.Count > target)
            {
                pool.Return(list[list.Count - 1]);
                list.RemoveAt(list.Count - 1);
            }
            while (list.Count < target)
            {
                var instance = pool.Borrow(type);
                instance.SetMode(BridgeSectionMode.Preview);
                list.Add(instance);
            }
        }

        private void PositionSections(BridgeLayout layout)
        {
            Vector3 dir = (layout.destination - layout.origin).normalized;
            float cursor = 0f;

            Place(previewStart, layout.origin, dir, config.startSection.length, ref cursor);

            for (int a = 0; a < layout.fillersPerSide; a++)
                Place(previewFillers[a], layout.origin, dir, config.fillerSection.length, ref cursor);

            for (int a = 0; a < layout.middleCount; a++)
                Place(previewMiddles[a], layout.origin, dir, config.middleSection.length, ref cursor);

            for (int a = 0; a < layout.fillersPerSide; a++)
                Place(previewFillers[layout.fillersPerSide + a], layout.origin, dir, config.fillerSection.length, ref cursor);

            Place(previewEnd, layout.origin, dir, config.endSection.length, ref cursor);
        }

        private static void Place(BridgeSectionInstance instance, Vector3 origin, Vector3 dir, float length, ref float cursor)
        {
            instance.transform.SetPositionAndRotation(origin + dir * (cursor + length * 0.5f), instance.GetPlacementRotation(dir));
            instance.transform.localScale = instance.OriginalScale;
            cursor += length;
        }

        private void Claim(ref BridgeSectionInstance instance, List<BridgeSectionInstance> output)
        {
            if (instance == null) return;
            instance.SetMode(BridgeSectionMode.Real);
            output.Add(instance);
            instance = null;
        }

        private void ReturnAndClear(ref BridgeSectionInstance instance)
        {
            if (instance == null) return;
            pool.Return(instance);
            instance = null;
        }

        private void ReturnList(List<BridgeSectionInstance> list)
        {
            foreach (var s in list) pool.Return(s);
            list.Clear();
        }
    }
}

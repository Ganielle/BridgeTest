using UnityEngine;
using UnityEngine.Pool;

namespace Bridge
{
    public class BridgeSectionPool : MonoBehaviour
    {
        [SerializeField] private BridgeSectionData startData;
        [SerializeField] private BridgeSectionData endData;
        [SerializeField] private BridgeSectionData middleData;
        [SerializeField] private BridgeSectionData fillerData;
        [SerializeField] private int defaultCapacity = 10;
        [SerializeField] private int maxSize = 50;

        //  ================================

        private ObjectPool<BridgeSectionInstance> startPool;
        private ObjectPool<BridgeSectionInstance> endPool;
        private ObjectPool<BridgeSectionInstance> middlePool;
        private ObjectPool<BridgeSectionInstance> fillerPool;

        //  ================================

        private void Awake()
        {
            startPool  = CreatePool(startData,  BridgeSectionType.Start);
            endPool    = CreatePool(endData,    BridgeSectionType.End);
            middlePool = CreatePool(middleData, BridgeSectionType.Middle);
            fillerPool = CreatePool(fillerData, BridgeSectionType.Filler);
        }

        private ObjectPool<BridgeSectionInstance> CreatePool(BridgeSectionData data, BridgeSectionType type)
        {
            return new ObjectPool<BridgeSectionInstance>(
                createFunc:      () => CreateInstance(data, type),
                actionOnGet:     instance => instance.gameObject.SetActive(true),
                actionOnRelease: instance => { instance.ResetState(); instance.gameObject.SetActive(false); },
                actionOnDestroy: instance => Destroy(instance.gameObject),
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize:         maxSize
            );
        }

        private BridgeSectionInstance CreateInstance(BridgeSectionData data, BridgeSectionType type)
        {
            var go = Instantiate(data.prefab, transform);
            var instance = go.GetComponent<BridgeSectionInstance>();
            if (instance == null)
                instance = go.AddComponent<BridgeSectionInstance>();
            instance.Initialize(type);
            go.SetActive(false);
            return instance;
        }

        public BridgeSectionInstance Borrow(BridgeSectionType type)
        {
            return type switch
            {
                BridgeSectionType.Start  => startPool.Get(),
                BridgeSectionType.End    => endPool.Get(),
                BridgeSectionType.Middle => middlePool.Get(),
                BridgeSectionType.Filler => fillerPool.Get(),
                _                        => null
            };
        }

        public void Return(BridgeSectionInstance instance)
        {
            if (instance == null) return;

            switch (instance.SectionType)
            {
                case BridgeSectionType.Start:  startPool.Release(instance);  break;
                case BridgeSectionType.End:    endPool.Release(instance);    break;
                case BridgeSectionType.Middle: middlePool.Release(instance); break;
                case BridgeSectionType.Filler: fillerPool.Release(instance); break;
            }
        }

        private void OnDestroy()
        {
            startPool?.Dispose();
            endPool?.Dispose();
            middlePool?.Dispose();
            fillerPool?.Dispose();
        }
    }
}

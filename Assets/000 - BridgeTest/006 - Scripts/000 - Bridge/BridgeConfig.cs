using UnityEngine;

namespace Bridge
{
    [CreateAssetMenu(fileName = "BridgeConfig", menuName = "Bridge/Config")]
    public class BridgeConfig : ScriptableObject
    {
        public BridgeSectionData startSection;
        public BridgeSectionData endSection;
        public BridgeSectionData middleSection;
        public BridgeSectionData fillerSection;

    }
}

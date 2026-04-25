using UnityEngine;

namespace Bridge
{
    [CreateAssetMenu(fileName = "BridgeSectionData", menuName = "Bridge/Section Data")]
    public class BridgeSectionData : ScriptableObject
    {
        public BridgeSectionType type;
        public GameObject prefab;
        public float length;
    }
}

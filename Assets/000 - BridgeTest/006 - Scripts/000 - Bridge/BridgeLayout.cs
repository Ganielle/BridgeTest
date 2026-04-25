using UnityEngine;

namespace Bridge
{
    public struct BridgeLayout
    {
        public Vector3 origin;
        public Vector3 destination;
        public float totalLength;
        public int middleCount;
        public int fillersPerSide;
        public bool isValid;
    }
}

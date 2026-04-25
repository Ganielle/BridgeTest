using UnityEngine;

namespace Bridge
{
    public static class BridgeBuilder
    {
        private const float MinLength = 0.01f;
        private const float MinGap = 0.001f;

        public static BridgeLayout CalculateLayout(Vector3 a, Vector3 b, BridgeConfig config)
        {
            float totalLength = Vector3.Distance(a, b);

            if (totalLength < MinLength)
                return Invalid(a, b);

            if (!IsConfigValid(config))
                return Invalid(a, b);

            float innerSpan = totalLength - config.startSection.length - config.endSection.length;

            if (innerSpan < 0f)
                return Invalid(a, b);

            int middleCount = Mathf.FloorToInt(innerSpan / config.middleSection.length);
            float gapTotal = innerSpan - middleCount * config.middleSection.length;
            float gapPerSide = gapTotal * 0.5f;

            int fillersPerSide = gapPerSide >= MinGap
                ? Mathf.FloorToInt(gapPerSide / config.fillerSection.length)
                : 0;

            return new BridgeLayout
            {
                origin = a,
                destination = b,
                totalLength = totalLength,
                middleCount = middleCount,
                fillersPerSide = fillersPerSide,
                isValid = true
            };
        }

        private static bool IsConfigValid(BridgeConfig config) =>
            config != null
            && config.startSection != null && config.startSection.length  > 0f
            && config.endSection != null && config.endSection.length    > 0f
            && config.middleSection != null && config.middleSection.length > 0f
            && config.fillerSection != null && config.fillerSection.length > 0f;

        private static BridgeLayout Invalid(Vector3 a, Vector3 b) =>
            new BridgeLayout { origin = a, destination = b, isValid = false };
    }
}

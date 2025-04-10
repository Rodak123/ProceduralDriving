using UnityEngine;

namespace ProceduralRoad
{
    public class RoadSegment
    {
        private RoadConfigSO roadConfig;

        private Vector3 startPosition;
        private float startDirection;
        private float startHeight;

        private float size;
        private float directionOffset;
        private float heightOffset;

        private Vector3 position;
        private float direction;
        private float height;

        private float stepSize;
        private float step;
        private float T => step / size;

        public Vector3 Position => new(position.x, height, position.z);
        public float Direction => direction;

        public bool IsDone => step >= size;
        public bool IsFirstStep => step < stepSize;

        public RoadSegment(RoadConfigSO roadConfig, RoadSegmentSO type, float stepSize, Vector3 startPosition, float startDirection)
        {
            this.roadConfig = roadConfig;

            this.stepSize = stepSize;
            this.startPosition = startPosition;
            this.startDirection = startDirection;
            startHeight = startPosition.y;

            size = type.PickSize();
            directionOffset = type.PickDirectionOffset();
            heightOffset = type.PickHeightOffset();

            position = startPosition;
            direction = startDirection;
            height = startHeight;
        }

        public void Step()
        {
            step += stepSize;

            direction = Mathf.LerpAngle(startDirection, startDirection + directionOffset, T);
            height = Mathf.Lerp(startHeight, startHeight + heightOffset, roadConfig.HeightChangeCurve.Evaluate(T));

            position += stepSize * new Vector3(Mathf.Sin(Direction * Mathf.Deg2Rad), 0, Mathf.Cos(Direction * Mathf.Deg2Rad));
        }
    }
}
using UnityEngine;

namespace ProceduralRoad
{
    [CreateAssetMenu(fileName = "RoadSegment", menuName = "ProceduralRoad/RoadSegment", order = 0)]
    public class RoadSegmentSO : ScriptableObject
    {
        [Header("Size")]
        [SerializeField] private float minSize = 10;
        [SerializeField] private float maxSize = 100;
        [SerializeField] private AnimationCurve sizeChanceCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Header("Direction")]
        [SerializeField] private float minDirectionOffset;
        [SerializeField] private float maxDirectionOffset;
        [SerializeField] private AnimationCurve directionOffsetChanceCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private AnimationCurve directionSideChanceCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Header("Height")]
        [SerializeField] private float minHeightOffset;
        [SerializeField] private float maxHeightOffset;
        [SerializeField] private AnimationCurve heightOffsetChanceCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private AnimationCurve heightDirectionChanceCurve = AnimationCurve.Linear(0, 0, 1, 1);

        public float PickSize()
        {
            float size = Mathf.Lerp(minSize, maxSize, sizeChanceCurve.Evaluate(Random.value));
            return Mathf.Max(1, size);
        }

        public float PickDirectionOffset()
        {
            float side = directionSideChanceCurve.Evaluate(Random.value) < 0.5 ? -1 : 1;
            return Mathf.Lerp(minDirectionOffset, maxDirectionOffset, directionOffsetChanceCurve.Evaluate(Random.value)) * side;
        }

        public float PickHeightOffset()
        {
            float direction = heightDirectionChanceCurve.Evaluate(Random.value) < 0.5 ? -1 : 1;
            return Mathf.Lerp(minHeightOffset, maxHeightOffset, heightOffsetChanceCurve.Evaluate(Random.value)) * direction;
        }
    }
}
using System;
using System.Linq;
using UnityEngine;

namespace ProceduralRoad
{
    [CreateAssetMenu(fileName = "RoadConfig", menuName = "ProceduralRoad/RoadConfig", order = 0)]
    public class RoadConfigSO : ScriptableObject
    {
        [Serializable]
        private struct RoadSegmentChance
        {
            public RoadSegmentSO roadSegment;
            public int chance;
        }

        [SerializeField] private RoadSegmentChance[] roadSegmentsChances;
        [SerializeField] private AnimationCurve heightChangeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public RoadSegmentSO[] RoadSegmentTypes => roadSegmentsChances.Select((segmentChance) => segmentChance.roadSegment).ToArray();
        public AnimationCurve HeightChangeCurve => heightChangeCurve;

        public RoadSegmentSO PickRandomSegmentType()
        {
            if (roadSegmentsChances.Length == 0)
                throw new Exception($"{roadSegmentsChances} list is empty");

            int chanceSum = roadSegmentsChances.Sum((segmentChance) => segmentChance.chance);
            int value = UnityEngine.Random.Range(0, chanceSum);

            int chance = 0;
            for (int i = 0; i < roadSegmentsChances.Length; i++)
            {
                chance += roadSegmentsChances[i].chance;
                if (value <= chance)
                    return roadSegmentsChances[i].roadSegment;
            }

            return roadSegmentsChances.Last().roadSegment;
        }
    }
}
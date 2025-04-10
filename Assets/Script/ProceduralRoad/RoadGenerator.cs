using System;
using System.Collections.Generic;
using PathCreation;
using PathCreation.Examples;
using UnityEngine;

namespace ProceduralRoad
{

    public class RoadGenerator : MonoBehaviour
    {
        [Serializable]
        private struct AnimatedAttribute
        {
            [Header("Execution")]
            public AnimationCurve executionCurve;
            public AnimationCurve executionDurationCurve;

            [Header("Change")]
            public AnimationCurve changeCurve;
            public float minChange;
            public float maxChange;

            public readonly float CurrentT(float time, float duration) => executionCurve.Evaluate(time / duration);
            public readonly float NextDuration() => executionDurationCurve.Evaluate(UnityEngine.Random.value);
            public readonly float NextValue() => Mathf.Lerp(minChange, maxChange, changeCurve.Evaluate(UnityEngine.Random.value));
        }

        [Header("Path")]
        [SerializeField] private PathCreator pathCreator;
        [SerializeField] private RoadMeshCreator roadMeshCreator;
        [SerializeField] private float stepTiling = 10;

        [Header("Generation")]
        [SerializeField] private bool useSeed = false;
        [SerializeField] private int seed;
        [SerializeField] private float stepSize = 3;
        [SerializeField] private int maxPoints = 16;
        [SerializeField] private float attributeTimeStep = 0.1f;

        [Header("Attributes")]
        [SerializeField]
        private AnimatedAttribute directionAnimation = new()
        {
            executionCurve = AnimationCurve.Linear(0, 0, 1, 1),
            executionDurationCurve = AnimationCurve.Constant(0, 1, 1),

            changeCurve = AnimationCurve.Linear(0, 0, 1, 1),
            minChange = -60,
            maxChange = 60
        };

        [SerializeField]
        private AnimatedAttribute heightAnimation = new()
        {
            executionCurve = AnimationCurve.Linear(0, 0, 1, 1),
            executionDurationCurve = AnimationCurve.Constant(0, 1, 1),

            changeCurve = AnimationCurve.Linear(0, 0, 1, 1),
            minChange = -5,
            maxChange = 5
        };

        public float StepSize => stepSize;

        private readonly List<Vector3> points = new();

        private Vector3 lastPoint = Vector3.zero;

        private Vector3 direction;
        private Vector3 startDirection;
        private Vector3 endDirection;
        private float directionChangeDuration;
        private float directionChangeTime;

        private float height;
        private float startHeight;
        private float endHeight;
        private float heightChangeDuration;
        private float heightChangeTime;

        private void Awake()
        {
            if (useSeed)
            {
                UnityEngine.Random.InitState(seed);
            }

            if (pathCreator == null)
            {
                Debug.LogError($"{nameof(pathCreator)} is null");
            }

            if (roadMeshCreator == null)
            {
                Debug.LogError($"{nameof(roadMeshCreator)} is null");
            }

            direction = transform.forward;
            startDirection = direction;
            endDirection = NextDirection();

            directionChangeTime = 0;
            directionChangeDuration = directionAnimation.NextDuration();

            height = transform.position.y;
            startHeight = height;
            endHeight = NextHeight();

            heightChangeTime = 0;
            heightChangeDuration = heightAnimation.NextDuration();

            points.Add(Vector3.zero);
            for (int i = 0; i < maxPoints - 1; i++)
            {
                Vector3 point = GenerateNextPoint();
                points.Add(point);
            }

            roadMeshCreator.textureTiling = maxPoints * stepTiling;
            UpdatePath();
            UpdatePath();
        }

        public void GenerateSegment()
        {
            points.RemoveAt(0);
            Vector3 point = GenerateNextPoint();
            points.Add(point);

            UpdatePath();
        }

        private Vector3 NextDirection()
        {
            float angle = Vector3.Angle(direction, Vector3.forward);
            float newAngle = angle + directionAnimation.NextValue();
            Debug.Log($"{angle} -> {newAngle}");

            float newAngleRad = newAngle * Mathf.Deg2Rad;
            return new(Mathf.Cos(newAngleRad), 0, Mathf.Sin(newAngleRad));
        }

        private float NextHeight()
        {
            return height + heightAnimation.NextValue();
        }

        private Vector3 GenerateNextPoint()
        {
            directionChangeTime = Mathf.Min(directionChangeTime + attributeTimeStep, directionChangeDuration);
            direction = Vector3.Slerp(startDirection, endDirection, directionAnimation.CurrentT(directionChangeTime, directionChangeDuration));
            if (directionChangeTime >= directionChangeDuration)
            {
                directionChangeTime = 0;
                directionChangeDuration = directionAnimation.NextDuration();

                endDirection = NextDirection();
                startDirection = direction;
            }

            heightChangeTime = Mathf.Min(heightChangeTime + attributeTimeStep, heightChangeDuration);
            height = Mathf.Lerp(startHeight, endHeight, heightAnimation.CurrentT(heightChangeTime, heightChangeDuration));
            if (heightChangeTime >= heightChangeDuration)
            {
                heightChangeTime = 0;
                heightChangeDuration = heightAnimation.NextDuration();

                endHeight = NextHeight();
                startHeight = height;
            }

            Vector3 point = lastPoint + direction * stepSize;
            point.y = height;
            lastPoint = point;

            return point;
        }

        private void UpdatePath()
        {
            pathCreator.bezierPath = new BezierPath(points, false, PathSpace.xyz);
            roadMeshCreator.ForcePathUpdate();
        }
    }

}
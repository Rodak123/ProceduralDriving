using System.Collections.Generic;
using PathCreation;
using PathCreation.Examples;
using UnityEngine;

namespace ProceduralRoad
{
    public class RoadGenerator : MonoBehaviour
    {
        [Header("Path")]
        [SerializeField] private PathCreator pathCreator;
        [SerializeField] private RoadMeshCreator roadMeshCreator;
        [SerializeField] private float stepTiling = 10;

        [Header("Generation")]
        [SerializeField] private bool useSeed = false;
        [SerializeField] private int seed;
        [SerializeField] private float stepSize = 3;
        [SerializeField] private int maxPoints = 16;

        [Header("Road")]
        [SerializeField] private RoadConfigSO roadConfig;
        private RoadSegment currentSegment;

        public float StepSize => stepSize;

        private readonly List<Vector3> points = new();

        private void Awake()
        {
            if (useSeed)
                Random.InitState(seed);

            if (pathCreator == null)
            {
                Debug.LogError($"{nameof(pathCreator)} is null");
            }

            if (roadMeshCreator == null)
            {
                Debug.LogError($"{nameof(roadMeshCreator)} is null");
            }

            roadMeshCreator.textureTiling = maxPoints * stepTiling;

            currentSegment = new RoadSegment(roadConfig, roadConfig.PickRandomSegmentType(), stepSize, transform.position, transform.rotation.eulerAngles.y);
            points.Add(currentSegment.Position);

            for (int i = 0; i < maxPoints - 1; i++)
                GeneratePoint();
            UpdatePath();
            UpdatePath();
        }

        public void GeneratePoint()
        {
            if (points.Count >= maxPoints) points.RemoveAt(0);

            currentSegment.Step();
            points.Add(currentSegment.Position);

            if (currentSegment.IsDone)
                currentSegment = new RoadSegment(roadConfig, roadConfig.PickRandomSegmentType(), stepSize, currentSegment.Position, currentSegment.Direction);

            UpdatePath();
        }

        private void UpdatePath()
        {
            pathCreator.bezierPath = new BezierPath(points, false, PathSpace.xyz);
            roadMeshCreator.ForcePathUpdate();
        }
    }

}
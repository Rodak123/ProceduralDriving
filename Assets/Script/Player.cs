using PathCreation;
using ProceduralRoad;
using UnityEngine;

namespace ProceduralDriving
{
    [RequireComponent(typeof(CarController))]
    public class Player : MonoBehaviour
    {
        [Header("Path")]
        [SerializeField] private PathCreator pathCreator;
        [SerializeField] private RoadGenerator roadGenerator;

        private Rigidbody rb;
        private float checkpointDistance;

        private float distanceTraveled = 0;

        public float DistanceTraveled => distanceTraveled;

        private void Awake()
        {
            if (pathCreator == null)
            {
                Debug.LogError($"{nameof(pathCreator)} is null");
            }

            if (roadGenerator == null)
            {
                Debug.LogError($"{nameof(roadGenerator)} is null");
            }

            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            checkpointDistance = pathCreator.path.length / 2;
            Respawn();
        }

        private void Update()
        {
            distanceTraveled += new Vector2(rb.velocity.x, rb.velocity.z).magnitude * Time.deltaTime;

            float distance = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
            float targetDistance = pathCreator.path.length / 2;
            if (distance - targetDistance > roadGenerator.StepSize)
            {
                roadGenerator.GeneratePoint();
                checkpointDistance = distance;
            }

            if (Input.GetKeyDown(KeyCode.R)) Respawn();
        }

        public void Respawn()
        {
            float t = checkpointDistance / pathCreator.path.length;
            transform.position = pathCreator.path.GetPointAtTime(t) + new Vector3(0, 1, 0);
            transform.rotation = Quaternion.LookRotation(pathCreator.path.GetDirection(t));

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
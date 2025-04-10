using UnityEngine;

namespace ProceduralDriving
{
    public class CameraFollower : MonoBehaviour
    {
        [Header("Transforms")]
        [SerializeField] private Transform lookTarget;
        [SerializeField] private Transform positionTarget;

        [Header("Speeds")]
        [SerializeField] private float followSpeed = 10f;
        [SerializeField] private float rotationSpeed = 5;

        private void Update()
        {
            transform.position = Vector3.Lerp(transform.position, positionTarget.position, followSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(lookTarget.transform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

    }
}
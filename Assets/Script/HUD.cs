using TMPro;
using UnityEngine;

namespace ProceduralDriving
{
    public class HUD : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private CarController carController;
        [SerializeField] private Player player;

        [Header("Texts")]
        [SerializeField] private TMP_Text distanceText;
        [SerializeField] private TMP_Text speedText;

        private void Start()
        {
            UpdateTexts();
        }

        private void Update()
        {
            UpdateTexts();
        }

        private void UpdateTexts()
        {
            float distanceKm = player.DistanceTraveled * 0.001f;
            distanceText.text = $"{distanceKm:0.00} km";
            speedText.text = $"{Mathf.RoundToInt(carController.CurrentSpeed)} {carController._speedType}";
        }
    }
}
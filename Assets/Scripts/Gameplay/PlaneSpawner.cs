using UnityEngine;

namespace WreckWing.Gameplay
{
    public class PlaneSpawner : MonoBehaviour
    {
        [SerializeField] private float spawnHeight = 100f;
        [SerializeField] private float spawnRange = 50f;
        [SerializeField] private Transform spawnParent;

        public PlaneController SpawnPlane(PlaneData planeData)
        {
            if (planeData?.prefab == null) return null;

            float randomX = Random.Range(-spawnRange, spawnRange);
            Vector3 spawnPosition = new Vector3(randomX, spawnHeight, 0f);

            GameObject planeObj = Instantiate(planeData.prefab, spawnPosition, Quaternion.identity, spawnParent);
            PlaneController controller = planeObj.GetComponent<PlaneController>();

            if (controller != null)
            {
                controller.SetPlaneData(planeData);
                controller.OnCrashed += () => OnPlaneCrashed(controller);
            }

            return controller;
        }

        private void OnPlaneCrashed(PlaneController plane)
        {
            Debug.Log($"[PlaneSpawner] Plane crashed: {plane.Data.planeName}");
        }
    }
}

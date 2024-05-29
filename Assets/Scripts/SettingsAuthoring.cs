using Unity.Entities;
using UnityEngine;

namespace Boids
{
    public class SettingsAuthoring : MonoBehaviour
    {
        public int unitCount;
        public GameObject unitPrefab;

        private class Baker : Baker<SettingsAuthoring>
        {
            public override void Bake(SettingsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                var settings = new Settings
                {
                    UnitCount = authoring.unitCount,
                    UnitPrefab = GetEntity(authoring.unitPrefab, TransformUsageFlags.Dynamic)
                };

                AddComponent(entity, settings);
            }
        }
    }

    public struct Settings : IComponentData
    {
        public int UnitCount;
        public Entity UnitPrefab;
    }
}
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PlanBuild.Blueprints
{
    abstract class MovingRuneCircle : MonoBehaviour
    {
        internal float speed = 2f;
        internal Transform runeCircleTransform;
        internal ZNetView m_nview;
        internal Vector3 relativeTargetPosition;

        public void Awake()
        {
            runeCircleTransform = transform.Find("rune_circle");
            m_nview = GetComponent<ZNetView>();
        }

        internal abstract bool UpdateCircle();

        internal abstract void OnMovementComplete();

        public void Update()
        {
            Vector3 previousPosition = runeCircleTransform.localPosition;
            Vector3 newPosition = Vector3.MoveTowards(previousPosition, relativeTargetPosition, Time.deltaTime * speed);
            runeCircleTransform.localPosition = newPosition;

            if(!UpdateCircle())
            {
                return;
            }

            if (Vector3.Distance(previousPosition, relativeTargetPosition) < 0.001f)
            {
                OnMovementComplete();
                Destroy(gameObject);
            }
        }

        public static GameObject  SpawnRuneCircle<T>(Vector3 position, Quaternion rotation, Bounds bounds, out T runeCircleComponent) where T : MovingRuneCircle
        { 
            Vector3 circlePosition = bounds.center;
            circlePosition.y = bounds.min.y;
#if DEBUG
            Jotunn.Logger.LogDebug($"Circle spawn position @ {circlePosition}");
#endif

            float magnitude = new Vector2(bounds.extents.x, bounds.extents.z).magnitude + 1f;
#if DEBUG
            Jotunn.Logger.LogDebug($"Magnitude of rune circle: {magnitude}");
#endif

            Vector3 targetPosition = bounds.center;
            targetPosition.y = bounds.max.y + 0.3f;

#if DEBUG
            Jotunn.Logger.LogDebug($"Circle target position @ {targetPosition}");
#endif

            GameObject circlePrefab = PrefabManager.Instance.GetPrefab(BlueprintRunePrefab.BlueprintCaptureFXCircle);
            GameObject circleObject = Object.Instantiate(circlePrefab, position, rotation);

            Transform placerTransform = circleObject.transform;
            for (int i = 0; i < placerTransform.childCount; i++)
            {
                Transform effectTransform = placerTransform.GetChild(i);
                effectTransform.localPosition = circlePosition;
                effectTransform.localScale = Vector3.one * magnitude;
            }

            runeCircleComponent = circleObject.AddComponent<T>();
            runeCircleComponent.relativeTargetPosition = targetPosition;

            return circleObject;
        }
    }
}

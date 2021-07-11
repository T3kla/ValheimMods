using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PlanBuild.Blueprints
{
    class BlueprintCapture : MonoBehaviour
    { 
        internal float speed = 1f;
        internal IOrderedEnumerable<Piece> m_pieces;
        
        internal Vector3 relativeTargetPosition; 
        private Queue<Vector3> poofQueue;

        internal Transform runeCircleTransform;
        private ZNetView m_nview;

        public void Awake()
        {
            runeCircleTransform = transform.Find("rune_circle");
            m_nview = GetComponent<ZNetView>();
        }

        public void Start()
        {
            poofQueue = new Queue<Vector3>(m_pieces.Select(piece => piece.transform.position));
        }

        public void Update()
        {
            Vector3 previousPosition = runeCircleTransform.localPosition;
            Vector3 newPosition = Vector3.MoveTowards(previousPosition, relativeTargetPosition, Time.deltaTime * speed);
            runeCircleTransform.localPosition = newPosition;

            GameObject heldItem = Player.m_localPlayer.m_visEquipment.m_rightItemInstance;
            ParticleSystemForceField forceField = null;
            if (heldItem) {
                forceField = heldItem.GetComponentInChildren<ParticleSystemForceField>();
            }

            while (poofQueue.Count > 0)
            {
                Vector3 next = poofQueue.Peek();
                if(next.y < newPosition.y)
                {
                    return;
                }
                Vector3 current = poofQueue.Dequeue();
                GameObject runePoof = Object.Instantiate(BlueprintRunePrefab.runePoofPrefab, current, Quaternion.identity);
                if(forceField)
                {
                    ParticleSystem particleSystem = runePoof.GetComponentInChildren<ParticleSystem>();
                    ParticleSystem.ExternalForcesModule externalForces = particleSystem.externalForces;
                    externalForces.enabled = true;
                    externalForces.AddInfluence(forceField);

                    ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particleSystem.velocityOverLifetime;
                    velocityOverLifetime.enabled = false;
                }
            }
            
            if (Vector3.Distance(previousPosition, relativeTargetPosition) < 0.001f)
            {
                Jotunn.Logger.LogInfo("Capture process complete!");
                Destroy(gameObject);
            }
        } 
    }
}

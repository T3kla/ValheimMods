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
    class BlueprintCapture : MovingRuneCircle
    {  
        internal IOrderedEnumerable<Piece> m_pieces;
        private Queue<Vector3> poofQueue;
          
        public void Start()
        {
            poofQueue = new Queue<Vector3>(m_pieces.Select(piece => piece.transform.position));
        }
        
        internal override bool UpdateCircle()
        { 
            GameObject heldItem = Player.m_localPlayer.m_visEquipment.m_rightItemInstance;
            ParticleSystemForceField forceField = null;
            if (heldItem)
            {
                forceField = heldItem.GetComponentInChildren<ParticleSystemForceField>();
            }

            while (poofQueue.Count > 0)
            {
                Vector3 next = poofQueue.Peek();
                if (next.y < runeCircleTransform.localPosition.y)
                {
                    return false;
                }
                Vector3 current = poofQueue.Dequeue();
                GameObject runePoof = Object.Instantiate(BlueprintRunePrefab.runePoofPrefab, current, Quaternion.identity);
                if (forceField)
                {
                    ParticleSystem particleSystem = runePoof.GetComponentInChildren<ParticleSystem>();
                    ParticleSystem.ExternalForcesModule externalForces = particleSystem.externalForces;
                    externalForces.enabled = true;
                    externalForces.AddInfluence(forceField);

                    ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particleSystem.velocityOverLifetime;
                    velocityOverLifetime.enabled = false;
                }
            }
            return true;
        }

        internal override void OnMovementComplete()
        {
            Jotunn.Logger.LogInfo("Capture process complete!");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlanBuild.Blueprints
{
    internal class ToolPiece : MonoBehaviour
    {
        private void Awake()
        {
            Jotunn.Logger.LogMessage($"{gameObject} awoken");
            On.Player.UpdatePlacementGhost += OnUpdatePlacementGhost;
        }

        private void OnDestroy()
        {
            Jotunn.Logger.LogMessage($"{gameObject} destroyed");
            On.Player.UpdatePlacementGhost -= OnUpdatePlacementGhost;
        }

        /// <summary>
        ///     Flatten the circle selector transform
        /// </summary>
        private void OnUpdatePlacementGhost(On.Player.orig_UpdatePlacementGhost orig, Player self, bool flashGuardStone)
        {
            orig(self, flashGuardStone);

            if (self.m_placementMarkerInstance && self.m_placementGhost &&
                (name.Equals(BlueprintRunePrefab.BlueprintCaptureName) ||
                 name.Equals(BlueprintRunePrefab.BlueprintDeleteName) ||
                 name.Equals(BlueprintRunePrefab.BlueprintTerrainName))
               )
            {
                self.m_placementMarkerInstance.transform.up = Vector3.back;
            }
        }

    }
}

﻿using HarmonyLib;
using PlanBuild.Blueprints;
using PlanBuild.Plans;
using System;
using UnityEngine;
using ValheimRAFT;

namespace PlanBuild.ModCompat
{
    internal class PatcherValheimRaft
    {  
        private PatcherValheimRaft()
        {

        }
          
        [HarmonyPatch(typeof(PlanPiece), "HasSupport")]
        [HarmonyPrefix]
        private static bool PlanPiece_HasSupport_Prefix(PlanPiece __instance, ref bool __result)
        {
            if(__instance.GetComponentInParent<MoveableBaseRoot>())
            {
                __result = true;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(PlanPiece), "OnPiecePlaced")]
        [HarmonyPrefix]
        private static void PlanPiece_OnPiecePlaced_Postfix(PlanPiece __instance, GameObject actualPiece)
        {
            MoveableBaseRoot moveableBaseRoot = __instance.GetComponentInParent<MoveableBaseRoot>();
            if (moveableBaseRoot) {
                moveableBaseRoot.AddNewPiece(actualPiece.GetComponent<Piece>());
            }
        }

        [HarmonyPatch(typeof(BlueprintManager), "OnPiecePlaced")]
        [HarmonyPrefix]
        private static void BlueprintManager_OnPiecePlaced_Postfix(GameObject placedPiece)
        {
            ValheimRAFT.Patches.ValheimRAFT_Patch.PlacedPiece(Player.m_localPlayer, placedPiece);
        }
    }
}
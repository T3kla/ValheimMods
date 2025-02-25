﻿// HaulingCart
// a Valheim mod skeleton using Jötunn
// 
// File:    HaulingCart.cs
// Project: HaulingCart

using BepInEx;
using BepInEx.Configuration;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace HaulingCart
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class HaulingCart : BaseUnityPlugin
    {
        public const string PluginGUID = "marcopogo.jotunnmodstub";
        public const string PluginName = "HaulingCart";
        public const string PluginVersion = "0.0.1";

        private void Awake()
        {
            // Do all your init stuff here
            // Acceptable value ranges can be defined to allow configuration via a slider in the BepInEx ConfigurationManager: https://github.com/BepInEx/BepInEx.ConfigurationManager
            Config.Bind<int>("Main Section", "Example configuration integer", 1, new ConfigDescription("This is an example config, using a range limitation for ConfigurationManager", new AcceptableValueRange<int>(0, 100)));

            // Jotunn comes with its own Logger class to provide a consistent Log style for all mods using it
            Jotunn.Logger.LogInfo("ModStub has landed");

            CustomPiece cpHeart = null;
            CustomPiece cpExpander = null;

            cpHeart.Piece.m_craftingStation = PrefabManager.Cache.GetPrefab<CraftingStation>("piece_workbench");
            CraftingStation customCraftingStation = cpHeart.PiecePrefab.AddComponent<CraftingStation>();
            customCraftingStation.m_name = "$piece_TS_Expander_CS";

            cpExpander.Piece.m_craftingStation = customCraftingStation;
            cpExpander.Piece.m_resources = new Piece.Requirement[]{
                MockRequirement.Create("Stone", 10),
                MockRequirement.Create("Wood", 10)
            };
            cpExpander.PiecePrefab.AddComponent<CraftingStation>();
            cpExpander.PiecePrefab.GetComponent<CraftingStation>().m_name = "$piece_TS_Expander_CS";
            cpExpander.PiecePrefab.GetComponent<CraftingStation>().m_rangeBuild = 45; // 50 or 45 - the range is for the player *not* the piece.
        }

#if DEBUG
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F9))
            { // Set a breakpoint here to break on F9 key press
                Jotunn.Logger.LogInfo("Right here");
            }
        }
#endif
    }
}
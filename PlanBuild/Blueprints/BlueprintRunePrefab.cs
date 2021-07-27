using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using System.Collections.Generic;

namespace PlanBuild.Blueprints
{
    internal class BlueprintRunePrefab
    {
        public const string PieceTableName = "_BlueprintPieceTable";
        public const string CategoryTools = "Tools";
        public const string CategoryBlueprints = "Blueprints";

        public const string BlueprintRuneName = "BlueprintRune";

        public const string BlueprintSnapPointName = "piece_bpsnappoint";
        public const string BlueprintCenterPointName = "piece_bpcenterpoint";
        public const string BlueprintCaptureName = "piece_bpcapture";
        public const string BlueprintDeleteName = "piece_bpdelete";
        public const string BlueprintTerrainName = "piece_bpterrain";
        public const string StandingBlueprintRuneName = "piece_world_standing_blueprint_rune";
        public const string BlueprintRuneStackName = "piece_world_blueprint_rune_stack";

        public static string BlueprintRuneItemName;

        public BlueprintRunePrefab(AssetBundle assetBundle)
        {
            // Rune piece table
            CustomPieceTable table = new CustomPieceTable(PieceTableName, new PieceTableConfig
            {
                UseCategories = false,
                UseCustomCategories = true,
                CustomCategories = new string[]
                {
                    CategoryTools, CategoryBlueprints
                }
            });
            PieceManager.Instance.AddPieceTable(table);

            // Asset Bundle prefabs
            GameObject[] prefabArray = assetBundle.LoadAllAssets<GameObject>();
            Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>(prefabArray.Length);
            for (int i = 0; i < prefabArray.Length; i++)
            {
                prefabs.Add(prefabArray[i].name, prefabArray[i]);
            }

            // Stub piece
            PrefabManager.Instance.AddPrefab(prefabs[Blueprint.PieceBlueprintName]);
                
            // Blueprint rune
            CustomItem item = new CustomItem(prefabs[BlueprintRuneName], false, new ItemConfig
            {
                Amount = 1,
                Requirements = new RequirementConfig[]
            {
            new RequirementConfig {Item = "Stone", Amount = 1}
            }
            });
            ItemManager.Instance.AddItem(item);
            BlueprintRuneItemName = item.ItemDrop.m_itemData.m_shared.m_name;

            // World runes
            foreach (string pieceName in new string[]
            {
                StandingBlueprintRuneName, BlueprintRuneStackName
            })
            {
                CustomPiece piece = new CustomPiece(prefabs[pieceName], new PieceConfig
                {
                    PieceTable = "Hammer",
                    Requirements = new RequirementConfig[] {
                        new RequirementConfig
                        {
                            Item = "Stone",
                            Amount = 5,
                            Recover= true
                        }
                    }
                });
                piece.PiecePrefab.AddComponent<WorldBlueprintRune>();
                piece.FixReference = true;
                PieceManager.Instance.AddPiece(piece);
            }

            // Tool pieces
            foreach (string pieceName in new string[]
            {
                BlueprintCaptureName, BlueprintSnapPointName, BlueprintCenterPointName,
                BlueprintDeleteName, BlueprintTerrainName
            })
            {
                CustomPiece piece = new CustomPiece(prefabs[pieceName], new PieceConfig
                {
                    PieceTable = PieceTableName,
                    Category = CategoryTools
                });
                piece.PiecePrefab.AddComponent<ToolPiece>();
                PieceManager.Instance.AddPiece(piece);
            }
        }
    }
}
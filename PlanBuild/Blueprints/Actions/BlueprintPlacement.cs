using Jotunn.Managers;
using PlanBuild.Plans;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace PlanBuild.Blueprints
{
    class BlueprintPlacement : MovingRuneCircle
    {   
        internal Blueprint blueprint;
        internal bool placeDirect;
        internal long creatorID;
         
        private Queue<PieceEntry> constructionQueue;
        private readonly List<WearNTear> placedWearNTears = new List<WearNTear>();
 
        private ZDO blueprintZDO;
        private ZDOID m_uid;
        private ZDOIDSet createdPlans;
         
        public void Start()
        { 
            GameObject blueprintPrefab = PrefabManager.Instance.GetPrefab(Blueprint.PieceBlueprintName);
            GameObject blueprintObject = Object.Instantiate(blueprintPrefab, transform.position, transform.rotation);

            blueprintZDO = blueprintObject.GetComponent<ZNetView>().GetZDO();
            m_uid = blueprintZDO.m_uid;
            blueprintZDO.Set(Blueprint.ZDOBlueprintName, blueprint.Name);
            createdPlans = new ZDOIDSet();
            
            constructionQueue = new Queue<PieceEntry>(blueprint.PieceEntries.OrderBy(entry => entry.posY));
        }
        
        internal override bool UpdateCircle()
        {

            uint cntEffects = 0u;
            uint maxEffects = 5u;

            while (constructionQueue.Count > 0)
            {
                PieceEntry next = constructionQueue.Peek();
                if (next.posY > runeCircleTransform.localPosition.y)
                {
                    return false;
                }
                PieceEntry entry = constructionQueue.Dequeue();
                // Final position
                Vector3 entryPosition = transform.TransformPoint(entry.GetPosition());
#if DEBUG
                Jotunn.Logger.LogDebug($"Placing entry {entry.name} @ {entryPosition}, {constructionQueue.Count} remaining");
#endif


                if (!m_nview.IsOwner())
                {
                    continue;
                }

                // Final rotation
                Quaternion entryQuat = transform.rotation * entry.GetRotation();
                // Get the prefab of the piece or the plan piece
                string prefabName = entry.name;
                if (!placeDirect)
                {
                    prefabName += PlanPiecePrefab.PlannedSuffix;
                }

                GameObject prefab = PrefabManager.Instance.GetPrefab(prefabName);
                if (!prefab)
                {
                    Jotunn.Logger.LogWarning($"{entry.name} not found, you are probably missing a dependency for blueprint {blueprint.Name}, not placing @ {entryPosition}");
                    continue;
                }

                // Instantiate a new object with the new prefab
                GameObject gameObject = Instantiate(prefab, entryPosition, entryQuat);

                ZNetView zNetView = gameObject.GetComponent<ZNetView>();
                if (!zNetView)
                {
                    Jotunn.Logger.LogWarning($"No ZNetView for {gameObject}!!??");
                }
                else if (gameObject.TryGetComponent(out PlanPiece planPiece))
                {
                    planPiece.PartOfBlueprint(m_uid, entry);
                    createdPlans.Add(planPiece.GetPlanPieceID());
                }

                // Register special effects
                CraftingStation craftingStation = gameObject.GetComponentInChildren<CraftingStation>();
                if (craftingStation)
                {
                    Player.m_localPlayer.AddKnownStation(craftingStation);
                }
                Piece newpiece = gameObject.GetComponent<Piece>();
                if (newpiece)
                {
                    newpiece.SetCreator(creatorID);
                }
                PrivateArea privateArea = gameObject.GetComponent<PrivateArea>();
                if (privateArea)
                {
                    privateArea.Setup(Game.instance.GetPlayerProfile().GetName());
                }
                WearNTear wearntear = gameObject.GetComponent<WearNTear>();
                if (wearntear)
                {
                    wearntear.enabled = false;
                    placedWearNTears.Add(wearntear);
                }
                TextReceiver textReceiver = gameObject.GetComponent<TextReceiver>();
                if (textReceiver != null)
                {
                    textReceiver.SetText(entry.additionalInfo);
                }

                // Limited build effects
                if (cntEffects < maxEffects)
                {
                    newpiece.m_placeEffect.Create(gameObject.transform.position, Quaternion.Euler(0, Random.Range(0, 360), 0), gameObject.transform, 1f);
                    if (placeDirect)
                    {
                        Player.m_localPlayer.AddNoise(50f);
                    }
                    cntEffects++;
                }

                // Count up player builds
                Game.instance.GetPlayerProfile().m_playerStats.m_builds++;
            }

            blueprintZDO.Set(PlanPiece.zdoBlueprintPiece, createdPlans.ToZPackage().GetArray());

            return true;
        }

        internal override void OnMovementComplete()
        {
            Jotunn.Logger.LogDebug($"Placement process complete for blueprint {blueprint.Name}!");
            foreach (WearNTear wearNTear in placedWearNTears)
            {
                wearNTear.enabled = true;
                wearNTear.OnPlaced();
            }
        }
    }
}

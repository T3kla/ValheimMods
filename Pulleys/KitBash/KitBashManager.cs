﻿using Jotunn;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PlanBuild.KitBash
{
    class KitBashManager
    { 
        private GameObject kitBashRoot;
        private static KitBashManager _instance;
        private readonly List<KitBashObject> kitBashObjects = new List<KitBashObject>();

        public static KitBashManager Instance
        {
            get
            {
                if (_instance == null) _instance = new KitBashManager();
                return _instance;
            }
        }

        private KitBashManager()
        {
            kitBashRoot = new GameObject("KitBashRoot");
            Object.DontDestroyOnLoad(kitBashRoot);
            kitBashRoot.SetActive(false);

            PieceManager.OnPiecesRegistered += ApplyKitBashes;
        }

        private void ApplyKitBashes()
        {
            try
            {

                if (kitBashObjects.Count == 0)
                {
                    return;
                }
                Jotunn.Logger.LogDebug("Applying KitBash in " + kitBashObjects.Count + " objects");
                foreach (KitBashObject kitBashObject in kitBashObjects)
                {
                    try
                    {
                        if (kitBashObject.Config.FixReferences)
                        {
                            kitBashObject.Prefab.FixReferences();
                        }
                        kitBashObject.ApplyKitBash();
                    }
                    catch (Exception e)
                    {
                        Jotunn.Logger.LogError(e);
                    }
                }
            }
            finally
            {
                PieceManager.OnPiecesRegistered -= ApplyKitBashes;
            }
        }

        public KitBashObject KitBash(GameObject embeddedPrefab, KitBashConfig kitBashConfig)
        { 
            GameObject kitbashedPrefab = Object.Instantiate(embeddedPrefab, kitBashRoot.transform);
            kitbashedPrefab.name = embeddedPrefab.name + "_kitbash";
            KitBashObject kitBashObject = new KitBashObject
            {
                Config = kitBashConfig,
                Prefab = kitbashedPrefab
            };
            kitBashObjects.Add(kitBashObject);
            return kitBashObject;
        }

        public bool KitBash(GameObject kitbashedPrefab, KitBashSourceConfig config)
        {
            GameObject sourcePrefab = PrefabManager.Instance.GetPrefab(config.sourcePrefab);
            if (!sourcePrefab)
            {
                Jotunn.Logger.LogWarning("No prefab found for " + config);
                return false;
            }
            GameObject sourceGameObject = sourcePrefab.transform.Find(config.sourcePath).gameObject;

            Transform parentTransform = config.targetParentPath != null ? kitbashedPrefab.transform.Find(config.targetParentPath) : kitbashedPrefab.transform;
            if (parentTransform == null)
            {
                Jotunn.Logger.LogWarning("Target parent not found for " + config);
                return false;
            }
            GameObject kitBashObject = Object.Instantiate(sourceGameObject, parentTransform);
            kitBashObject.name = config.name;
            kitBashObject.transform.localPosition = config.position;
            kitBashObject.transform.localRotation = config.rotation;
            kitBashObject.transform.localScale = config.scale;

            if (config.materialPath != null)
            {
                Material[] sourceMaterials = GetSourceMaterials(config, sourcePrefab);

                if (sourceMaterials == null)
                {
                    Jotunn.Logger.LogWarning("No materials for " + config);
                    return false;
                }

                if (config.materialRemap != null)
                {
                    Material[] remapped = new Material[sourceMaterials.Length];
                    for (int i = 0; i < sourceMaterials.Length; i++)
                    {
                        remapped[config.materialRemap[i]] = sourceMaterials[i];
                    }
                    sourceMaterials = remapped;
                }

                SkinnedMeshRenderer[] targetSkinnedMeshRenderers = kitBashObject.GetComponentsInChildren<SkinnedMeshRenderer>();

                foreach (SkinnedMeshRenderer targetSkinnedMeshRenderer in targetSkinnedMeshRenderers)
                {
                    targetSkinnedMeshRenderer.sharedMaterials = sourceMaterials;
                    targetSkinnedMeshRenderer.materials = sourceMaterials;
                }

                MeshRenderer[] targetMeshRenderers = kitBashObject.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer targetMeshRenderer in targetMeshRenderers)
                {
                    targetMeshRenderer.sharedMaterials = sourceMaterials;
                    targetMeshRenderer.materials = sourceMaterials;
                }

            }
            return true;
        }

        private Material[] GetSourceMaterials(KitBashSourceConfig config, GameObject sourcePrefab)
        {
            GameObject materialPrefab = config.materialPrefab != null ? PrefabManager.Instance.GetPrefab(config.materialPrefab) : sourcePrefab;
            GameObject materialSourceObject = materialPrefab.transform.Find(config.materialPath).gameObject;
            MeshRenderer[] sourceMeshRenderers = materialSourceObject.GetComponentsInChildren<MeshRenderer>();
            SkinnedMeshRenderer[] sourceSkinnedMeshRenderers = materialSourceObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (MeshRenderer meshRenderer in sourceMeshRenderers)
            {
                return meshRenderer.sharedMaterials;
            }
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in sourceSkinnedMeshRenderers)
            {
                return skinnedMeshRenderer.sharedMaterials;
            }
            return null;
        }
    }
}

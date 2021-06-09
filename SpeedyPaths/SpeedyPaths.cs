using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using ServerSync;
using System;
using System.Linq;

namespace SpeedyPaths
{

    [BepInPlugin(SpeedyPathsClientMod.GUID, SpeedyPathsClientMod.ModName, SpeedyPathsClientMod.Version)]
    public class SpeedyPathsClientMod : BaseUnityPlugin
    {
        public enum GroundType
        {
            Untamed,
            PathDirt,
            PathStone,
            Cultivated,
            StructureWood,
            StructureHardWood,
            StructureStone,
            StructureIron
        }
        public const string Version = "1.0.6";
        public const string ModName = "Speedy Paths Mod";
        public const string GUID = "nex.SpeedyPaths";
        public static ServerSync.ConfigSync configSync = new ServerSync.ConfigSync(GUID) { DisplayName = ModName, CurrentVersion = Version };
        private static Dictionary<GroundType, ConfigEntry<float>> _speedModifiers = new Dictionary<GroundType, ConfigEntry<float>>();
        private static Dictionary<GroundType, ConfigEntry<float>> _staminaModifiers = new Dictionary<GroundType, ConfigEntry<float>>();
        private static Dictionary<Heightmap.Biome, ConfigEntry<float>> _untamedSpeedModifiers = new Dictionary<Heightmap.Biome, ConfigEntry<float>>();
        private static Dictionary<Heightmap.Biome, ConfigEntry<float>> _untamedStaminaModifiers = new Dictionary<Heightmap.Biome, ConfigEntry<float>>();
        private static ConfigEntry<bool> _showHudStatus;
        private static ConfigEntry<string> _hudStatusText;
        private static ConfigEntry<bool> _hudDynamicStatusText;
        private static ConfigEntry<bool> _hudShowEffectPercent;
        private static List<ConfigEntry<float>> _hudPosIconThresholds = new List<ConfigEntry<float>>();
        private static List<ConfigEntry<float>> _hudNegIconThresholds = new List<ConfigEntry<float>>();
        public static ConfigEntry<bool> serverConfigLocked;


        //Display Strings
        private static Dictionary<GroundType, ConfigEntry<string>> _groundTypeStrings = new Dictionary<GroundType, ConfigEntry<string>>();
        private static Dictionary<Heightmap.Biome, ConfigEntry<string>> _biomeTypeStrings = new Dictionary<Heightmap.Biome, ConfigEntry<string>>();

        private static int m_pieceLayer;
        private static object[] _worldToVertexArgs = new object[] { Vector3.zero, null, null };
        private static List<Sprite> _speedSprites = new List<Sprite>();
        AssetBundle _speedyassets;

        public new static BepInEx.Logging.ManualLogSource Logger;

        ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        public static AssetBundle GetAssetBundleFromResources(string fileName)
        {
            var execAssembly = Assembly.GetExecutingAssembly();

            var resourceName = execAssembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(fileName));

            using (var stream = execAssembly.GetManifestResourceStream(resourceName))
            {
                return AssetBundle.LoadFromStream(stream);
            }
        }
        void Awake()
        {
            Logger = base.Logger;

            if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                _speedyassets = GetAssetBundleFromResources("speedyassets");
                if (!_speedyassets)
                {
                    Logger.LogError($"Failed to read AssetBundle stream");
                    return;
                }
            }

            serverConfigLocked = config("General", "Lock Configuration", false, "Lock Configuration");
            _hudDynamicStatusText = config("Hud", "EnableDynamicStatusText", true, "Show surface type text as status effect name", false);
            _hudShowEffectPercent = config("Hud", "ShowStatusEffectPercent", true, "Show the status effect buff/debuff %", false);
            _showHudStatus = config("Hud", "ShowStatusIcon", true, "Show the speedypaths status icon in the hud", false);
            _hudStatusText = config("Hud", "StatusText", "Speedy Path", "Text shown above the status icon", false);
            _hudPosIconThresholds.Add(config("Hud", "BuffIcon +1", 1.0f, "Speed buff threshold to show lvl 1 buff icon"));
            _hudPosIconThresholds.Add(config("Hud", "BuffIcon +2", 1.39f, "Speed buff threshold to show lvl 2 buff icon"));
            _hudNegIconThresholds.Add(config("Hud", "DebuffIcon -1", 1.0f, "Speed buff threshold to show lvl 1 debuff icon"));
            _hudNegIconThresholds.Add(config("Hud", "DebuffIcon -2", 0.79f, "Speed buff threshold to show lvl 2 debuff icon"));

            _speedModifiers[GroundType.PathDirt] = config("SpeedModifiers", "DirtPathSpeed", 1.15f, "Modifier for speed while on dirt paths");
            _speedModifiers[GroundType.PathStone] = config("SpeedModifiers", "StonePathSpeed", 1.4f, "Modifier for speed while on stone paths");
            _speedModifiers[GroundType.Cultivated] = config("SpeedModifiers", "CultivatedSpeed", 1.0f, "Modifier for speed while on Cultivated land");
            _speedModifiers[GroundType.StructureWood] = config("SpeedModifiers", "StructureWoodSpeed", 1.15f, "Modifier for speed while on wood structures");
            _speedModifiers[GroundType.StructureHardWood] = config("SpeedModifiers", "StructureHardWoodSpeed", 1.15f, "Modifier for speed while on core wood structures");
            _speedModifiers[GroundType.StructureStone] = config("SpeedModifiers", "StructureStoneSpeed", 1.4f, "Modifier for speed while on stone structures");
            _speedModifiers[GroundType.StructureIron] = config("SpeedModifiers", "StructureIronSpeed", 1.4f, "Modifier for speed while on ironwood structures");

            _staminaModifiers[GroundType.PathDirt] = config("StaminaModifiers", "DirtPathStamina", 0.8f, "Modifier for stamina while on dirt paths");
            _staminaModifiers[GroundType.PathStone] = config("StaminaModifiers", "StonePathStamina", 0.7f, "Modifier for stamina while on stone paths");
            _staminaModifiers[GroundType.Cultivated] = config("StaminaModifiers", "CultivatedStamina", 1.0f, "Modifier for stamina while on Cultivated land");
            _staminaModifiers[GroundType.StructureWood] = config("StaminaModifiers", "StructureWoodStamina", 0.8f, "Modifier for stamina while on wood structures");
            _staminaModifiers[GroundType.StructureHardWood] = config("StaminaModifiers", "StructureHardWoodStamina", 0.8f, "Modifier for stamina while on core wood structures");
            _staminaModifiers[GroundType.StructureStone] = config("StaminaModifiers", "StructureStoneStamina", 0.7f, "Modifier for stamina while on stone structures");
            _staminaModifiers[GroundType.StructureIron] = config("StaminaModifiers", "StructureIronStamina", 0.7f, "Modifier for stamina while on ironwood structures");

            //Should handle new biomes automagically
            foreach (Heightmap.Biome biome in Enum.GetValues(typeof(Heightmap.Biome)))
            {
                if (biome == Heightmap.Biome.BiomesMax)
                {
                    break;
                }
                _untamedSpeedModifiers[biome] = Config.Bind("SpeedModifiers_Biomes", "Untamed_" + biome.ToString() + "_Speed", 1.0f, "Speed modifier for uncleared ground in " + biome.ToString() + " Biomes");
                _untamedStaminaModifiers[biome] = Config.Bind("StaminaModifiers_Biomes", "Untamed_" + biome.ToString() + "_Stamina", 1.0f, "Stamina modifier for uncleared ground in " + biome.ToString() + " Biomes");
                _biomeTypeStrings[biome] = Config.Bind("Strings", "Biome_" + biome.ToString(), "default", "Dynamic status mapping for " + biome.ToString() + " groundcover. 'default' uses localized biome name.");
            }

            _groundTypeStrings[GroundType.PathDirt] = Config.Bind("Strings", "PathDirt", "Dirt Path", "Dynamic status mapping for dirt paths");
            _groundTypeStrings[GroundType.PathStone] = Config.Bind("Strings", "PathStone", "Stone Path", "Dynamic status mapping for  stone paths");
            _groundTypeStrings[GroundType.Cultivated] = Config.Bind("Strings", "Cultivated", "Cultivated", "Dynamic status mapping for Cultivated land");
            _groundTypeStrings[GroundType.StructureWood] = Config.Bind("Strings", "StructureWood", "Wood", "Dynamic status mapping for wood structures");
            _groundTypeStrings[GroundType.StructureHardWood] = Config.Bind("Strings", "StructureHardWood", "Hardwood", "Dynamic status mapping for core wood structures");
            _groundTypeStrings[GroundType.StructureStone] = Config.Bind("Strings", "StructureStone", "Stone", "MDynamic status mapping for stone structures");
            _groundTypeStrings[GroundType.StructureIron] = Config.Bind("Strings", "StructureIron", "Iron", "Dynamic status mapping for ironwood structures");

            if (m_pieceLayer == 0)
            {
                m_pieceLayer = LayerMask.NameToLayer("piece");
            }

            _speedSprites.Add(_speedyassets.LoadAsset<Sprite>("assets/nex.speedypaths/speedypaths_1.png"));
            _speedSprites.Add(_speedyassets.LoadAsset<Sprite>("assets/nex.speedypaths/speedypaths_2.png"));
            _speedSprites.Add(_speedyassets.LoadAsset<Sprite>("assets/nex.speedypaths/speedypaths_n1.png"));
            _speedSprites.Add(_speedyassets.LoadAsset<Sprite>("assets/nex.speedypaths/speedypaths_n2.png"));

            Harmony.CreateAndPatchAll(typeof(SpeedyPathsClientMod));
        }

        private static float _activeSpeedModifier = 1.0f;
        private static float _activeStaminaModifier = 1.0f;
        private static String _activeStatusText;
        private static Sprite _activeStatusSprite;

        //hooks
        [HarmonyPatch(typeof(Player), "FixedUpdate")]
        [HarmonyPrefix]
        static void UpdateModifiers(Player __instance)
        {
            if (Player.m_localPlayer == __instance && !__instance.IsDead())
            {
                _activeSpeedModifier = GetSpeedyPathModifier(__instance);
                _activeStaminaModifier = GetSpeedyPathStaminaModifier(__instance);

                if (_activeSpeedModifier > 1.0f)
                {
                    for (int i = 0; i < _hudPosIconThresholds.Count; ++i)
                    {
                        if (_activeSpeedModifier > _hudPosIconThresholds[i].Value)
                        {
                            _activeStatusSprite = _speedSprites[i];
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < _hudNegIconThresholds.Count; ++i)
                    {
                        if (_activeSpeedModifier < _hudNegIconThresholds[i].Value)
                        {
                            _activeStatusSprite = _speedSprites[_hudPosIconThresholds.Count + i];
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Player), "CheckRun")]
        [HarmonyPrefix]
        static void CheckRunPrefixStaminaMod(Player __instance, out float __state)
        {
            __state = __instance.m_runStaminaDrain;
            __instance.m_runStaminaDrain *= _activeStaminaModifier;
        }
        [HarmonyPatch(typeof(Player), "CheckRun")]
        [HarmonyPostfix]
        static void CheckRunPostfixStaminaMod(Player __instance, float __state)
        {
            __instance.m_runStaminaDrain = __state;
        }

        [HarmonyPatch(typeof(Player), "GetJogSpeedFactor")]
        [HarmonyPostfix]
        static void JogSpeedPathFactor(Player __instance, ref float __result)
        {
            __result *= _activeSpeedModifier;
        }

        [HarmonyPatch(typeof(Player), "GetRunSpeedFactor")]
        [HarmonyPostfix]
        static void RunSpeedPathFactor(Player __instance, ref float __result)
        {
            __result *= _activeSpeedModifier;
        }

        private static StatusEffect _pathBuffSEDummy;
        private static float _prevModValue = 1.0f;
        [HarmonyPatch(typeof(Hud), "UpdateStatusEffects")]
        [HarmonyPrefix]
        static void UpdatePathIcon(Hud __instance, List<StatusEffect> statusEffects)
        {
            if (!Player.m_localPlayer.IsDead() && _showHudStatus.Value && _activeSpeedModifier != 1.0f)
            {
                if (_pathBuffSEDummy == null)
                {
                    _pathBuffSEDummy = ScriptableObject.CreateInstance(typeof(StatusEffect)) as StatusEffect;
                }
                _pathBuffSEDummy.m_name = _hudDynamicStatusText.Value ? _activeStatusText : _hudStatusText.Value;
                _pathBuffSEDummy.m_icon = _activeStatusSprite;

                if (_activeSpeedModifier != 1.0f)
                {
                    if (_activeSpeedModifier != _prevModValue)
                    {
                        _pathBuffSEDummy.m_isNew = true;
                    }
                    statusEffects.Add(_pathBuffSEDummy);
                }
                _prevModValue = _activeSpeedModifier;
            }
        }

        [HarmonyPatch(typeof(StatusEffect), "GetIconText")]
        [HarmonyPostfix]
        static void SpeedyPathsStatusEffect_GetIconText(StatusEffect __instance, ref string __result)
        {
            if (__instance == _pathBuffSEDummy && _hudShowEffectPercent.Value)
            {
                __result = (_activeSpeedModifier - 1.0f).ToString("P0");
            }
        }

        //util funcs
        static float GetSpeedyPathModifier(Player player)
        {
            if (!player.IsSwiming() && !player.InInterior())
            {
                GroundType groundtype = GetGroundType(player);
                if (_speedModifiers.ContainsKey(groundtype))
                {
                    _activeStatusText = _groundTypeStrings[groundtype].Value;
                    return _speedModifiers[groundtype].Value;
                }
                //fallback to biome speed
                Heightmap.Biome playerBiome = player.GetCurrentBiome();
                //Handle new biomes "gracefully"
                if (!_untamedSpeedModifiers.ContainsKey(playerBiome))
                {
                    Logger.LogWarning($"New biome {playerBiome.ToString()}. Unsure how to Handle. Falling back to None.");
                    playerBiome = Heightmap.Biome.None;
                }
                _activeStatusText = "$biome_" + playerBiome.ToString().ToLower();
                if (_biomeTypeStrings.ContainsKey(playerBiome) && _biomeTypeStrings[playerBiome].Value != "default")
                {
                    _activeStatusText = _biomeTypeStrings[playerBiome].Value;
                }
                return _untamedSpeedModifiers[playerBiome].Value;
            }
            return 1.0f;
        }

        static float GetSpeedyPathStaminaModifier(Player player)
        {
            if (!player.IsSwiming() && !player.InInterior())
            {
                GroundType groundtype = GetGroundType(player);
                if (_staminaModifiers.ContainsKey(groundtype))
                {
                    return _staminaModifiers[groundtype].Value;
                }
                //fallback to biome stamina
                Heightmap.Biome playerBiome = player.GetCurrentBiome();
                //Handle new biomes "gracefully"
                if (!_untamedStaminaModifiers.ContainsKey(playerBiome))
                {
                    Logger.LogWarning($"New biome {playerBiome.ToString()}. Unsure how to Handle. Falling back to None.");
                    playerBiome = Heightmap.Biome.None;
                }
                return _untamedStaminaModifiers[playerBiome].Value;
            }
            return 1.0f;
        }

        static GroundType GetGroundType(Player player)
        {
            Collider lastGroundCollider = player.GetLastGroundCollider();
            if ((bool)lastGroundCollider)
            {
                Heightmap hmap = lastGroundCollider.GetComponent<Heightmap>();
                if (hmap != null)
                {
                    Texture2D paintMask = Traverse.Create(hmap).Field("m_paintMask").GetValue() as Texture2D;
                    _worldToVertexArgs[0] = Traverse.Create(player).Field("m_lastGroundPoint").GetValue() as Vector3?;
                    AccessTools.Method(typeof(Heightmap), "WorldToVertex").Invoke(hmap, _worldToVertexArgs);
                    //Sample a 3x3 range of pixels at last groundpoint
                    int sample_radius = 1;
                    Color hmcl_pixel = new Color();
                    int samples = 0;
                    for (int x = -sample_radius; x <= sample_radius; ++x)
                    {
                        for (int y = -sample_radius; y <= sample_radius; ++y)
                        {
                            int x_scan = (int)_worldToVertexArgs[1] + x;
                            int y_scan = (int)_worldToVertexArgs[2] + y;
                            if (x_scan >= 0 && x_scan < paintMask.width && y_scan >= 0 && y_scan < paintMask.height)
                            {
                                hmcl_pixel += paintMask.GetPixel(x_scan, y_scan);
                                samples++;
                            }
                        }
                    }
                    hmcl_pixel.r /= (float)samples;
                    hmcl_pixel.g /= (float)samples;
                    hmcl_pixel.b /= (float)samples;
                    hmcl_pixel.a /= (float)samples;
                    //Logger.LogInfo($"Avg ground pixel {hmcl_pixel.ToString()} from {samples.ToString()} Samples");
                    //Single Pixel
                    //Color hmcl_pixel = paintMask.GetPixel( (int)_worldToVertexArgs[1], (int)_worldToVertexArgs[2] );
                    if (hmcl_pixel.b > 0.4f)
                    {
                        return GroundType.PathStone;
                    }
                    else if (hmcl_pixel.r > 0.4f)
                    {
                        return GroundType.PathDirt;
                    }
                    else if (hmcl_pixel.g > 0.4f)
                    {
                        return GroundType.Cultivated;
                    }

                }
                //this is extremely generous, a kiln road will work...
                //todo: piece whitelist / blacklist?
                if (lastGroundCollider.gameObject.layer == m_pieceLayer)
                {
                    WearNTear componentInParent = lastGroundCollider.GetComponentInParent<WearNTear>();
                    if ((bool)componentInParent)
                    {
                        switch (componentInParent.m_materialType)
                        {
                            case WearNTear.MaterialType.Wood:
                                return GroundType.StructureWood;
                            case WearNTear.MaterialType.Stone:
                                return GroundType.StructureStone;
                            case WearNTear.MaterialType.HardWood:
                                return GroundType.StructureHardWood;
                            case WearNTear.MaterialType.Iron:
                                return GroundType.StructureIron;
                        }
                    }
                }
            }
            return GroundType.Untamed;
        }
    }
}

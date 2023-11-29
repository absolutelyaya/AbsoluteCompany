using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;
using LethalLib.Modules;

namespace AbsoluteCompany
{
    [BepInPlugin(guid, modName, modVersion)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    public class AbsoluteCompanyMod : BaseUnityPlugin
    {
        private const string guid = "Absolutelyaya.AbsoluteCompany", modName = "AbsoluteCompany", modVersion = "0.0.1";
        private readonly Harmony harmony = new Harmony(guid);

        private static AbsoluteCompanyMod Instance;
        internal ManualLogSource logger;
        public static AssetBundle Assets;
        private ConfigEntry<int> configBlueSkullRarity;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            logger = BepInEx.Logging.Logger.CreateLogSource(modName);
            logger.LogInfo("Absolutely Initialized.");
            Assets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "absolute"));
            configBlueSkullRarity = Config.Bind("Scrap", "BlueSkullRarity", 30, new ConfigDescription("How rare Blue Skulls are. Lower == Less.", new AcceptableValueRange<int>(0, 100)));
            //RegisterScrap("blueskull");
            //RegisterScrap("redskull");
            RegisterScrap("florp");
        }

        void RegisterScrap(string id)
        {
            Item item = Assets.LoadAsset<Item>(string.Format("assets/{0}.asset", id));
            if(item == null)
            {
                logger.LogError(string.Format("Failed to load prefab for Scrap '{0}'", id));
                return;
            }
            NetworkPrefabs.RegisterNetworkPrefab(item.spawnPrefab);
            Items.RegisterScrap(item, configBlueSkullRarity.Value, Levels.LevelTypes.All);
            logger.LogInfo(string.Format("registered Scrap '{0}'.", id));
        }
    }
}

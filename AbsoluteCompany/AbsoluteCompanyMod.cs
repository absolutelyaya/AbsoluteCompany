using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;
using LethalLib.Modules;
using AbsoluteCompany.patches;
using HarmonyLib.Tools;
using UnityEngine.Assertions;

namespace AbsoluteCompany
{
    [BepInPlugin(guid, modName, modVersion)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    public class AbsoluteCompanyMod : BaseUnityPlugin
    {
        private const string guid = "Absolutelyaya.AbsoluteCompany", modName = "AbsoluteCompany", modVersion = "1.0.1";
        private readonly Harmony harmony = new Harmony(guid);

        private static AbsoluteCompanyMod Instance;
        public static ManualLogSource logger;
        public static AssetBundle Assets;
        private ConfigEntry<int> configFlorpRarity;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            logger = BepInEx.Logging.Logger.CreateLogSource(modName);
            logger.LogInfo("Absolutely Initialized.");
            Assets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "absolute"));
            configFlorpRarity = Config.Bind("Scrap", "FlorpRarity", 30, new ConfigDescription("How rare Florp is. Lower == Less Spawns.", new AcceptableValueRange<int>(0, 100)));
            //RegisterScrap("blueskull");
            //RegisterScrap("redskull");
            RegisterScrap("florp");
            harmony.PatchAll(typeof(EnemyPatches));
            harmony.PatchAll(typeof(TerminalPatches));
        }

        void RegisterScrap(string id)
        {
            Item item = loadAsset<Item>(id + ".asset");
            if (item == null)
                return;
            NetworkPrefabs.RegisterNetworkPrefab(item.spawnPrefab);
            Items.RegisterScrap(item, configFlorpRarity.Value, Levels.LevelTypes.All);
            logger.LogInfo(string.Format("registered Scrap '{0}'.", id));
        }

        public static T loadAsset<T>(string id) where T : Object
        {
            T asset = Assets.LoadAsset<T>(string.Format("assets/{0}", id));
            if (asset == null)
            {
                logger.LogError(string.Format("Failed to load asset for '{0}'", id));
                return null;
            }
            return asset;
        }
    }
}

namespace AbsoluteCompany.patches
{
    [HarmonyPatch]
    internal class EnemyPatches
    {
        [HarmonyPatch(typeof(SpringManAI), "__initializeVariables")]
        [HarmonyPostfix]
        public static void CreateMannequinModel(SpringManAI __instance)
        {
            Transform modelBase = __instance.transform.Find("SpringManModel");
            Transform head = modelBase.transform.Find("Head");
            Transform body = modelBase.transform.Find("Body");
            Renderer[] models = head.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in models)
                r.enabled = false;
            models = body.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in models)
                r.enabled = false;

            Animator[] animators = modelBase.GetComponentsInChildren<Animator>();
            foreach (Animator r in animators)
                r.enabled = false;
            ScanNodeProperties scanNode = modelBase.GetComponentInChildren<ScanNodeProperties>();
            scanNode.headerText = "Mannequin";
            GameObject newModel = AbsoluteCompanyMod.loadAsset<GameObject>("prefabs/springmannequinmodel.prefab");
            if (newModel == null)
                return;
            GameObject instance = Object.Instantiate(newModel, __instance.gameObject.transform);
            instance.name = "SpringMannequinModel";
            instance.transform.Rotate(new Vector3(0, 90, 0), Space.Self);
            Animator animator = instance.GetComponentInChildren<Animator>();
            __instance.creatureAnimator = animator;
            __instance.animStopPoints = instance.GetComponentInChildren<AnimationStopPoints>();
            animator.Rebind();
        }
    }

    [HarmonyPatch]
    internal class TerminalPatches
    {
        [HarmonyPatch(typeof(Terminal), "Awake")]
        [HarmonyPostfix]
        public static void EditTerminal(Terminal __instance)
        {
            __instance.enemyFiles[7].creatureName = "Mannequin";
            __instance.enemyFiles[7].displayText = "Type: Lesser Demon\r\n\r\nDuring the departure of the angels after the Disappearance of God, many sinners attempted to escape the Violence layer, braving the labyrinth that lay at its edges, thinking they could find a way out in the absence of Heaven's wardens.\r\n\r\nThe fools who attempted would realize far too late that angels were not all that kept them from freedom, as the Garden of forking Paths is no ordinary labyrinth, but with malicious intent, overseen with cunning cruelty.\r\n\r\nThe halls, similar enough to give no sense of direction, but different enough to grant no familiarity, would eventually exhaust each escapee, and as they fell into a deep sleep, their metamorphosis began.\r\n\r\nEach sinner was torn apart joint by joint, their broken and shattered limbs shoved into hollow statues, starting them to life as the flesh and blood of Mannequins.\r\n\r\nThe gift of death is a rare privilege in Hell, so sinners who attempted to control their fate now lie in eternal agony, unable to now control even their own bodies, which now continue to carry on the same punishment to other unfortunate fools.\r\n\r\nThas fukced up - Sigurd";
            __instance.terminalNodes.allKeywords[46].word = "mannequin";
        }
    }
}
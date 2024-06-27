using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace LegendOfGaryStu
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        readonly GUIStyle textStyle = new()
        {
            alignment = TextAnchor.MiddleCenter
        };
        static bool showDiceWindow = false;
        static Mortal.Story.DiceMenuDialog? current = null;
        static int diceSize = 100;
        static bool useDestinyDice = false;
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            textStyle.normal.textColor = Color.yellow;
            textStyle.fontStyle = FontStyle.Bold;

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }
        private void OnGUI()
        {
            if (showDiceWindow && current != null)
            {
                var renderer = current.GetComponent<Renderer>();

                GUI.Window(8999644, new Rect((Camera.main.pixelWidth * 0.5f) - 100,
                    (Camera.main.pixelHeight * 0.3f) - 100, 200, 100), WindowFunc, "天命骰子");
            }
        }
        static float diceValue = 1;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Mortal.Story.CheckPointManager), "Dice")]
        static void PatchRandom(string name, ref int random)
        {
            try
            {
                if (showDiceWindow && useDestinyDice)
                {
                    random = (int)diceValue;
                    if (current != null)
                    {
                        DiceMenuDialogHelper.SetRandomValue(current, random);
                    }
                }
            }
            finally
            {
                useDestinyDice = false;
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Mortal.Story.DiceMenuDialog), "SetRandom")]
        static void PatchDiceMenuSetRandom(Mortal.Story.DiceMenuDialog __instance, int count, int value)
        {
            diceSize = count;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Mortal.Story.DiceMenuDialog), "OnEnable")]
        static void PatchDiceMenuOnEnable(Mortal.Story.DiceMenuDialog __instance)
        {
            current = __instance;
            useDestinyDice = false;
            showDiceWindow = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Mortal.Story.DiceMenuDialog), "OnDisable")]
        static void PatchDiceMenuOnDisable()
        {
            current = null;
            useDestinyDice = false;
            showDiceWindow = false;
        }
        public void WindowFunc(int winId)
        {
            if(current == null)
            {
                return;
            }
            using (new GUILayout.AreaScope(new Rect(10, 30, 180, 70)))
            {
                GUI.enabled = true;
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    if (diceValue > diceSize)
                    {
                        diceValue = diceSize;
                    }
                    diceValue = Mathf.RoundToInt(GUILayout.HorizontalSlider(diceValue, 1, diceSize, GUILayout.Width(180)));
                }
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"{diceValue} 調整後 {diceValue + DiceMenuDialogHelper.GetStatsValue(current)}", textStyle, GUILayout.Width(100));
                    GUILayout.FlexibleSpace();
                }
                GUILayout.Space(5);
                GUI.enabled = !DiceMenuDialogHelper.StartButtonPress(current) || 
                    (DiceMenuDialogHelper.ReStartButton(current).isActiveAndEnabled && !DiceMenuDialogHelper.ReStartButtonPress(current)) ;
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("天命骰啟動", GUILayout.Width(100)))
                    {
                        useDestinyDice = true;
                        if (DiceMenuDialogHelper.ReStartButton(current).isActiveAndEnabled)
                        {
                            current.PressRestartButton();
                        }
                        else
                        {
                            current.PressStartButton();
                        }
                    }
                    GUILayout.FlexibleSpace();
                }
                GUILayout.Space(5);
            }
        }
    }
}

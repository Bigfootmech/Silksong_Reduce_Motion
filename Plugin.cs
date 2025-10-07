using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace reduced_motion_accessibility;

[BepInPlugin(modGUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
	private const string modGUID = "com.bigfootmech.silksong.reduced_motion_accessibility";
    internal static new ManualLogSource Logger; // propagate to other static methods here
	private readonly Harmony harmony = new Harmony(modGUID);
        
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {modGUID} is loaded!");
		
		harmony.PatchAll();
    }
    
    [HarmonyPatch(typeof(MenuStyles), "Start")]
    public class MenuParticleKill
    {
	    [HarmonyPostfix]
	    static void Postfix(MenuStyles __instance)
	    {
			var fireBackgroundTfm = __instance.transform.Find("Hornet_Style");
			foreach(Transform childTfm in fireBackgroundTfm)
			{
				if(childTfm.name.StartsWith("lava_particles"))
                    childTfm.gameObject.SetActive(false);
					// turn off
			}
        }
    }

    [HarmonyPatch(typeof(InventoryCursor), "Awake")]
    public class InventoryCursorPreventJitter
    {
	    [HarmonyPostfix]
	    static void Postfix(InventoryCursor __instance
            , ref Transform ___topLeft
            , ref Transform ___topRight
            , ref Transform ___bottomLeft
            , ref Transform ___bottomRight
            )
	    {
            // Logger.LogInfo("Hello?");
            DisableAnimatorIfAvailable(___topLeft);
            DisableAnimatorIfAvailable(___topRight);
            DisableAnimatorIfAvailable(___bottomLeft);
            DisableAnimatorIfAvailable(___bottomRight);
            // if(___topLeft) Logger.LogInfo($"TopLeft = {___topLeft.name}");
            // if(___topRight) Logger.LogInfo($"TopRight = {___topRight.name}");
            // if(___bottomLeft) Logger.LogInfo($"BottomLeft = {___bottomLeft.name}");
            // if(___bottomRight) Logger.LogInfo($"BottomRight = {___bottomRight.name}");
        }

        private static void DisableAnimatorIfAvailable(Transform topLeft)
        {
            if(topLeft == null) return;
            var spriteObj = topLeft.GetChild(0);
            if(spriteObj == null) return;
            var animator = spriteObj.GetComponent<UnityEngine.Animator>();
            if(animator == null) return;
            animator.speed = 0;
        }
    }

    [HarmonyPatch(typeof(InvMarker), "Awake")]
    public class MapMarkersStopFlash
    {
	    [HarmonyPostfix]
	    static void Postfix(InvMarker __instance) // collider
	    {
            var markerCloneTfm = __instance.transform.parent;
            // Logger.LogInfo("markerCloneTfm? " + markerCloneTfm.name);

            Transform dunMarker = GrabExistingMarkerStartingAtPlayerOwnedPin(markerCloneTfm);
            // Logger.LogInfo("got existing marker? " + dunMarker.name);
            
            SpriteRenderer playerPinSpriteRenderer = markerCloneTfm.GetComponent<UnityEngine.SpriteRenderer>();
            // Logger.LogInfo("renderer? " + playerPinSpriteRenderer);
            SpriteRenderer dunMarkerSpriteRenderer = dunMarker.GetComponent<UnityEngine.SpriteRenderer>();
            // Logger.LogInfo("renderer? " + dunMarkerSpriteRenderer);
            
            playerPinSpriteRenderer.material = dunMarkerSpriteRenderer.material; // replace shiny with dun
        }

        private static Transform GrabExistingMarkerStartingAtPlayerOwnedPin(Transform markerCloneTfm)
        {
            var gameMapCloneTfm = markerCloneTfm.parent.parent;
            // Logger.LogInfo("gamemap? " + gameMapCloneTfm.name);
            var questPinContainer = gameMapCloneTfm.Find("Main Quest Pins");
            return TryGetMarkerChild(questPinContainer);
        }

        private static Transform TryGetMarkerChild(Transform questPinContainer)
        {
            var tryGetQuestPin = questPinContainer.Find("Quest_Pin_Citadel_Seeker");
            if(tryGetQuestPin != null) return tryGetQuestPin;
            return questPinContainer.GetChild(0);
        }
    }
}

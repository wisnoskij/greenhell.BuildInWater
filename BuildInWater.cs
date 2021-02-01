using HarmonyLib;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildInWater : Mod
{
	public const string ModName = "buildinwater";
	public const string HarmonyId = "com.wisnoski.greenhell." + ModName;
	public static Harmony instance;

	public void Start()
	{
		instance = new Harmony(HarmonyId);
        instance.PatchAll(Assembly.GetExecutingAssembly());
		if(Player.Get() != null){
			((int[,])Traverse.Create(Player.Get()).Field("m_PlayerControllerArray").GetValue())[(int)PlayerControllerType.HeavyObject, (int)PlayerControllerType.Swim] = 1;
		}
		Debug.Log(string.Format("Mod {0} has been loaded!", ModName));
	}
	
	public void OnModUnload()
	{
		GameObject InGameMenu = GameObject.Find("InGameMenu");
		if (InGameMenu != null && InGameMenu.transform.Find("MenuInGame").Find("Buttons").Find("ToggleSwim") != null)
        {
			Destroy(InGameMenu.transform.Find("MenuInGame").Find("Buttons").Find("ToggleSwim").gameObject);
        }
        instance.UnpatchAll(HarmonyId);
		if(Player.Get()){
			((int[,])Traverse.Create(Player.Get()).Field("m_PlayerControllerArray").GetValue())[(int)PlayerControllerType.HeavyObject, (int)PlayerControllerType.Swim] = 0;
		}
		Debug.Log(string.Format("Mod {0} has been unloaded!", ModName));
	}
}


[HarmonyPatch(typeof(Player))]
[HarmonyPatch("Start")]
internal class Patch_Start
{
	public static void Prefix(Player __instance)
	{
		((int[,])Traverse.Create(Player.Get()).Field("m_PlayerControllerArray").GetValue())[(int)PlayerControllerType.HeavyObject, (int)PlayerControllerType.Swim] = 1;
	}
}


class Myclass
{
	public static void Patch_UpdateInWater(){
		Traverse.Create(Player.Get()).Field("m_InSwimWater").SetValue(false);
	}
}


[HarmonyPatch(typeof(MenuInGame))]
[HarmonyPatch("OnShow")]
internal class Patch_MenuInGame_AddButton
{
	static bool swim = true;
	
	public static void Prefix(MenuInGame __instance)
	{
		GameObject InGameMenu = GameObject.Find("InGameMenu");
		if (InGameMenu.transform.Find("MenuInGame").Find("Buttons").Find("ToggleSwim") == null)
        {
			GameObject btn = GameObject.Instantiate(InGameMenu.transform.Find("MenuInGame").Find("Buttons").Find("Resume").gameObject, InGameMenu.transform.Find("MenuInGame").Find("Buttons"));
			btn.name = "ToggleSwim";
			btn.GetComponent<UIButtonEx>().onClick.AddListener(Patch_MenuInGame_AddButton.toggleSwim);
			InGameMenu.transform.Find("MenuInGame").GetComponent<MenuInGame>().AddMenuButton(btn.GetComponent<UIButtonEx>(), "Toggle Swim ");
			btn.GetComponentInChildren<Text>().text = "Toggle Swim (" + (swim ? "ON" : "OFF") + ")";
		}
	}
	
	public static void toggleMenu(){
		GameObject InGameMenu = GameObject.Find("InGameMenu");
		if (InGameMenu.transform.Find("MenuInGame").Find("Buttons").Find("ToggleSwim") != null){
			InGameMenu.transform.Find("MenuInGame").Find("Buttons").Find("ToggleSwim").GetComponentInChildren<Text>().text = "Toggle Swim (" + (swim ? "ON" : "OFF") + ")";
		}
	}
	
	public static void toggleSwim(){
		var original_UpdateInWater = typeof(Player).GetMethod("UpdateInWater", BindingFlags.NonPublic | BindingFlags.Instance);
		var postfix_noSwim = new HarmonyMethod(typeof(Myclass).GetMethod("Patch_UpdateInWater"));
		
		swim = !swim;
		if(swim){
			BuildInWater.instance.Unpatch(original_UpdateInWater, HarmonyPatchType.Postfix, BuildInWater.HarmonyId);
		}else{
			BuildInWater.instance.Patch(original_UpdateInWater, postfix: postfix_noSwim);
		}
		toggleMenu();
	}
}
using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using System.Collections;
using UnityEngine;
using WildfrostModMiya;

namespace Cursed3dSpritesMod
{
    [BepInPlugin("WildFrost.Bglobb.Cursed3dSprites", "Cursed 3D Sprites Mod", "0.0.1")]
    [BepInDependency("WildFrost.Miya.WildfrostAPI")]
    [BepInProcess("Wildfrost.exe")]
    public class BglobbPlugin : BasePlugin
    {
        internal static BglobbPlugin Instance;

        // This is how this must be run because the function is an IEnumerator
        public class Behaviour : MonoBehaviour
        {
            private void Start()
            {
                this.StartCoroutine(ModifyLilBerry());
            }
        }

        private static void HandleTaintedCard(string name, Func<CardData, CardData, CardData> extraAction)
        {
            var originalCardData = AddressableLoader.groups["CardData"].lookup[name].Cast<CardData>();
            var newCardData = originalCardData.InstantiateKeepName();
            newCardData = extraAction(originalCardData, newCardData);
            AddressableLoader.groups["CardData"].lookup[name] = newCardData;
            AddressableLoader.groups["CardData"].list.Remove(originalCardData);
            AddressableLoader.groups["CardData"].list.Add(newCardData);
        }
        public static IEnumerator ModifyLilBerry()
        {
            yield return new WaitUntil((Func<bool>)(() =>
                AddressableLoader.IsGroupLoaded("CardData")));
            Instance.Log.LogInfo("CardData is Loaded!");

            CardData card = AddressableLoader.groups["CardData"].list
                .Find(a => a.name.Equals("LilBerry"))
                .Cast<CardData>();

            var allImages = UnityEngine.Object.FindObjectsOfTypeIncludingAssets(Il2CppType.Of<UnityEngine.UI.Image>());
            UnityEngine.UI.Image image = allImages.ToList()
                .Find(a => a.Cast<UnityEngine.UI.Image>().m_Sprite == card.mainSprite)
                .Cast<UnityEngine.UI.Image>();

            image.m_Sprite = CardAdder.LoadSpriteFromCardPortraits("CardPortraits\\NewLittleBerry");
            Instance.Log.LogInfo("Image is injected!");
        }

        public override void Load()
        {
            Instance = this; 
            ClassInjector.RegisterTypeInIl2Cpp<Behaviour>();
            Harmony.CreateAndPatchAll(System.Reflection.Assembly.GetExecutingAssembly(), "WildFrost.Bglobb.BglobbCardPack");
            AddComponent<Behaviour>();
            CardAdder.OnAskForAddingCards += delegate (int i)
            {

                

                HandleTaintedCard("BigBerry", (originalData, createdData) => createdData
                  .SetSprites("CardPortraits\\NewBigBerry", "CardPortraits\\berryBck")
                  .AddToPets()
                 );

                HandleTaintedCard("HeartmistStation", (originalData, createdData) => createdData
                  .SetSprites("CardPortraits\\NewMist", "CardPortraits\\mistBck")
                  .AddToPets()
                 );

                // This won't actually modify his sprite, but it will set his background properly
                HandleTaintedCard("LilBerry", (originalData, createdData) => createdData
                  .SetSprites("CardPortraits\\NewLittleBerry", "CardPortraits\\berryBck")
                  .AddToPets()
                 );


            };
        }
    }
}

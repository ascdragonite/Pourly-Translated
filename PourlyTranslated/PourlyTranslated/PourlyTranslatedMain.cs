using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace PourlyTranslated
{

    [BepInPlugin("ggl_tr", "Poorly Translated", "1.0.0")]

    public class PourlyTranslatedMain : BaseUnityPlugin
    {

        public static bool initialized = false;
        private void OnEnable()
        {
            
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        }

        public static global::InGameTranslator.LanguageID Google = new global::InGameTranslator.LanguageID("Google", register: true);

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {

            orig.Invoke(self);
            if (!initialized)
            {
                try
                {
                    IL.InGameTranslator.LoadShortStrings += LoadShortStrings;
                    On.MoreSlugcats.ChatlogData.DecryptResult += ChatlogData_DecryptResult;
                    On.Options.DeveloperCommentaryLocalized += Options_DeveloperCommentaryLocalized;
                    initialized = true;
                }
                catch (Exception e) 
                {
                    Debug.Log($"[Poorly Translated]  Exception at RainWorld.OnModsInit:\n{e}");
                }

            }

        }

        private bool Options_DeveloperCommentaryLocalized(On.Options.orig_DeveloperCommentaryLocalized orig, Options self)
        {
            InGameTranslator.LanguageID language = RWCustom.Custom.rainWorld.inGameTranslator.currentLanguage;
            return language == InGameTranslator.LanguageID.English || language == InGameTranslator.LanguageID.French || language == InGameTranslator.LanguageID.Korean || language == Google;
        }

        private string ChatlogData_DecryptResult(On.MoreSlugcats.ChatlogData.orig_DecryptResult orig, string result, string path)
        {
            
            if (RWCustom.Custom.rainWorld.inGameTranslator.currentLanguage != Google)
            {

                int num = 0;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
                for (int i = 0; i < fileNameWithoutExtension.Length; i++)
                {
                    num += (int)(fileNameWithoutExtension[i] - '0');
                }
                return RWCustom.Custom.xorEncrypt(result, 54 + num + RWCustom.Custom.rainWorld.inGameTranslator.currentLanguage.Index * 7);
            }

            return (result);
        }

        private void LoadShortStrings(ILContext il)
        {

            ILCursor cursor = new ILCursor(il);
            int value = -1;
            try
            {
                cursor.GotoNext(
                    x => x.MatchCall("InGameTranslator", "get_currentLanguage"),
                    x => x.MatchCall(out MethodReference methodReference),
                    x => x.MatchStloc(out value),
                    x => x.MatchLdloc(out int num),
                    x => x.MatchLdcI4(out int num),
                    x => x.MatchLdelemRef(),
                    x => x.MatchCall("System.IO.File", "Exists"),
                    x => x.MatchBrtrue(out ILLabel illabel),
                    x => x.MatchRet()
                    );

            }
            catch (Exception e)
            {
                Debug.Log("[Poorly Translated] Cursor did not find instruction \n" + e);
            }
            //finds a sequence of instructions like this

            Debug.Log(cursor.ToString());

            cursor.Index += 3; //first call -> ldloc
            cursor.RemoveRange(6); //deletes the next 6 instructions (removes 25 -> 30)
            cursor.Emit(OpCodes.Ldloca, value); //???????
            cursor.Emit<PourlyTranslatedMain>(OpCodes.Call, "FiltrationSystem"); //makes an instruction that calls FiltrationSystem
        } 
        private static void FiltrationSystem(ref string[] files)
        {
            List<string> list = new List<string>(files);
            list.RemoveAll((string p) => !File.Exists(p));
            if (list.Count != files.Length)
            {
                files = list.ToArray();
            }
            //i have no idea what this part of the code does but if i remove it everything breaks
        }
    }
}
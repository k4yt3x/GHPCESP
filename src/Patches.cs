using GHPC;
using HarmonyLib;

namespace GHPCESP
{
    internal class Patches
    {
        [HarmonyPatch(typeof(Unit), "Start")]
        internal static class Unit_Start
        {
            private static void Postfix(Unit __instance)
            {
                if (!GHPCESP.Instance.Units.Contains(__instance))
                {
                    GHPCESP.Instance.Units.Add(__instance);
                }
            }
        }
    }
}

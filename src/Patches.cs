using GHPC;
using GHPC.Infantry;
using HarmonyLib;

namespace GHPCESP
{
    internal class Patches
    {
        // InfantryUnit has its own Start method so it will be not affected by this patch
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

        [HarmonyPatch(typeof(InfantryUnit), "Start")]
        internal static class InfantryUnit_Start
        {
            private static void Postfix(Unit __instance)
            {
                if (!GHPCESP.Instance.InfantryUnits.Contains(__instance))
                {
                    GHPCESP.Instance.InfantryUnits.Add(__instance);
                }
            }
        }
    }
}

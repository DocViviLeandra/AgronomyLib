using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace AgronomyLib {

    [HarmonyPatch(typeof(BlockEntityFarmland))]
    public static class FarmlandPatches {

        [HarmonyPostfix]
        [HarmonyPatch(nameof(BlockEntityFarmland.GetBlockInfo))]
        public static void GetBlockInfoPostfix(ref BlockEntityFarmland __instance, IPlayer forPlayer, StringBuilder dsc) {
            // Make it actually get BlockInfo from each of its behaviors like every other BlockEntity in the game. Why doesn't it already do this? Beats me!
            foreach (BlockEntityBehavior behavior in __instance.Behaviors) {
                behavior.GetBlockInfo(forPlayer, dsc);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BlockEntityFarmland.TryGrowCrop))]
        public static bool TryGrowCropPrefix(ref BlockEntityFarmland __instance, ref bool __result, double currentTotalHours) {
            return FarmlandMethods.TryGrowCropOverrides(ref __instance, ref __result, currentTotalHours);
        }
    }
}

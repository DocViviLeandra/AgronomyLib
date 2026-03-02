using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace AgronomyLib {

    [HarmonyPatch(typeof(ItemScythe))]
    public static class ScythePatches {

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ItemScythe.CanMultiBreak))]
        public static bool CanMultiBreakPrefix(ref bool __result, Block block) {
            if (block.HasBlockBehavior<BlockBehaviorUnscytheable>()) {
                __result = false;
                return false;
            }

            return true;
        }
    }
}

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AgronomyLib {

    [HarmonyPatch(typeof(BlockCrop))]
    public static class CropPatches {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(BlockCrop.GetPlacedBlockInfo))]
        public static string GetPlacedBlockInfoPostfix(string __result, ref BlockCrop __instance, IWorldAccessor world, BlockPos pos, IPlayer forPlayer) {

            // We don't need to get any additional block info if it's on farmland, since farmland is already getting that info.
            if (world.BlockAccessor.GetBlock(pos.DownCopy()) is Block onBlock && onBlock.FirstCodePart().Equals("farmland")) return __result;

            StringBuilder stringBuilder = new StringBuilder($"{__result}\n");
            if (__instance.EntityClass != null) {
                BlockEntity blockEntity = world.BlockAccessor.GetBlockEntity(pos);

                if (blockEntity != null) {
                    try {
                        blockEntity.GetBlockInfo(forPlayer, stringBuilder);
                    } catch (Exception e) {
                        stringBuilder.AppendLine("(error in " + blockEntity.GetType().Name + ")");
                        blockEntity.Api.Logger.Error(e);
                    }
                }
            }

            foreach (BlockBehavior bh in __instance.BlockBehaviors) {
                stringBuilder.AppendLine(bh.GetPlacedBlockInfo(world, pos, forPlayer));
            }

            return stringBuilder.ToString();
        }
    }
}

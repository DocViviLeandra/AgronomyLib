using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace AgronomyLib {

    /// <summary>
    /// This transpiler replaces all accesses to Block.CropProps in the specified methods with calls to BlockExtensions.GetCropProps, allowing crop classes to provide crop properties by method.
    /// </summary>
    [HarmonyPatch]
    public static class TranspilerPatches {
        public static IEnumerable<MethodBase> TargetMethods() {
            return [
                AccessTools.Method(typeof(BlockEntitySoilNutrition), "beginIntervalledUpdate"),

                AccessTools.Method(typeof(BlockEntityFarmland), nameof(BlockEntityFarmland.GetHoursForNextStage)),
                AccessTools.Method(typeof(BlockEntityFarmland), nameof(BlockEntityFarmland.TryPlant)),
                AccessTools.Method(typeof(BlockEntityFarmland), nameof(BlockEntityFarmland.HasRipeCrop)),
                AccessTools.Method(typeof(BlockEntityFarmland), nameof(BlockEntityFarmland.TryGrowCrop)),
                AccessTools.Method(typeof(BlockEntityFarmland), "ConsumeNutrients", [typeof(Block)]),
                AccessTools.Method(typeof(BlockEntityFarmland), nameof(BlockEntityFarmland.GetCrop)),
                AccessTools.Method(typeof(BlockEntityFarmland), nameof(BlockEntityFarmland.GetDrops)),
                AccessTools.Method(typeof(BlockEntityFarmland), nameof(BlockEntityFarmland.GetBlockInfo)),

                AccessTools.Method(typeof(ItemPlantableSeed), nameof(ItemPlantableSeed.GetHeldItemInfo)),
            ];
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            // TODO: Get ILGenerator?
            var codeMatcher = new CodeMatcher(instructions, il);

            codeMatcher.MatchStartForward(
                CodeMatch.LoadsField(AccessTools.Field(typeof(Block), "CropProps"))
                )
                .Repeat(matchAction: cm => {
                    var labels = cm.Instruction.labels;
                    cm.SetInstruction(
                        CodeInstruction.Call(typeof(BlockExtensions), nameof(BlockExtensions.GetCropProps))
                        );
                    cm.AddLabels(labels);
                    cm.Advance();
                });

            return codeMatcher.Instructions();
        }
    }
}

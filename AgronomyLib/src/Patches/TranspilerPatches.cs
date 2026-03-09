using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Vintagestory;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AgronomyLib {

    /// <summary>
    /// This transpiler replaces all accesses to Block.CropProps in the specified methods with calls to BlockExtensions.GetCropProps, allowing crop classes to provide crop properties by method.
    /// </summary>
    [HarmonyPatch]
    public static class CropPropsTranspilerPatch {
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

                //AccessTools.Method(typeof(ItemPlantableSeed), nameof(ItemPlantableSeed.GetHeldItemInfo)),
            ];
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            // TODO: Get ILGenerator?
            var codeMatcher = new CodeMatcher(instructions, il);

            codeMatcher.MatchStartForward(
                CodeMatch.LoadsField(AccessTools.Field(typeof(Block), "CropProps"))
                )
                .Repeat(matchAction: cm => {
                    var labels = cm.Instruction.labels;
                    //cm.SetInstruction(
                    //    CodeInstruction.Call((Block block) => block.GetCropProps())
                    //    ).AddLabels(labels).Advance();

                    
                    cm.RemoveInstruction()
                    .Insert([
                        //CodeInstruction.Call(typeof(BlockEntityFarmland), nameof(BlockEntityFarmland.GetCrop)),
                        CodeInstruction.LoadArgument(0),
                        CodeInstruction.LoadField(typeof(BlockEntity), "Api"),
                        CodeInstruction.LoadArgument(0),
                        CodeInstruction.LoadField(typeof(BlockEntitySoilNutrition), "upPos"),
                        CodeInstruction.Call((Block block, ICoreAPI api, BlockPos pos) => block.GetCropProps(api, pos))
                        ])
                    .AddLabels(labels)
                    .Advance();
                    
                    /*
                    cm.SetInstruction(
                        CodeInstruction.Call(typeof(BlockExtensions), nameof(BlockExtensions.GetCropProps))
                    ).AddLabels(labels).Advance();
                    */
                });

            return codeMatcher.Instructions();
        }
    }
}

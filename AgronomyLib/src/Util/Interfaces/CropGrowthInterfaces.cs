using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AgronomyLib {
    // There is no equivalent interface for <see cref="BlockEntity"/>. If a <see cref="BlockEntity"/> wants to replace TryGrowCrop, give it an associated <see cref="Block"/> that implements this interface, which then calls methods on the <see cref="BlockEntity"/>. See <see cref="BlockCropContainerBase"/> for an example!

    /// <summary>
    /// An interface for a <see cref="Block"/> to REPLACE the <see cref="BlockEntityFarmland.TryGrowCrop"/> method. If a <see cref="Block"/> implements this interface, the default TryGrowCrop behavior will never be applied!
    /// </summary>
    /// <remarks>
    /// Replacing TryGrowCrop logic via this interface will not prevent the BeforeTryGrowCrop() and OnGrowth() methods of behaviors that implement the <see cref="IBehaviorCropGrowth"/> and <see cref="IBlockEntityBehaviorCropGrowth"/> interfaces from executing.
    /// </remarks>
    public interface IBlockCropGrowth {
        /// <summary>
        /// A method which replaces the normal <see cref="BlockEntityFarmland.TryGrowCrop"/> when it is invoked on a <see cref="Block"/> implementing <see cref="IBlockCropGrowth"/>.
        /// </summary>
        /// <remarks>
        /// Has default logic which is identical to base behavior. Call the base version of the method in your implementation if you merely want to add to, not replace, base logic.
        /// </remarks>
        /// <param name="farmland">The <see cref="BlockEntityFarmland"/> that invoked <see cref="BlockEntityFarmland.TryGrowCrop(double)"/></param>
        /// <param name="currentTotalHours"></param>
        /// <returns>Whether the growth of the crop was successful.</returns>
        public virtual bool TryGrowCrop(BlockEntityFarmland farmland, double currentTotalHours) {
            ICoreAPI Api = farmland.Api;

            Block block = farmland.GetCrop();
            if (block == null) return false;

            int currentGrowthStage = block.GetCurrentCropStage();
            BlockCropProperties cropProps = block.GetCropProps(Api, farmland.UpPos);
            if (currentGrowthStage < cropProps.GrowthStages) {
                int newGrowthStage = currentGrowthStage + 1;

                Block? nextBlock = Api.World.GetBlock(block.CodeWithParts($"{newGrowthStage}"));
                if (nextBlock == null) return false;

                if (cropProps.Behaviors != null) {
                    EnumHandling handling = EnumHandling.PassThrough;
                    bool result = false;

                    foreach (CropBehavior behavior in cropProps.Behaviors) {
                        result = behavior.TryGrowCrop(Api, farmland, currentTotalHours, newGrowthStage, ref handling);
                        if (handling == EnumHandling.PreventSubsequent) return result;
                    }

                    if (handling == EnumHandling.PreventDefault) return result;
                }

                if (Api.World.BlockAccessor.GetBlockEntity(farmland.UpPos) == null) {
                    Api.World.BlockAccessor.SetBlock(nextBlock.Id, farmland.UpPos);
                } else {
                    Api.World.BlockAccessor.ExchangeBlock(nextBlock.Id, farmland.UpPos);
                }

                // There's one fewer update than growth stages, since we start on the first one already
                farmland.ConsumeNutrients(cropProps.RequiredNutrient, cropProps.NutrientConsumption / Math.Max(1, cropProps.GrowthStages - 1));
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// An interface for a <see cref="BlockBehavior"/> to affect the <see cref="BlockEntityFarmland.TryGrowCrop(double)"/> method.
    /// </summary>
    public interface IBehaviorCropGrowth {

        /// <summary>
        /// Executes when <see cref="BlockEntityFarmland.TryGrowCrop"/> is called on a crop, before any growth logic is executed. Setting <see cref="EnumHandling.PreventDefault"/> or <see cref="EnumHandling.PreventSubsequent"/> will prevent default growth logic from executing.
        /// </summary>
        /// <param name="farmland">The <see cref="BlockEntityFarmland"/> that invoked <see cref="BlockEntityFarmland.TryGrowCrop(double)"/></param>
        /// <param name="currentTotalHours"></param>
        /// <param name="handling"></param>
        /// <returns>Whether the crop should grow or not, according to this behavior.</returns>
        public abstract bool BeforeTryGrowCrop(BlockEntityFarmland farmland, double currentTotalHours, ref EnumHandling handling);

        /// <summary>
        /// Invoked when <see cref="BlockEntityFarmland.TryGrowCrop"/> is called on a crop, after all growth logic is executed, regardless of the outcome.
        /// </summary>
        /// <param name="farmland">The <see cref="BlockEntityFarmland"/> that invoked <see cref="BlockEntityFarmland.TryGrowCrop(double)"/></param>
        /// <param name="currentTotalHours"></param>
        /// <param name="wasGrown">Whether or not the crop successfully grew</param>
        /// <param name="handling"></param>
        public abstract void OnGrowth(BlockEntityFarmland farmland, double currentTotalHours, bool wasGrown, ref EnumHandling handling);
    }

    /// <summary>
    /// An interface for a <see cref="BlockEntityBehavior"/> to affect the <see cref="BlockEntityFarmland.TryGrowCrop(double)"/> method 
    /// </summary>
    public interface IBlockEntityBehaviorCropGrowth {

        /// <summary>
        /// Executes when <see cref="BlockEntityFarmland.TryGrowCrop"/> is called on a crop, before any growth logic is executed. Setting <see cref="EnumHandling.PreventDefault"/> or <see cref="EnumHandling.PreventSubsequent"/> will prevent default growth logic from executing.
        /// </summary>
        /// <param name="farmland">The <see cref="BlockEntityFarmland"/> that invoked <see cref="BlockEntityFarmland.TryGrowCrop(double)"/></param>
        /// <param name="currentTotalHours"></param>
        /// <param name="handling"></param>
        /// <returns>Whether the crop should grow or not, according to this behavior.</returns>
        public abstract bool BeforeTryGrowCrop(BlockEntityFarmland farmland, double currentTotalHours, ref EnumHandling handling);

        /// <summary>
        /// Invoked when <see cref="BlockEntityFarmland.TryGrowCrop"/> is called on a crop, after all growth logic is executed, regardless of the outcome.
        /// </summary>
        /// <param name="farmland">The <see cref="BlockEntityFarmland"/> that invoked <see cref="BlockEntityFarmland.TryGrowCrop(double)"/></param>
        /// <param name="currentTotalHours"></param>
        /// <param name="handling"></param>
        /// <param name="wasGrown">Whether the crop's growth was successful or not</param>
        public abstract void OnGrowth(BlockEntityFarmland farmland, double currentTotalHours, bool wasGrown, ref EnumHandling handling);
    }

    
}

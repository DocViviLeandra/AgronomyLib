using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AgronomyLib {
    // There is no interface for BlockEntity. If it needs to replace TryGrowCrop, it should do so via the associated Block.

    /// <summary>
    /// An interface for a <see cref="Block"/> to REPLACE the <see cref="BlockEntityFarmland.TryGrowCrop"/> method. If a <see cref="Block"/> implements this interface, the default TryGrowCrop behavior will never be applied!
    /// </summary>
    public interface IBlockCropGrowth {
        /// <summary>
        /// A method which replaces the normal <see cref="BlockEntityFarmland.TryGrowCrop"/> when it is invoked on a <see cref="Block"/> implementing <see cref="IBlockCropGrowth"/>.
        /// </summary>
        /// <param name="farmland">The <see cref="BlockEntityFarmland"/> that invoked <see cref="BlockEntityFarmland.TryGrowCrop(double)"/></param>
        /// <param name="currentTotalHours"></param>
        /// <returns>Whether the growth of the crop was successful.</returns>
        public abstract bool TryGrowCrop(BlockEntityFarmland farmland, double currentTotalHours);
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

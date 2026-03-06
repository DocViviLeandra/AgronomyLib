using AgronomyLib;
using HarmonyLib;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace AgronomyLib
{
    public class AgronomyLibModSystem : ModSystem
    {
        #region globals
        public static readonly string classPrefix = "AgronomyLib";
        public static readonly string attributePrefix = "agronomylib";
        #endregion

        private static Harmony? harmony;

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            // Mod.Logger.Notification("Hello from template mod: " + api.Side);

            RegisterBlocks(api);
            RegisterBlockEntities(api);
            RegisterBlockBehaviors(api);
            RegisterBlockEntityBehaviors(api);

            if (harmony == null) {
                harmony = new Harmony(Mod.Info.ModID);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            // Mod.Logger.Notification("Hello from template mod server side: " + Lang.Get("agronomylib:hello"));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            // Mod.Logger.Notification("Hello from template mod client side: " + Lang.Get("agronomylib:hello"));
        }

        public override void Dispose() {
            base.Dispose();

            harmony?.UnpatchAll(Mod.Info.ModID);
        }

        private void RegisterBlocks(ICoreAPI api) {

        }

        private void RegisterBlockEntities(ICoreAPI api) {

        }

        private void RegisterBlockBehaviors(ICoreAPI api) {
            api.RegisterBlockBehaviorClass($"{classPrefix}.{BlockBehaviorHasRoots.className}", typeof(BlockBehaviorHasRoots));
            api.RegisterBlockBehaviorClass($"{classPrefix}.{BlockBehaviorRegrowsFromRoots.className}", typeof(BlockBehaviorRegrowsFromRoots));
            api.RegisterBlockBehaviorClass($"{classPrefix}.{BlockBehaviorUnscytheable.className}", typeof(BlockBehaviorUnscytheable));
        }

        private void RegisterBlockEntityBehaviors(ICoreAPI api) {
            api.RegisterBlockEntityBehaviorClass($"{classPrefix}.{BlockEntityBehaviorRequiresVernalization.className}", typeof(BlockEntityBehaviorRequiresVernalization));
            api.RegisterBlockEntityBehaviorClass($"{classPrefix}.{BlockEntityBehaviorAgronomyFarmland.className}", typeof(BlockEntityBehaviorAgronomyFarmland));
        }
    }
}

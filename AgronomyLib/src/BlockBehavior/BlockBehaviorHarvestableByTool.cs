using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace AgronomyLib {
    public class HarvestProperties {
        /// <summary>
        /// The amount of time, in seconds, it takes to harvest this block.
        /// </summary>
        [DocumentAsJson("Recommended", "0")]
        public float harvestTime;

        /// <summary>
        /// Should this block be exchanged (true) or replaced (false)? If true, then any block entity at the same position will not be deleted.
        /// </summary>
        [DocumentAsJson("Optional", "False")]
        public bool exchangeBlock;

        /// <summary>
        /// An array of drops for when the block is harvested. If only using a single drop you can use <see cref="harvestedStack"/>, otherwise this property is required.
        /// </summary>
        [DocumentAsJson("Required")]
        public BlockDropItemStack[]? harvestedStacks;

        /// <summary>
        /// A drop for when the block is harvested. If using more than a single drop, use <see cref="harvestedStacks"/>, otherwise this property is required.
        /// </summary>
        [DocumentAsJson("Obsolete")]
        public BlockDropItemStack? harvestedStack { get { return harvestedStacks?[0]; } set { if (harvestedStacks != null && value != null) harvestedStacks[0] = value; } }

        /// <summary>
        /// The sound to play whilst the object is being harvested.
        /// </summary>
        [DocumentAsJson("Optional", "sounds/block/leafy-picking")]
        public AssetLocation? harvestingSound;

        /// <summary>
        /// The block to replace this one after it is harvested.
        /// </summary>
        [DocumentAsJson("Optional", "None")]
        public AssetLocation? harvestedBlockCode;

        public EnumTool? Tool;

        public Block? harvestedBlock;

        /// <summary>
        /// The code to use for the interaction help of this block.
        /// </summary>
        [DocumentAsJson("Optional", "blockhelp-harvetable-harvest")]
        public string interactionHelpCode = "blockhelp-harvetable-harvest";
    }

    // TODO: Reminder to actually test this!
    /// <summary>
    /// Similar to vanilla <see cref="BlockBehaviorHarvestable"/>, but allows the specification of multiple methods of harvesting a block as long as they require different tools.
    /// Uses code "AgronomyLib.HarvestableByTool"
    /// </summary>
    /// <example>
    /// <code lang="json">
    /// "behaviors": [
    ///     {
    ///         "name": "AgronomyLib.HarvestableByTool",
    ///         "properties": {
    ///             "harvestPropsByTool": {
    ///                 "Knife": {
    ///                     "harvestTime": 0.6,
	///				        "harvestedStack": {
	///				        	"type": "item",
	///				        	"code": "fruit-{type}",
	///				        	"quantity": { "avg": 4.4 }
	///				        },
	///				        "harvestedBlockCode": "bigberrybush-{type}-empty",
	///				        "exchangeBlock": true
    ///                 }
    ///             }
    ///         }
    ///     }
    /// ]
    /// </code>
    /// </example>
    public class BlockBehaviorHarvestableByTool : BlockBehavior {
        #region keys
        public static readonly string className = "HarvestableByTool";
        #endregion

        public HarvestProperties? defaultHarvestProps;
        public Dictionary<EnumTool, HarvestProperties> harvestPropsByTool = new Dictionary<EnumTool, HarvestProperties>();

        public BlockBehaviorHarvestableByTool(Block block) : base(block) { }

        public override void Initialize(JsonObject properties) {
            base.Initialize(properties);

            JsonObject defaultProps = properties["harvestPropsDefault"];
            if (defaultProps != null) {
                defaultHarvestProps = RegisterHarvestProperties(null, defaultProps);
            }

            Dictionary<EnumTool, JsonObject> dict = properties["harvestPropsByTool"].AsObject <Dictionary<EnumTool, JsonObject>>();
            foreach (EnumTool tool in dict.Keys) {
                harvestPropsByTool[tool] = RegisterHarvestProperties(tool, dict[tool]);
            }
        }

        protected HarvestProperties RegisterHarvestProperties(EnumTool? tool, JsonObject harvestProps) {
            HarvestProperties harvestProperties = new HarvestProperties {
                interactionHelpCode = harvestProps["interactionHelpCode"].AsString("blockhelp-harvetable-harvest"),
                harvestTime = harvestProps["harvestTime"].AsFloat(0),
                Tool = tool,
                harvestedStacks = harvestProps["harvestedStacks"].AsObject<BlockDropItemStack[]>(null),
                exchangeBlock = harvestProps["exchangeBlock"].AsBool(false),
            };

            BlockDropItemStack? tempStack = harvestProps["harvestedStack"].AsObject<BlockDropItemStack?>(null);
            if (harvestProperties.harvestedStacks == null && tempStack != null) {
                harvestProperties.harvestedStacks = [tempStack];
            }

            string? code = harvestProps["harvestingSound"].AsString("game:sounds/block/leafy-picking");
            if (code != null) {
                harvestProperties.harvestingSound = AssetLocation.Create(code);
            }

            code = harvestProps["harvestedBlockCode"].AsString();
            if (code != null) {
                harvestProperties.harvestedBlockCode = AssetLocation.Create(code, block.Code.Domain);
            }

            return harvestProperties;
        }

        public override void OnLoaded(ICoreAPI api) {
            base.OnLoaded(api);

            foreach(HarvestProperties props in harvestPropsByTool.Values) {
                props.harvestedStacks.Foreach(harvestedStack => harvestedStack?.Resolve(api.World, "harvestedStack of block", block.Code));

                props.harvestedBlock = api.World.BlockAccessor.GetBlock(props.harvestedBlockCode);
                if (props.harvestedBlock == null) {
                    api.World.Logger.Warning("Unable to resolve harvested block code '{0}' for block {1}. Will ignore.", props.harvestedBlockCode, block.Code);
                }
            }
        }

        /// <summary>
        /// Dictionary of players currently harvesting blocks with this behavior, and which tool they're using. Used to prevent switching harvest type partway. Players should not be registered in this unless they are actually, right now, harvesting a block.
        /// </summary>
        protected Dictionary<string, EnumTool?> harvestToolsByPlayer = new Dictionary<string, EnumTool?>();

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling) {
            HarvestProperties? activeHarvest = null;
            EnumTool? activeTool = byPlayer.InventoryManager.ActiveTool;
            EnumTool? harvestTool = null;
            if (activeTool != null) {
                if (harvestPropsByTool.TryGetValue(activeTool.Value, out activeHarvest)) {
                    harvestTool = activeTool;
                }
            }

            if (activeHarvest == null) {
                activeHarvest = defaultHarvestProps;
            }

            if (activeHarvest == null) {
                return false;
            }

            if (!world.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.Use)) {
                return false;
            }

            handling = EnumHandling.PreventDefault;

            if (activeHarvest.harvestedStacks != null) {
                world.PlaySoundAt(activeHarvest.harvestingSound, blockSel.Position, 0, byPlayer);

                harvestToolsByPlayer[byPlayer.PlayerUID] = harvestTool;

                return true;
            }

            return false;
        }

        public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling) {
            EnumTool? harvestTool = harvestToolsByPlayer[byPlayer.PlayerUID];
            if (harvestTool != null && byPlayer.InventoryManager.ActiveTool != harvestTool) return false;

            HarvestProperties? activeHarvest;
            if (harvestTool != null) {
                if (!harvestPropsByTool.TryGetValue(harvestTool.Value, out activeHarvest)) {
                    return false;  
                }
            } else {
                activeHarvest = defaultHarvestProps;
            }

            if (activeHarvest == null) return false;

            if (blockSel == null) return false;

            handling = EnumHandling.PreventDefault;

            (byPlayer as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemAttack);

            if (world.Rand.NextDouble() < 0.05) {
                world.PlaySoundAt(activeHarvest.harvestingSound, blockSel.Position, 0, byPlayer);
            }

            if (world.Side == EnumAppSide.Client && world.Rand.NextDouble() < 0.25 && activeHarvest.harvestedStacks?[0]?.ResolvedItemstack != null) {
                world.SpawnCubeParticles(blockSel.Position.ToVec3d().Add(blockSel.HitPosition), activeHarvest.harvestedStacks[0].ResolvedItemstack, 0.25f, 1, 0.5f, byPlayer, new Vintagestory.API.MathTools.Vec3f(0, 1, 0));
            }

            return world.Side == EnumAppSide.Client || secondsUsed < activeHarvest.harvestTime;
        }

        public override void OnBlockInteractStop(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling) {
            EnumTool? harvestTool = harvestToolsByPlayer[byPlayer.PlayerUID];
            if (harvestTool != null && byPlayer.InventoryManager.ActiveTool != harvestTool) {
                harvestToolsByPlayer.Remove(byPlayer.PlayerUID);
                return;
            }

            HarvestProperties? activeHarvest;
            if (harvestTool != null) {
                if (!harvestPropsByTool.TryGetValue(harvestTool.Value, out activeHarvest)) {
                    harvestToolsByPlayer.Remove(byPlayer.PlayerUID);
                    return;
                }
            } else {
                activeHarvest = defaultHarvestProps;
            }

            if (activeHarvest == null) {
                harvestToolsByPlayer.Remove(byPlayer.PlayerUID);
                return;
            }

            handling = EnumHandling.PreventDefault;

            if (secondsUsed > activeHarvest.harvestTime - 0.05f && activeHarvest.harvestedStacks != null && world.Side == EnumAppSide.Server) {
                float dropRate = 1;

                if (block.Attributes?.IsTrue("forageStatAffected") == true) {
                    dropRate *= byPlayer.Entity.Stats.GetBlended("forageDropRate");
                }

                activeHarvest.harvestedStacks.Foreach(harvestedStack => {
                    ItemStack? stack = harvestedStack.GetNextItemStack(dropRate);
                    if (stack == null) {
                        harvestToolsByPlayer.Remove(byPlayer.PlayerUID);
                        return;
                    }

                    var origStack = stack.Clone();
                    var quantity = stack.StackSize;
                    if (!byPlayer.InventoryManager.TryGiveItemstack(stack)) {
                        world.SpawnItemEntity(stack, blockSel.Position);
                    }

                    world.Logger.Audit("{0} Took {1}x{2} from {3} at {4}.",
                        byPlayer.PlayerName,
                        quantity,
                        stack.Collectible.Code,
                        block.Code,
                        blockSel.Position
                    );

                    TreeAttribute tree = new TreeAttribute();
                    tree["itemstack"] = new ItemstackAttribute(origStack.Clone());
                    tree["byentityid"] = new LongAttribute(byPlayer.Entity.EntityId);
                    world.Api.Event.PushEvent("onitemcollected", tree);
                });

                if (activeHarvest.harvestedBlock != null) {
                    if (!activeHarvest.exchangeBlock) world.BlockAccessor.SetBlock(activeHarvest.harvestedBlock.BlockId, blockSel.Position);
                    else world.BlockAccessor.ExchangeBlock(activeHarvest.harvestedBlock.BlockId, blockSel.Position);
                }

                if (activeHarvest.Tool != null) {
                    var toolSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
                    toolSlot.Itemstack?.Collectible.DamageItem(world, byPlayer.Entity, toolSlot);
                }

                world.PlaySoundAt(activeHarvest.harvestingSound, blockSel.Position, 0, byPlayer);

                harvestToolsByPlayer.Remove(byPlayer.PlayerUID);
            }
        }

        public override bool OnBlockInteractCancel(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling) {
            harvestToolsByPlayer.Remove(byPlayer.PlayerUID);
            return base.OnBlockInteractCancel(secondsUsed, world, byPlayer, blockSel, ref handling);
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer, ref EnumHandling handling) {
            bool notProtected = true;
            if (world.Claims != null && world is IClientWorldAccessor clientWorld && clientWorld.Player?.WorldData.CurrentGameMode == EnumGameMode.Survival) {
                EnumWorldAccessResponse resp = world.Claims.TestAccess(clientWorld.Player, selection.Position, EnumBlockAccessFlags.Use);
                if (resp != EnumWorldAccessResponse.Granted) notProtected = false;
            }

            if (notProtected) {
                List<WorldInteraction> worldInteractions = new List<WorldInteraction>();
                foreach (EnumTool tool in harvestPropsByTool.Keys) {
                    HarvestProperties harvestProps = harvestPropsByTool[tool];
                    worldInteractions.Add(generateWorldInteractionForProps(world, harvestProps));
                }

                if (defaultHarvestProps != null) {
                    worldInteractions.Add(generateWorldInteractionForProps(world, defaultHarvestProps));
                }

                return worldInteractions.ToArray();
            }

            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer, ref handling);
        }

        protected WorldInteraction generateWorldInteractionForProps(IWorldAccessor world, HarvestProperties harvestProps) {
            return new WorldInteraction() {
                ActionLangCode = harvestProps.interactionHelpCode,
                MouseButton = EnumMouseButton.Right,
                Itemstacks = harvestProps.Tool == null ? null : ObjectCacheUtil.GetToolStacks(world.Api, (EnumTool)harvestProps.Tool)
            };
        }
    }
}

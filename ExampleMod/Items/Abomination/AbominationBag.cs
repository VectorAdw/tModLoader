using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Items.Abomination
{
	public class AbominationBag : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Treasure Bag");
			Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}");
		}

		public override void SetDefaults() {
			item.maxStack = 999;
			item.consumable = true;
			item.width = 24;
			item.height = 24;
			item.rare = 9;
			item.expert = true;
		}

		public override bool CanRightClick() {
			return true;
		}

		public override void OpenBossBag(Player player) {
			player.TryGettingDevArmor();
			if (Main.rand.NextBool(7)) {
				player.QuickSpawnItem(mod.ItemType("AbominationMask"));
			}
			player.QuickSpawnItem(mod.ItemType("MoltenDrill"));
			player.QuickSpawnItem(mod.ItemType("ElementResidue"));
			player.QuickSpawnItem(mod.ItemType("PurityTotem"));
			player.QuickSpawnItem(mod.ItemType("SixColorShield"));
		}

		public override int BossBagNPC => mod.NPCType("Abomination");
	}
}
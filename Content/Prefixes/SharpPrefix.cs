using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Brigade.Content.Prefixes {
    internal class SharpPrefix : ModPrefix {

        // This allows prefixes derived from this one to override this virtual Power attribute.
        public virtual float Power => 1f;

        // The category in which this Prefix or Modifier can be rolled on.
        public override PrefixCategory Category => PrefixCategory.AnyWeapon;


        // Providing a value of 1f means that this prefix has the same chance to roll as every base prefix in Terraria.
        // Providing a value of 2f menas it has twice as likely to be rolled compared to every other prefix.
        // Providing a value of 0.5f means it has half as likely to be rolled compared to every other prefix.
        public override float RollChance(Item item) {
            return 1f;
        }

        public override bool CanRoll(Item item) {
            return true;
        }

        // Use to modify the stats for items which have this prefix.
        public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
            damageMult *= 1f + 0.20f * Power;
        }

        // This will change the cost of the item with this modifier.
        public override void ModifyValue(ref float valueMult) {
            valueMult *= 1f + 0.05f * Power;
        }

        // This can be used to modify most other stats of the item.
        public override void Apply(Item item) {
            
        }

        // If a prefix doesn't have any non-standard stats, the additional tooltip lines aren't actually necessary.
        // If it does, it can follow this general outline.
        public override IEnumerable<TooltipLine> GetTooltipLines(Item item) {
            // Due to inheritence for the Derived Prefix, we add 2 different tooltip lines.
            // The first tooltip lines serves as the typical stat boost
            // While the second tooltip serves as additional flavor text.

            // Uses a special format that automatically adds + or - to the value.
            // This shared localization is formatted with the power value from the PowerTooltip, resulting in different text for the main and inherited class.
            yield return new TooltipLine(Mod, "SharpWeaponPrefix", PowerTooltip.Format(Power));

            // This localization is not shared between inherited classes. If there was a derived SharpPrefix, SharpPrefixDerived would have its own translation for this line.
            yield return new TooltipLine(Mod, "SharpWeaponPrefixDescription", AdditionalTooltip.Value) {
                IsModifier = true,
            };

            // If possible and suitable, try to reuse the name identifier and translation value of Terraria Prefixes.
            // This code uses the vanilla translation for the word defense, resulting in "-5 defense".
            // IsModifierBad is used for this bad modifier.
            /* yield return new TooltipLine(Mod, "PrefixAccDefense", "-5" + Lang.tip[25].Value) {
                IsModifier = true,
                IsModifierBad = true,
            }; */
        }

        // PowerTooltip is shared between itself and its derived prefix class.
        public static LocalizedText PowerTooltip { get; private set; }

        // AdditionalTooltip shows off how to do inheritable localized properties approach.
        // It is necessary if we use inheritance, and we want different translations for different classes.
        // More Information: https://github.com/tModLoader/tModLoader/wiki/Localization#inheritable-localized-properties
        public LocalizedText AdditionalTooltip => this.GetLocalization(nameof(AdditionalTooltip));

        public override void SetStaticDefaults() {
            // we dont use this.GetLocalization because we want to use a shared key.
            PowerTooltip = Mod.GetLocalization($"{LocalizationCategory}.{nameof(PowerTooltip)}");

            // This is required to properly register the key for the AdditionalTooltip.
            _ = AdditionalTooltip;
        }
    }
}

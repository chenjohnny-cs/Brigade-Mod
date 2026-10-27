using System.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Brigade.Core.System {

    // This class acts as a container for "Downed Boss" flags.
    // Set a flag like this on your bosses OnKill:
    //  NPC.SetEventFlagCleared(ref DownedBossSystem.downedMinionBoss, -1);

    // Saving and loading these flags require TagCompounds.
    // https://github.com/tModLoader/tModLoader/wiki/Saving-and-loading-using-TagCompound for more info.
    internal class DownedBossSystem : ModSystem {

        public static bool downedGeldBoss = false;

        public override void ClearWorld() {
            downedGeldBoss = false;
        }

        // We save our own data sets using TagCompounds.
        // The tag instance provided by the function here is always empty by default.
        public override void SaveWorldData(TagCompound tag) {
            if (downedGeldBoss) {
                tag["downedGeldBoss"] = true;
            }
        }

        public override void LoadWorldData(TagCompound tag) {
            downedGeldBoss = tag.ContainsKey("downedGeldBoss");
        }

        public override void NetSend(BinaryWriter writer) {

            // The order of parameters is important and must match the parameters in NetReceive.
            // WriteFlags supports up to 8 entires, if there is more write a new WriteFlags .
            // If a large number of flags need to be sent (such as a flag per item) using BitArray can be useful (see Util.BitArray documentation).
            writer.WriteFlags(downedGeldBoss);
        }

        public override void NetReceive(BinaryReader reader) {
            // ReadFlags also supports up to 8 entries, if there ismore than 8 flags to sync, write a new ReadFlags.
            reader.ReadFlags(out downedGeldBoss);
        }

    }
}

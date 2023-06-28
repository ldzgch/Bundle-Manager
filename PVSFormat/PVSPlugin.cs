using BundleFormat;
using PluginAPI;

namespace PVSFormat
{
    public class PVSPlugin : Plugin
    {
        public override void Init()
        {
            EntryTypeRegistry.Register(EntryTypeBP.ZoneList, new PVS());
        }

        public override string GetID()
        {
            return "pvsplugin";
        }

        public override string GetName()
        {
            return "PVS Resource Handler";
        }
    }
}

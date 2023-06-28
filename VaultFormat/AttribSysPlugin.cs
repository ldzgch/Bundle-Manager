using BundleFormat;
using PluginAPI;

namespace VaultFormat
{
    public class AttribSysPlugin : Plugin
    {
        public override void Init()
        {
            EntryTypeRegistry.Register(EntryTypeBP.AttribSysVault, new AttribSys());
        }

        public override string GetID()
        {
            return "attribsysplugin";
        }

        public override string GetName()
        {
            return "AttribSys Resource Handler";
        }
    }
}

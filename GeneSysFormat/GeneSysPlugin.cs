using PluginAPI;
using BundleFormat;
using GeneSys;

namespace GeneSysFormat
{
    internal class GeneSysPlugin : Plugin
    {
        public override void Init()
        {
            EntryTypeRegistry.Register(EntryTypeNFS.GeneSysDefinition, new GeneSysDefinition());
        }

        public override string GetID()
        {
            return "genesysplugin";
        }

        public override string GetName()
        {
            return "GeneSysDefinition/GeneSysInstance Format Handler";
        }
    }
}

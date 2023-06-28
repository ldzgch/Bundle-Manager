using BundleFormat;
using PluginAPI;

namespace LuaList
{

    public class LuaListPlugin : Plugin
    {
        public override void Init()
        {
            EntryTypeRegistry.Register(EntryTypeBP.LUAList, new LuaList());
        }

        public override string GetID()
        {
            return "lualistplugin";
        }

        public override string GetName()
        {
            return "Lua List Resource Handler";
        }
        
    }
}

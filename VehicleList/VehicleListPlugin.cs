using BundleFormat;
using PluginAPI;

namespace VehicleList
{
    public class VehicleListPlugin : Plugin
    {
        public override void Init()
        {
            EntryTypeRegistry.Register(EntryTypeBP.VehicleList, new VehicleListData());
        }

        public override string GetID()
        {
            return "vehiclelistplugin";
        }

        public override string GetName()
        {
            return "VehicleList Resource Handler";
        }
    }
}

using BundleFormat;
using PluginAPI;

namespace BaseHandlers
{
    public class BasePlugin : Plugin
    {
        public override void Init()
        {
            EntryTypeRegistry.Register(EntryTypeBP.TriggerData, new TriggerData());
            EntryTypeRegistry.Register(EntryTypeBP.StreetData, new StreetData());
            EntryTypeRegistry.Register(EntryTypeBP.ProgressionData, new ProgressionData());
            EntryTypeRegistry.Register(EntryTypeBP.EntryList, new IDList());
            EntryTypeRegistry.Register(EntryTypeBP.TrafficData, new Traffic());
            EntryTypeRegistry.Register(EntryTypeBP.FlaptFile, new FlaptFile());
            //EntryTypeRegistry.Register(EntryType.AptDataHeaderType, new AptData());
            EntryTypeRegistry.Register(EntryTypeBP.InstanceList, new InstanceList());
            EntryTypeRegistry.Register(EntryTypeBP.GraphicsSpec, new GraphicsSpec());
            EntryTypeRegistry.Register(EntryTypeBP.Renderable, new Renderable());
        }

        public override string GetID()
        {
            return "baseplugin";
        }

        public override string GetName()
        {
            return "Base Resource Handlers";
        }
    }
}

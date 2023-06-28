using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using BundleUtilities;

namespace BundleFormat
{
    using RVer = EntryType.ResourceVer;
    public class EntryBlock
    {
        public bool Compressed;
        public uint CompressedSize;
        public uint UncompressedSize;
        public uint UncompressedAlignment; // default depending on file type
        public byte[] RawData;
        public byte[] Data
        {
            get
            {
                if (RawData == null)
                    return null;

                if (Compressed)
                    return RawData.Decompress((int)UncompressedSize);

                return RawData;
            }
            set
            {
                if (Compressed)
                    RawData = value.Compress();
                else
                    RawData = value;

                UncompressedSize = (uint)value.Length;
                CompressedSize = (uint)RawData.Length;
            }
        }
    }

    public class EntryInfo
    {
        public uint ID;
        public EntryType Type;
        public string Path;
        public DebugInfo DebugInfo;

        public EntryInfo(uint id, EntryType type, string path, DebugInfo debugInfo)
        {
            ID = id;
            Type = type;
            Path = path;
            DebugInfo = debugInfo;
        }
    }

    public struct Dependency
    {
        public ulong ID;
        public uint EntryPointerOffset;
    }

    public struct DebugInfo
    {
        public string Name;
        public string TypeName;
    }


    public class BundleEntry
    {
        public BundleArchive Archive;
        //public BundleReference Archive;

        public int Index;

        public ulong ID;
        public ulong References;
        public int DependenciesListOffset;
        public short DependencyCount;
        public List<Dependency> Dependencies;

        public DebugInfo DebugInfo;

        public EntryBlock[] EntryBlocks;

        public bool HasHeader => HasSection(0);
        public bool HasBody => HasSection(1);
        public bool HasThird => HasSection(2);

        public EntryType Type;

        public BundlePlatform Platform;
        public bool Console => Platform == BundlePlatform.X360 || Platform == BundlePlatform.PS3;

        public bool Dirty;

        public BundleEntry(BundleArchive archive)
        {
            Archive = archive;
            /*Archive = new BundleReference();
            Archive.Path = archive.Path;
            Archive.EntryCount = (uint)archive.Entries.Count;*/
            Dependencies = new List<Dependency>();
        }

        // public abstract void setType(EntryType type);
        // public abstract EntryType getType();


        public bool HasSection(int section)
        {
            return EntryBlocks != null &&
                   section < EntryBlocks.Length &&
                   section >= 0 &&
                   EntryBlocks[section] != null &&
                   EntryBlocks[section].Data != null &&
                   EntryBlocks[section].Data.Length > 0;
        }

        public MemoryStream MakeStream(bool body = false)
        {
            if (EntryBlocks == null)
                return null;

            if (body)
                return new MemoryStream(EntryBlocks[1].Data);
            return new MemoryStream(EntryBlocks[0].Data);
        }

        public List<BundleDependency> GetDependencies()
        {
            List<BundleDependency> result = new List<BundleDependency>();

            if (Dependencies.Count > 0)
            {
                for (int i = 0; i < Dependencies.Count; i++)
                {
                    BundleDependency dependency = new BundleDependency();

                    dependency.EntryID = Dependencies[i].ID;
                    dependency.EntryPointerOffset = (int)Dependencies[i].EntryPointerOffset;

                    BundleEntry entry = null;

                    /*string file = BundleCache.GetFileByEntryID(dependency.EntryID);
                    if (!string.IsNullOrEmpty(file))
                    {
                        BundleArchive archive = BundleArchive.Read(file, dependency.EntryID);
                        entry = archive.GetEntryByID(dependency.EntryID);
                    }*/
                    //}

                    // TODO
                    for (int j = 0; j < Archive.Entries.Count; j++)
                    {
                        if (Archive.Entries[j].ID != dependency.EntryID)
                            continue;

                        dependency.EntryIndex = j;
                        entry = Archive.Entries[j];
                    }

                    dependency.Entry = entry;

                    result.Add(dependency);
                }
                return result;
            }

            MemoryStream ms = MakeStream();
            BinaryReader2 br = new BinaryReader2(ms);
            br.BigEndian = Console;

            br.BaseStream.Position = DependenciesListOffset;

            for (int i = 0; i < DependencyCount; i++)
            {
                BundleDependency bundleDependency = new BundleDependency();

                bundleDependency.EntryID = br.ReadUInt64();
                bundleDependency.EntryPointerOffset = br.ReadInt32();
                bundleDependency.Unknown2 = br.ReadInt32();

                BundleEntry entry = null;

                /*string file = BundleCache.GetFileByEntryID(bundleDependency.EntryID);
                if (!string.IsNullOrEmpty(file))
                {
                    BundleArchive archive = BundleArchive.Read(file, bundleDependency.EntryID);
                    entry = archive.GetEntryByID(bundleDependency.EntryID);
                }*/

                // TODO
                for (int j = 0; j < Archive.Entries.Count; j++)
                {
                    if (Archive.Entries[j].ID != bundleDependency.EntryID)
                        continue;

                    bundleDependency.EntryIndex = j;
                    entry = Archive.Entries[j];
                }

                bundleDependency.Entry = entry;

                result.Add(bundleDependency);
            }

            br.Close();
            ms.Close();

            return result;
        }

        public string DetectName()
        {
            if (!string.IsNullOrWhiteSpace(DebugInfo.Name))
                return DebugInfo.Name;

            string theName = "worldvault";
            ulong theID = Crc32.HashCrc32B(theName);
            if (theID == ID)
                return theName;
            theName = "postfxvault";
            theID = Crc32.HashCrc32B(theName);
            if (theID == ID)
                return theName;
            theName = "cameravault";
            theID = Crc32.HashCrc32B(theName);
            if (theID == ID)
                return theName;

            string path = Path.GetFileNameWithoutExtension(Archive.Path);
            string file = null;
            if (path != null)
                file = path.ToUpper();

            if (file != null && file.StartsWith("TRK_UNIT") && file.EndsWith("_GR"))
            {
                string trackID = file.Substring(8).Replace("_GR", "").ToLower();
                string name = "trk_unit" + trackID + "_list";
                ulong newID = Crc32.HashCrc32B(name);
                if (newID == ID)
                    return name;
                name = "prp_inst_" + trackID;
                newID = Crc32.HashCrc32B(name);
                if (newID == ID)
                    return name;
                name = "prp_gl__" + trackID;
                newID = Crc32.HashCrc32B(name);
                if (newID == ID)
                    return name;
                name = "trk_unit" + trackID + "_passby";
                newID = Crc32.HashCrc32B(name);
                if (newID == ID)
                    return name;
                name = "trk_unit" + trackID + "_emitter";
                newID = Crc32.HashCrc32B(name);
                if (newID == ID)
                    return name;
            }

            if (file != null)
            {
                string aptName = file.ToLower() + ".swf";
                ulong aptID = Crc32.HashCrc32B(aptName);
                if (aptID == ID)
                    return aptName;
            }

            if (file != null && file.StartsWith("WHE_") && file.EndsWith("_GR"))
            {
                string wheelID = file.Substring(4).Replace("_GR", "").ToLower();
                string name = wheelID + "_graphics";
                ulong newID = Crc32.HashCrc32B(name);
                if (newID == ID)
                    return name;
            }

            if (file != null && file.StartsWith("VEH_"))
            {
                if (file.EndsWith("_AT"))
                {
                    string vehicleID = file.Substring(4).Replace("_AT", "").ToLower();
                    string name = vehicleID + "_attribsys";
                    ulong newID = Crc32.HashCrc32B(name);
                    if (newID == ID)
                        return name;
                    name = vehicleID + "deformationmodel";
                    newID = Crc32.HashCrc32B(name);
                    if (newID == ID)
                        return name;
                    name = vehicleID + "_bpr";
                    newID = Crc32.HashCrc32B(name);
                    if (newID == ID)
                        return name;
                    name = vehicleID + "_anim";
                    newID = Crc32.HashCrc32B(name);
                    if (newID == ID)
                        return name;
                    name = vehicleID + "_trafficstub";
                    newID = Crc32.HashCrc32B(name);
                    if (newID == ID)
                        return name;
                    name = vehicleID + "_vanm";
                    newID = Crc32.HashCrc32B(name);
                    if (newID == ID)
                        return name;
                }
                else if (file.EndsWith("_CD"))
                {
                    string vehicleID = file.Substring(4).Replace("_CD", "").ToLower();
                    string name = vehicleID;
                    ulong newID = Crc32.HashCrc32B(name);
                    if (newID == ID)
                        return name;
                }
                else if (file.EndsWith("_GR"))
                {
                    string vehicleID = file.Substring(4).Replace("_GR", "").ToLower();
                    string name = vehicleID + "_graphics";
                    ulong newID = Crc32.HashCrc32B(name);
                    if (newID == ID)
                        return name;
                }
            }

            // WorldCol Names
            for (int i = 0; i < Archive.EntryCount; i++)
            {
                string name = "trk_col_" + i;
                ulong newID = Crc32.HashCrc32B(name);
                if (newID == ID)
                    return name;
                name = "trk_clil" + i;
                newID = Crc32.HashCrc32B(name);
                if (newID == ID)
                    return name;
            }

            return "";
        }

        public Color GetColor()
        {
            if (Type.Ver == RVer.BP)
            {
                switch ((EntryTypeBP)(int)Type)
                {
                    case EntryTypeBP.Texture:
                        return Color.Orange;
                    case EntryTypeBP.Material:  
                        return Color.HotPink;
                    case EntryTypeBP.Renderable:
                        return Color.Aquamarine;
                    case EntryTypeBP.InstanceList:
                        return Color.BlueViolet;
                    case EntryTypeBP.EntryList:
                        return Color.Tomato;
                    case EntryTypeBP.Model:
                        return Color.Yellow;
                    case EntryTypeBP.PolygonSoupList:
                        return Color.Goldenrod;
                    case EntryTypeBP.GraphicsSpec:
                        return Color.SeaGreen;
                    default:
                        break;
                }
            } else
            {
                switch((EntryTypeNFS)(int)Type)
                {
                    case EntryTypeNFS.Texture: return Color.Orange;
                    default: break;
                }
            }
            return Color.Transparent;
        }

        public bool IsType(EntryTypeBP t)
        {
            return (Type.Ver == RVer.BP && (int)Type == (int)t);
        }
        public bool IsType(EntryTypeNFS t)
        {
            return (Type.Ver == RVer.NFS && (int)Type == (int)t);
        }

    }
}

    /*

    public class EntryNFS : BundleEntry
    {
        public EntryTypeNFS TypeNFS;
        public EntryNFS(BundleArchive a) :
            base(a) {}
        public override Color GetColor()
        {
                switch (TypeNFS)
                {
                    case EntryTypeNFS.Texture:
                        return Color.Orange;
                    case EntryTypeNFS.Material:
                        return Color.HotPink;
                    case EntryTypeNFS.Renderable:
                        return Color.Aquamarine;
                    case EntryTypeNFS.InstanceList:
                        return Color.BlueViolet;
                    case EntryTypeNFS.EntryList:
                        return Color.Tomato;
                    case EntryTypeNFS.Model:
                        return Color.Yellow;
                    case EntryTypeNFS.PolygonSoupList:
                        return Color.Goldenrod;
                    case EntryTypeNFS.VehicleGraphicsSpec:
                        return Color.SeaGreen;
                    default:
                        break;
                }
            return Color.Transparent;
        }
        public override void setType(int type)
        {
            TypeNFS = (EntryTypeNFS)type;
        }

        public override int getType()
        {
            return (int)TypeNFS;
        }
    }


    public class EntryBP : BundleEntry
    {
        public EntryTypeBP TypeBP;
        public EntryBP(BundleArchive a) : base(a) { }
        public override void setType(int type)
        {
            TypeBP = (EntryTypeBP)type;
        }

        public override int getType()
        {
            return (int)TypeBP;
        }
        public override Color GetColor()
        {
            switch (TypeBP)
            {
                case EntryTypeBP.Texture:
                    return Color.Orange;
                case EntryTypeBP.Material:
                    return Color.HotPink;
                case EntryTypeBP.Renderable:
                    return Color.Aquamarine;
                case EntryTypeBP.InstanceList:
                    return Color.BlueViolet;
                case EntryTypeBP.EntryList:
                    return Color.Tomato;
                case EntryTypeBP.Model:
                    return Color.Yellow;
                case EntryTypeBP.PolygonSoupList:
                    return Color.Goldenrod;
                case EntryTypeBP.GraphicsSpec:
                    return Color.SeaGreen;
                default:
                    break;
            }
            return Color.Transparent;
        }

    }

}
    */

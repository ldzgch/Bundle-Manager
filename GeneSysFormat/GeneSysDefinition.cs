using BundleFormat;
using PluginAPI;
using BundleUtilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace GeneSys {
    public class GeneSysDefinition : IEntryData
    {
        public uint InternalID;
        public uint ExternalID;
        public uint NumberOfFields;
        public string DefName;
        public FieldInfo[] Fields;

        public IEntryEditor GetEditor(BundleEntry entry)
        {
            return null;
        }

        public EntryType GetEntryType(BundleEntry entry)
        {
            return EntryTypeNFS.GeneSysDefinition;
        }

        public bool Read(BundleEntry entry, ILoader loader = null)
        {
            MemoryStream ms = entry.MakeStream();
            BinaryReader2 br = new BinaryReader2(ms);

            // header //
            InternalID = br.ReadUInt32();
            ExternalID = br.ReadUInt32();
            uint fieldOffset = br.ReadUInt32(); //  0x44 in all encounters yet
            NumberOfFields = br.ReadUInt32();

            byte unk1 = br.ReadByte(); // unk, but likely to have some meaning
            DefName = br.ReadCStr();

            // fields //

            br.BaseStream.Seek(fieldOffset, SeekOrigin.Begin);
            Fields = new FieldInfo[NumberOfFields];
            for (int i = 0; i < NumberOfFields;i++)
            {
                Fields[i] = new FieldInfo();
                Fields[i].Read(br);
            }
            // dependencies ??
            return true;
        }

        public bool Write(BundleEntry entry)
        {
            throw new NotImplementedException();
        }
    }

    public class FieldInfo
    {
        public uint ID;
        public DataType Type;
        public uint NumValues;
        public uint OffSet;
        public uint Size;

        public bool Read(BinaryReader2 br) { 
            ID = br.ReadUInt32();
            Type = (DataType)br.ReadInt32();
            NumValues = br.ReadUInt32();
            OffSet = br.ReadUInt32();
            Size = br.ReadUInt32();
            return true;
        }
    }

    public enum DataType
    {
        Int32 = 0,
        Float32 = 1,
        Bool = 2,
        Unk = 3, // does not exist
        String = 4,
        WideString = 5,
        ResourceHandle = 6,
        ResourceID = 7,
        Instance = 8,
        Enum = 9,
        RwVec2 = 10,
        RwVec3 = 11,
        RwVec4 = 12,
        Mat44 = 13,
        Mat44Affine = 14,
        Pointer = 0x1000,
    }

}

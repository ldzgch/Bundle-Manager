using BundleFormat;
using PluginAPI;
using BundleUtilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System;

namespace LuaList
{

    public class LuaList : IEntryData
    {
        public int version { get; set; }    //0x0	0x4	int32_t Version number	1	
        public byte[] padding { get; set; } //0x4	0x4			padding		
        [TypeConverter(typeof(ULongHexTypeConverter))]
        public ulong CgsId { get; set; }     //0x8	0x8	CgsID List ID Encoded
        public byte[] Unk0 { get; set; }    //0x10	0x4	Unk0* Script list Unk0 format	
        public byte[] typePointer { get; set; }//0x14	0x4	char[32] * *		Types		
        public byte[] variablePointer { get; set; } //0x18	0x4	char[32] * *		Variables		
        public int numScripts { get; set; }      //0x1C	0x4	uint32_t Num scripts		
        public int dataLength { get; set; }      //0x20	0x4	uint32_t Data length Not including padding to end
        public string listTitle { get; set; }   //0x24	0x80	char[128] List title		
        public short numTypes { get; set; }     //0xA4	0x1	uint8_t Num types		
        public short numVariables { get; set; } //0xA5	0x1	uint8_t Num variables
        public byte[] unknownData { get; set; }

        public IEntryEditor GetEditor(BundleEntry entry)
        {
            LuaListEditor luaListEditor = new LuaListEditor();
            luaListEditor.LuaList = this;
            luaListEditor.EditEvent += () =>
            {
                Write(entry);
            };
            return luaListEditor;
        }

        public EntryType GetEntryType(BundleEntry entry)
        {
            return EntryType.LUAList;
        }

        public int getLengthOfHeader() { 
            List<byte[]> bytes = new List<byte[]>();
            bytes.Add(BitConverter.GetBytes(version));
            bytes.Add(padding);
            bytes.Add(BitConverter.GetBytes(CgsId));
            bytes.Add(Unk0);
            bytes.Add(typePointer);
            bytes.Add(variablePointer);
            bytes.Add(BitConverter.GetBytes(numScripts));
            bytes.Add(BitConverter.GetBytes(dataLength));
            bytes.Add(Encoding.ASCII.GetBytes((listTitle.PadRight(128).Substring(0, 128).ToCharArray())));
            bytes.Add(BitConverter.GetBytes(numTypes));
            bytes.Add(BitConverter.GetBytes(numVariables));
            return bytes.SelectMany(i => i).Count();
        }

        public bool Read(BundleEntry entry, ILoader loader = null)
        {
            MemoryStream ms = entry.MakeStream();
            BinaryReader2 br = new BinaryReader2(ms);

            version = br.ReadInt32();
            padding = br.ReadBytes(4);
            CgsId = br.ReadUInt64();
            Unk0 = br.ReadBytes(4);
            typePointer = br.ReadBytes(4);
            variablePointer = br.ReadBytes(4);
            numScripts = br.ReadInt32();
            dataLength = br.ReadInt32();
            listTitle = new string(br.ReadChars(128));
            numTypes = br.ReadInt16();
            numVariables = br.ReadInt16();

            unknownData = br.ReadBytes(dataLength - getLengthOfHeader());

            br.Close();
            ms.Close();

            return true;
        }

        public bool Write(BundleEntry entry)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(version);
            bw.Write(padding);
            bw.Write(CgsId);
            bw.Write(Unk0);
            bw.Write(typePointer);
            bw.Write(variablePointer);
            bw.Write(numScripts);
            bw.Write(dataLength);
            bw.Write(listTitle.PadRight(128).Substring(0, 128).ToCharArray());
            bw.Write(numTypes);
            bw.Write(numVariables);
            bw.Write(unknownData);
            bw.Flush();
            byte[] data = ms.ToArray();
            bw.Close();
            ms.Close();

            entry.EntryBlocks[0].Data = data;
            entry.Dirty = true;
            return true;
        }
    }
}

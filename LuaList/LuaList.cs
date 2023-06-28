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
        public int version { get; set; }

        [TypeConverter(typeof(EncryptedStringConverter))]
        public EncryptedString CgsId { get; set; }

        public int dataLength;
        public string listTitle { get; set; }
        public List<LuaListEntry> entries { get; set; }
        public List<string> types { get; set; }
        public List<string> variables { get; set; }
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
            return EntryTypeBP.LUAList;
        }

        public int getLengthOfHeader()
        {
            List<byte[]> bytes = new List<byte[]>();
            bytes.Add(BitConverter.GetBytes(version));
            bytes.Add(new byte[4]);
            bytes.Add(BitConverter.GetBytes(CgsId.Encrypted));
            bytes.Add(BitConverter.GetBytes(192));
            bytes.Add(BitConverter.GetBytes(1231));
            bytes.Add(BitConverter.GetBytes(312312));
            bytes.Add(BitConverter.GetBytes(entries.Count));
            bytes.Add(BitConverter.GetBytes(dataLength));
            bytes.Add(Encoding.ASCII.GetBytes((listTitle.PadRight(128).Substring(0, 128).ToCharArray())));
            bytes.Add(new byte[] { (byte)types.Count() });
            bytes.Add(new byte[] { (byte)variables.Count() });
            bytes.Add(new byte[26]);
            return bytes.SelectMany(i => i).Count();
        }

        public int getLengthOfTypes()
        {
            if (types.Count == 0)
            {
                return 0;
            }
            return (types.Count * 32) + 32; // Pointer Array of 32 bytes
        }

        public int getLengthOfVariables()
        {
            if (variables.Count == 0)
            {
                return 0;
            }
            return (variables.Count * 32) + 32; // Pointer Array of 32 bytes
        }

        public int getLengthOfEntries()
        {
            return entries.Sum(i => i.getDataSize());
        }

        public bool Read(BundleEntry entry, ILoader loader = null)
        {
            MemoryStream ms = entry.MakeStream();
            BinaryReader2 br = new BinaryReader2(ms);

            version = br.ReadInt32(); //0x0	0x4	int32_t Version number	1
            br.ReadBytes(4); // 0x4	0x4			padding	
            CgsId = br.ReadEncryptedString();  //0x8	0x8	CgsID List ID Encoded
            var entriesPointer = br.ReadInt32(); //0x10	0x4	Unk0* Script list Unk0 format	
            var typePointer = br.ReadInt32(); //0x14	0x4	char[32] * *		Types
            var variablePointer = br.ReadInt32(); //0x18	0x4	char[32] * *		Variables
            var numScripts = br.ReadInt32(); //0x1C	0x4	uint32_t	Num scripts
            var dataLength = br.ReadInt32(); //0x20	0x4	uint32_t Data length Not including padding to end
            listTitle = br.ReadLenString(128); //0x24	0x80	char[128] List title	
            var numTypes = br.ReadByte(); //0xA4	0x1	uint8_t Num types		
            var numVariables = br.ReadByte(); //0xA5	0x1	uint8_t Num variables
            br.ReadBytes(26); // padding

            entries = new List<LuaListEntry>();
            for (int i = 0; i < numScripts; i++)
            {
                LuaListEntry luaentry = new LuaListEntry();
                luaentry.Read(loader, br);
                entries.Add(luaentry);
            }

            br.BaseStream.Position = typePointer;
            long currentAddress = br.BaseStream.Position;
            types = new List<string>();
            for (int i = 0; i < numTypes; i++)
            {
                int pointer = br.ReadInt32();
                currentAddress = br.BaseStream.Position;
                br.BaseStream.Position = pointer;
                types.Add(br.ReadLenString(32));
                br.BaseStream.Position = currentAddress;
            }
            br.BaseStream.Position = variablePointer;
            variables = new List<string>();
            for (int i = 0; i < numVariables; i++)
            {
                int pointer = br.ReadInt32();
                currentAddress = br.BaseStream.Position;
                br.BaseStream.Position = pointer;
                if (dataLength - pointer > 31)
                {
                    variables.Add(br.ReadLenString(32));
                }
                else
                {
                    Console.WriteLine(dataLength - pointer);
                    variables.Add(br.ReadLenString(dataLength - pointer));
                }
                br.BaseStream.Position = currentAddress;
            }
            Console.WriteLine(getLengthOfHeader() + getLengthOfEntries() + getLengthOfTypes() + getLengthOfVariables());
            Console.WriteLine(dataLength);
            br.Close();
            ms.Close();

            return true;
        }

        public bool Write(BundleEntry entry)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(version);
            bw.WriteUniquePadding(4);
            bw.WriteEncryptedString(CgsId);
            bw.Write(getLengthOfHeader());
            bw.Write(getLengthOfHeader() + getLengthOfEntries());
            bw.Write(getLengthOfHeader() + getLengthOfEntries() + getLengthOfTypes());
            bw.Write(entries.Count);
            bw.Write(getLengthOfHeader() + getLengthOfEntries() + getLengthOfTypes() + getLengthOfVariables()); //Calculate this
            bw.Write(listTitle.PadRight(128, '\0').Substring(0, 128).ToCharArray());
            bw.Write((byte)types.Count);
            bw.Write((byte)variables.Count);
            bw.WriteUniquePadding(26);
            foreach (LuaListEntry luaentry in entries)
            {
                luaentry.Write(bw);
            }
            var counter = 0;
            var pointerAddress = bw.BaseStream.Position;
            // Skip pointer address for now
            bw.BaseStream.Position = bw.BaseStream.Position + 32;
            foreach (string type in types)
            {
                // Save pointer to text
                int pointer = (int)bw.BaseStream.Position;
                bw.Write(type.PadRight(32, '\0').Substring(0, 32).ToCharArray());
                long currentAddress = bw.BaseStream.Position;
                // Go to pointer array
                bw.BaseStream.Position = pointerAddress + (4 * counter);
                bw.Write(pointer);
                counter++;
                // Go back to old position
                bw.BaseStream.Position = currentAddress;
            }
            counter = 0;
            pointerAddress = bw.BaseStream.Position;
            // Skip pointer address for now
            bw.BaseStream.Position = bw.BaseStream.Position + 32;
            foreach (string variable in variables)
            {
                // Save pointer to text
                int pointer = (int)bw.BaseStream.Position;
                bw.Write(variable.PadRight(32, '\0').Substring(0, 32).ToCharArray());
                long currentAddress = bw.BaseStream.Position;
                // Go to pointer array
                bw.BaseStream.Position = pointerAddress + (4 * counter);
                bw.Write(pointer);
                counter++;
                // Go back to old position
                bw.BaseStream.Position = currentAddress;
            }
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

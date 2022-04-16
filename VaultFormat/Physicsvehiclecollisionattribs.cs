using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BundleUtilities;

namespace VaultFormat
{
    public class Physicsvehiclecollisionattribs : IAttribute {
        public AttributeHeader header;
        public SizeAndPositionInformation info;

        public Vector3I BodyBox;
        public Physicsvehiclecollisionattribs(SizeAndPositionInformation chunk, AttributeHeader dataChunk)
        {
            header = dataChunk;
            info = chunk;
        }

        public int getDataSize()
        {
            List<byte[]> bytes = new List<byte[]>();
            bytes.Add(BodyBox.toBytes());
            Console.WriteLine(bytes.SelectMany(i => i).Count());
            return CountingUtilities.AddPadding(bytes);
        } 

        public AttributeHeader getHeader()
        {
            return header;
        }

        public SizeAndPositionInformation getInfo()
        {
            return info;
        }

        public void Read(ILoader loader, BinaryReader2 br)
        {
            BodyBox = br.ReadVector3I();
        }

        public void Write(BinaryWriter wr)
        {
            wr.Write(BodyBox.toBytes());
        }
    }
}

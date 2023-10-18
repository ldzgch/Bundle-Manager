using BundleFormat;
using PluginAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BaseHandlers
{
    public partial class TrafficEditor : Form, IEntryEditor
    {

        public TrafficEditor()
        {
            InitializeComponent();
        }

        private void RemoveTraffic(object sender, EventArgs e)
        {
            bool bigEndian = false;
            bool is64bit = false;

            MemoryStream ms = new MemoryStream(data.rawdata);
            BinaryReader br = new BinaryReader(ms); 
            
            br.BaseStream.Position = 2;
            uint numHulls = br.ReadUInt16();

            br.BaseStream.Position = 0xC;
            uint hullTableLoc = br.ReadUInt32();

            uint[] hullTable = new uint[numHulls];
            br.BaseStream.Position = hullTableLoc;
            for (int i = 0; i < numHulls; i++)
            {
                hullTable[i] = br.ReadUInt32(); 
            }
            foreach (uint hullLoc in hullTable){
                data.rawdata[hullLoc + 5] = 0;
            }
            foreach (uint hullLoc in hullTable)
            {
                br.BaseStream.Position = hullLoc;
                byte numSections = br.ReadByte();

                br.BaseStream.Position = hullLoc  + 0x28;
                uint sectionFlowLoc = br.ReadUInt32();

                br.BaseStream.Position = sectionFlowLoc;
                for(int i = 0;i < numSections; i++)
                {
                    data.rawdata[sectionFlowLoc + i * 4 + 2] = 0;
                }
            }

            entry.EntryBlocks[0].Data = data.rawdata;
            entry.Dirty = true;

            rmTraffic.Text = "done";

        }

        private void TrafficEditor_Load(object sender, EventArgs e)
        {
            rmTraffic = new Button();
            rmTraffic.Text = "remove traffic";
            rmTraffic.Location = new Point(70, 70);
            rmTraffic.Size = new Size(100, 100);
            rmTraffic.Visible = true;
            rmTraffic.BringToFront();
            rmTraffic.Click += new System.EventHandler(this.RemoveTraffic);
            this.Controls.Add(rmTraffic);           
        }
        Button rmTraffic;
        public Traffic data;
        public BundleEntry entry;


    }
}

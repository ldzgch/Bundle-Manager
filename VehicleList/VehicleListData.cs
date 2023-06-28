using BundleFormat;
using BundleUtilities;
using PluginAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VehicleList
{
    [Flags]
    public enum Flags
    {
        IsRaceVehicle       = 1,
        CanCheckTraffic     = 1 << 1,
        CanBeChecked        = 1 << 2,
        IsTrailer           = 1 << 3,
        CanTowTrailer       = 1 << 4,
        CanBePainted        = 1 << 5,
        Unknown0            = 1 << 6,
        IsFirstInSpeedRange = 1 << 7,
        HasSwitchableBoost  = 1 << 8,
        Unknown1            = 1 << 9,
        Unknown2            = 1 << 10,
        IsWIP               = 1 << 11,
        IsFromV10           = 1 << 12,
        IsFromV13           = 1 << 13,
        IsFromV14           = 1 << 14,
        IsFromV15           = 1 << 15,
        IsFromV16           = 1 << 16,
        IsFromV17           = 1 << 17,
        IsFromV18           = 1 << 18,
        IsFromV19           = 1 << 19
    }

    public enum VehicleRank
    {
        LEARNER,
        D_CLASS,
        C_CLASS,
        B_CLASS,
        A_CLASS,
        BURNOUT
    }

    public enum ClassUnlock : uint
    {
        SuperClassUnlock  = 0x0470A5BF,
        MuscleClassUnlock = 0x48346FEF,
        F1ClassUnlock     = 0x817B91D9,
        TunerClassUnlock  = 0xA3E2D8C9,
        HotRodClassUnlock = 0xB3845465,
        RivalGen          = 0xEBE39AE9
    }

    public enum AIMusic : uint
    {
        None,
        Muscle = 0xA9813C9D,
        Truck  = 0xCB72AEA7,
        Tuner  = 0x284D944B,
        Sedan  = 0xD95C2309,
        Exotic = 0x8A1A90E9,
        Super  = 0xB12A34DD
    }

    public enum AIExhaustIndex
    {
        None,
        AIROD_EX,
        AI_CIVIC_EX,
        AI_GT_ENG,
        AI_MUST_EX,
        AI_F1_EX
    }

    [Flags]
    public enum VehicleCategory
    {
        NONE,
        PARADISE_CARS  = 1,
        PARADISE_BIKES = 1 << 1,
        ONLINE_CARS    = 1 << 2,
        TOYS           = 1 << 3,
        LEGENDARY_CARS = 1 << 4,
        BOOST_SPECIALS = 1 << 5,
        COP_CARS       = 1 << 6,
        ISLAND_CARS    = 1 << 7
    }
    
    public enum VehicleType
    {
        CAR,
        BIKE,
        PLANE
    }

    public enum BoostType
    {
        SPEED,
        AGGRESSION,
        STUNT,
        NONE,
        LOCKED
    }

    public enum FinishType
    {
        DEFAULT,
        COLOUR,
        PATTERN,
        SILVER,
        GOLD,
        COMMUNITY
    }

    public enum ColorType
    {
        GLOSS,
        METALLIC,
        PEARLESCENT,
        SPECIAL,
        UNKNOWN
    }

    public class VehicleListData : IEntryData
    {
        public int Unknown1;
        public int Unknown2;
        public List<Vehicle> Entries;

        public VehicleListData()
        {
            Entries = new List<Vehicle>();
        }

        public IEntryEditor GetEditor(BundleEntry entry)
        {
            VehicleListForm vehicleList = new VehicleListForm();
            vehicleList.List = this;
            vehicleList.Edit += () =>
            {
                Write(entry);
            };

            return vehicleList;
        }

        public EntryType GetEntryType(BundleEntry entry)
        {
            return EntryTypeBP.VehicleList;
        }

        private void Clear()
        {
            Unknown1 = default;
            Unknown2 = default;
            Entries.Clear();
        }

        public bool Read(BundleEntry entry, ILoader loader = null)
        {
            Clear();

            MemoryStream ms = new MemoryStream(entry.EntryBlocks[0].Data);
            BinaryReader2 br = new BinaryReader2(ms);
            br.BigEndian = entry.Console;

            int count = br.ReadInt32();
            int startOff = br.ReadInt32();

            Unknown1 = br.ReadInt32();
            Unknown2 = br.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                Vehicle vehicle = new Vehicle();

                vehicle.Index = i;

                vehicle.ID = br.ReadEncryptedString();
                vehicle.ParentID = br.ReadEncryptedString();
                vehicle.WheelType = Encoding.ASCII.GetString(br.ReadBytes(32));
                vehicle.CarName = Encoding.ASCII.GetString(br.ReadBytes(64));
                vehicle.CarBrand = Encoding.ASCII.GetString(br.ReadBytes(32));
                vehicle.DamageLimit = br.ReadSingle();
                vehicle.Flags = (Flags)br.ReadInt32();
                vehicle.BoostLength = br.ReadByte();
                vehicle.VehicleRank = (VehicleRank)br.ReadByte();
                vehicle.BoostCapacity = br.ReadByte();
                vehicle.DisplayStrength = br.ReadByte();
                vehicle.padding0 = br.ReadInt32();
                vehicle.AttribSysCollectionKey = br.ReadInt64();
                vehicle.ExhaustName = br.ReadEncryptedString();
                vehicle.ExhaustID = br.ReadInt64();
                vehicle.EngineID = br.ReadInt64();
                vehicle.EngineName = br.ReadEncryptedString();
                vehicle.ClassUnlockStreamHash = (ClassUnlock)br.ReadInt32();
                vehicle.padding1 = br.ReadInt32();
                vehicle.CarShutdownStreamID = br.ReadInt64();
                vehicle.CarReleasedStreamID = br.ReadInt64();
                vehicle.AIMusicHash = (AIMusic)br.ReadInt32();
                vehicle.AIExhaustIndex = (AIExhaustIndex)br.ReadByte();
                vehicle.AIExhaustIndex2 = (AIExhaustIndex)br.ReadByte();
                vehicle.AIExhaustIndex3 = (AIExhaustIndex)br.ReadByte();
                vehicle.padding2 = br.ReadByte();
                vehicle.Unknown[0] = br.ReadInt64();
                vehicle.Unknown[1] = br.ReadInt64();
                vehicle.Category = (VehicleCategory)br.ReadInt32();
                vehicle.VehicleAndBoostType = br.ReadByte();
                vehicle.FinishType = (FinishType)br.ReadByte();
                vehicle.MaxSpeedNoBoost = br.ReadByte();
                vehicle.MaxSpeedBoost = br.ReadByte();
                vehicle.DisplaySpeed = br.ReadByte();
                vehicle.DisplayBoost = br.ReadByte();
                vehicle.Color = br.ReadByte();
                vehicle.ColorType = (ColorType)br.ReadByte();
                vehicle.padding3 = br.ReadInt32();

                vehicle.VehicleType = (VehicleType)(vehicle.VehicleAndBoostType >> 4 & 0xF); // High nibble
                vehicle.BoostType = (BoostType)(vehicle.VehicleAndBoostType & 0xF); // Low nibble

                Entries.Add(vehicle);
            }

            br.Close();
            return true;
        }

        public bool Write(BundleEntry entry)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bool console = entry.Console;
            // TODO: Implement Console Saving

            bw.Write(console ? Util.ReverseBytes((int)Entries.Count) : (int)Entries.Count);
            bw.Write(console ? Util.ReverseBytes((int)0x10) : (int)0x10);

            bw.Write(console ? Util.ReverseBytes(Unknown1) : Unknown1);
            bw.Write(console ? Util.ReverseBytes(Unknown2) : Unknown2);

            for (int i = 0; i < Entries.Count; i++)
            {
                Vehicle vehicle = Entries[i];

                // Need to create the correct byte before writing
                vehicle.VehicleAndBoostType = (byte)(((byte)vehicle.VehicleType << 4) + (byte)vehicle.BoostType);

                bw.WriteEncryptedString(vehicle.ID, console);
                bw.WriteEncryptedString(vehicle.ParentID, console);
                bw.WriteLenString(vehicle.WheelType, 32, console);
                bw.WriteLenString(vehicle.CarName, 64, console);
                bw.WriteLenString(vehicle.CarBrand, 32, console);
                bw.Write(console ? Util.ReverseBytes(vehicle.DamageLimit) : vehicle.DamageLimit);
                bw.Write(console ? Util.ReverseBytes((uint)vehicle.Flags) : (uint)vehicle.Flags);
                bw.Write(vehicle.BoostLength);
                bw.Write((byte)vehicle.VehicleRank);
                bw.Write(vehicle.BoostCapacity);
                bw.Write(vehicle.DisplayStrength);
                bw.Write(vehicle.padding0);
                bw.Write(console ? Util.ReverseBytes(vehicle.AttribSysCollectionKey) : vehicle.AttribSysCollectionKey);
                bw.WriteEncryptedString(vehicle.ExhaustName, console);
                bw.Write(console ? Util.ReverseBytes(vehicle.ExhaustID) : vehicle.ExhaustID);
                bw.Write(console ? Util.ReverseBytes(vehicle.EngineID) : vehicle.EngineID);
                bw.WriteEncryptedString(vehicle.EngineName, console);
                bw.Write(console ? Util.ReverseBytes((uint)vehicle.ClassUnlockStreamHash) : (uint)vehicle.ClassUnlockStreamHash);
                bw.Write(vehicle.padding1);
                bw.Write(console ? Util.ReverseBytes(vehicle.CarShutdownStreamID) : vehicle.CarShutdownStreamID);
                bw.Write(console ? Util.ReverseBytes(vehicle.CarReleasedStreamID) : vehicle.CarReleasedStreamID);
                bw.Write(console ? Util.ReverseBytes((uint)vehicle.AIMusicHash) : (uint)vehicle.AIMusicHash);
                bw.Write((byte)vehicle.AIExhaustIndex);
                bw.Write((byte)vehicle.AIExhaustIndex2);
                bw.Write((byte)vehicle.AIExhaustIndex3);
                bw.Write(vehicle.padding2);
                bw.Write(vehicle.Unknown[0]);
                bw.Write(vehicle.Unknown[1]);
                bw.Write(console ? Util.ReverseBytes((uint)vehicle.Category) : (uint)vehicle.Category);
                bw.Write(vehicle.VehicleAndBoostType);
                bw.Write((byte)vehicle.FinishType);
                bw.Write(vehicle.MaxSpeedNoBoost);
                bw.Write(vehicle.MaxSpeedBoost);
                bw.Write(vehicle.DisplaySpeed);
                bw.Write(vehicle.DisplayBoost);
                bw.Write(vehicle.Color);
                bw.Write((byte)vehicle.ColorType);
                bw.Write(vehicle.padding3);
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

    public class Vehicle
    {
        public int Index;
        public EncryptedString ID;
        public EncryptedString ParentID;
        public string WheelType;
        public string CarName;
        public string CarBrand;
        public float DamageLimit;
        public Flags Flags;
        public byte BoostLength;
        public VehicleRank VehicleRank;
        public byte BoostCapacity;
        public byte DisplayStrength;
        public int padding0;
        public long AttribSysCollectionKey;
        public EncryptedString ExhaustName;
        public long ExhaustID;
        public long EngineID;
        public EncryptedString EngineName;
        public ClassUnlock ClassUnlockStreamHash;
        public int padding1;
        public long CarShutdownStreamID;
        public long CarReleasedStreamID;
        public AIMusic AIMusicHash;
        public AIExhaustIndex AIExhaustIndex;
        public AIExhaustIndex AIExhaustIndex2;
        public AIExhaustIndex AIExhaustIndex3;
        public byte padding2;
        public long[] Unknown = {0, 0}; // idk C# at all so this forces reading 128 bits in a probably dumb way
        public VehicleCategory Category;
        public byte VehicleAndBoostType;
        public VehicleType VehicleType;
        public BoostType BoostType;
        public FinishType FinishType;
        public byte MaxSpeedNoBoost;
        public byte MaxSpeedBoost;
        public byte DisplaySpeed;
        public byte DisplayBoost;
        public byte Color;
        public ColorType ColorType;
        public int padding3;

        public Vehicle()
        {

        }

        public Vehicle(Vehicle copy)
        {
            Index = copy.Index;
            ID = copy.ID;
            ParentID = copy.ParentID;
            WheelType = copy.WheelType;
            CarName = copy.CarName;
            CarBrand = copy.CarBrand;
            DamageLimit = copy.DamageLimit;
            Flags = copy.Flags;
            BoostLength = copy.BoostLength;
            VehicleRank = copy.VehicleRank;
            BoostCapacity = copy.BoostCapacity;
            DisplayStrength = copy.DisplayStrength;
            padding0 = copy.padding0;
            AttribSysCollectionKey = copy.AttribSysCollectionKey;
            ExhaustName = copy.ExhaustName;
            ExhaustID = copy.ExhaustID;
            EngineID = copy.EngineID;
            EngineName = copy.EngineName;
            ClassUnlockStreamHash = copy.ClassUnlockStreamHash;
            padding1 = copy.padding1;
            CarShutdownStreamID = copy.CarShutdownStreamID;
            CarReleasedStreamID = copy.CarReleasedStreamID;
            AIMusicHash = copy.AIMusicHash;
            AIExhaustIndex = copy.AIExhaustIndex;
            AIExhaustIndex2 = copy.AIExhaustIndex2;
            AIExhaustIndex3 = copy.AIExhaustIndex3;
            padding2 = copy.padding2;
            Unknown = copy.Unknown;
            Category = copy.Category;
            VehicleAndBoostType = copy.VehicleAndBoostType;
            VehicleType = copy.VehicleType;
            BoostType = copy.BoostType;
            FinishType = copy.FinishType;
            MaxSpeedNoBoost = copy.MaxSpeedNoBoost;
            MaxSpeedBoost = copy.MaxSpeedBoost;
            Color = copy.Color;
            ColorType = copy.ColorType;
            DisplaySpeed = copy.DisplaySpeed;
            DisplayBoost = copy.DisplayBoost;
            padding3 = copy.padding3;
        }
    }
}

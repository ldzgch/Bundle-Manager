﻿using BundleFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleList;

namespace BundleManager
{
	public static class EntryTypeRegistry
	{
		private static Dictionary<EntryType, IEntryData> _handlers = new Dictionary<EntryType, IEntryData>();

		static EntryTypeRegistry()
		{
			InitEntryTypes();
		}

		public static void Register(EntryType type, IEntryData handler)
		{
			if (IsRegistered(type))
				return;
			_handlers.Add(type, handler);
		}

		public static void Unregister(EntryType type)
		{
			if (!IsRegistered(type))
				return;
			_handlers.Remove(type);
		}

		public static IEntryData GetHandler(EntryType type)
		{
			if (!IsRegistered(type))
				return null;

			return _handlers[type];
		}

		public static bool IsRegistered(EntryType type)
		{
			return _handlers.ContainsKey(type);
		}

		private static void InitEntryTypes()
		{
			Register(EntryType.PolygonSoupListResourceType, new PolygonSoupList());
			Register(EntryType.VehicleListResourceType, new VehicleListData());
		}
	}
}

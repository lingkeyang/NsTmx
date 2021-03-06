﻿using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using XmlParser;
using TmxCSharp.Models;
using UnityEngine;
using Utils;

namespace TmxCSharp.Loader
{
	internal static class ObjectLayerLoader
	{
		public static IList<ObjectGroup> LoadObjectGroup(Stream stream)
		{
			if (stream == null || stream.Length <= 0)
				return null;

			int groupCnt = FilePathMgr.Instance.ReadInt(stream);
			if (groupCnt <= 0)
				return null;

			IList<ObjectGroup> ret = new List<ObjectGroup>(groupCnt);
			for (int i = 0; i < groupCnt; ++i)
			{
				string name = FilePathMgr.Instance.ReadString(stream);
				int width = FilePathMgr.Instance.ReadInt(stream);
				int height = FilePathMgr.Instance.ReadInt(stream);

				ObjectGroup gp = new ObjectGroup (name, width, height);
				ret.Add(gp);
				LoadObject (stream, gp);
			}

			return ret;
		}

		public static void SaveToBinary(Stream stream, IList<ObjectGroup> list)
		{
			if (stream == null)
				return;
			if (list == null)
			{
				FilePathMgr.Instance.WriteInt(stream, 0);
				return;
			}

			FilePathMgr.Instance.WriteInt(stream, list.Count);

			for (int i = 0; i < list.Count; ++i)
			{
				ObjectGroup gp = list[i];
				FilePathMgr.Instance.WriteString(stream, gp.Name);
				FilePathMgr.Instance.WriteInt(stream, gp.Width);
				FilePathMgr.Instance.WriteInt(stream, gp.Height);
				SaveObject(stream, gp);
			}
		}

		public static IList<ObjectGroup> LoadObjectGroup(XMLNode parent)
		{
			if (parent == null)
				return null;

			XMLNodeList objectNodeList = parent.GetNodeList ("objectgroup");
			if (objectNodeList == null || objectNodeList.Count <= 0)
				return null;

			IList<ObjectGroup> ret = null;
			for (int i = 0; i < objectNodeList.Count; ++i) {
				XMLNode node = objectNodeList [i] as XMLNode;
				if (node == null)
					continue;

				string name = node.GetValue ("@name");
				if (string.IsNullOrEmpty (name))
					continue;
				
				int width;
				string str = node.GetValue ("@width");
				if (!int.TryParse (str, out width))
					width = 0;

				int height;
				str = node.GetValue ("@height");
				if (!int.TryParse (str, out height))
					height = 0;

				if (ret == null)
					ret = new List<ObjectGroup> ();
				ObjectGroup gp = new ObjectGroup (name, width, height);
				LoadObject (node, gp);
				ret.Add (gp);
			}

			return ret;

		}

		private static void SaveObject(Stream stream, ObjectGroup gp)
		{
			FilePathMgr.Instance.WriteInt(stream, gp.LayerCount);

			for (int i = 0; i < gp.LayerCount; ++i)
			{
				ObjectLayer layer = gp.GetLayer(i);
				FilePathMgr.Instance.WriteString(stream, layer.Name);
				FilePathMgr.Instance.WriteString(stream, layer.Type);
				FilePathMgr.Instance.WriteInt(stream, layer.X);
				FilePathMgr.Instance.WriteInt(stream, layer.Y);
				FilePathMgr.Instance.WriteInt(stream, layer.Width);
				FilePathMgr.Instance.WriteInt(stream, layer.Height);
				PropertysLoader.SaveToBinary(stream, layer.Props);
				SavePolygon(stream, layer.Polygon);
			}
		}

		private static void LoadObject(Stream stream, ObjectGroup gp)
		{
			int cnt = FilePathMgr.Instance.ReadInt(stream);
			for (int i = 0; i < cnt; ++i)
			{
				string name = FilePathMgr.Instance.ReadString(stream);
				string type = FilePathMgr.Instance.ReadString(stream);
				int x = FilePathMgr.Instance.ReadInt(stream);
				int y = FilePathMgr.Instance.ReadInt(stream);
				int width = FilePathMgr.Instance.ReadInt(stream);
				int height = FilePathMgr.Instance.ReadInt(stream);

				ObjectLayer layer = new ObjectLayer (name, x, y, width, height, type);
				gp.AddLayer (layer);

				layer.Props = PropertysLoader.LoadPropertys (stream);
				layer.Polygon = LoadPolygon(stream);
			}
		}

		private static void LoadObject(XMLNode parent, ObjectGroup gp)
		{
			if (parent == null || gp == null || !gp.IsVaild)
				return;

			XMLNodeList objNodeList =  parent.GetNodeList ("object");
			if (objNodeList == null || objNodeList.Count <= 0)
				return;

			for (int i = 0; i < objNodeList.Count; ++i) {
				XMLNode node = objNodeList [i] as XMLNode;
				if (node == null)
					continue;
				
				string name = node.GetValue ("@name");

				string type = node.GetValue ("@type");

				int x;
				string str = node.GetValue ("@x");
				if (!int.TryParse (str, out x))
					x = 0;

				int y;
				str = node.GetValue ("@y");
				if (!int.TryParse (str, out y))
					y = 0;

				int width;
				str = node.GetValue ("@width");
				if (!int.TryParse (str, out width))
					width = 0;

				int height;
				str = node.GetValue ("@height");
				if (!int.TryParse (str, out height))
					height = 0;

				ObjectLayer layer = new ObjectLayer (name, x, y, width, height, type);
				gp.AddLayer (layer);

				layer.Props = PropertysLoader.LoadPropertys (node);
				layer.Polygon = LoadPolygon(node);
			}

		}

		private static readonly char[] _cVecsSplit = new char[]{' '};
		private static readonly char[] _cVecSplit = new char[]{','};

		private static IList<Vector2> LoadPolygon(Stream stream)
		{
			int pointCnt = FilePathMgr.Instance.ReadInt(stream);
			if (pointCnt <= 0)
				return null;

			IList<Vector2> ret = new List<Vector2>();
			for (int i = 0; i < pointCnt; ++i)
			{
				int x = FilePathMgr.Instance.ReadInt(stream);
				int y = FilePathMgr.Instance.ReadInt(stream);
				Vector2 vec2 = new Vector2(x, y);
				ret.Add(vec2);
			}

			return ret;
		}

		private static void SavePolygon(Stream stream, IList<Vector2> vecs)
		{
			if (vecs == null)
			{
				FilePathMgr.Instance.WriteInt(stream, 0);
				return;
			}

			FilePathMgr.Instance.WriteInt(stream, vecs.Count);

			for (int i = 0; i < vecs.Count; ++i)
			{
				Vector2 vec = vecs[i];
				int x = Mathf.RoundToInt(vec.x);
				int y = Mathf.RoundToInt(vec.y);
				FilePathMgr.Instance.WriteInt(stream, x);
				FilePathMgr.Instance.WriteInt(stream, y);
			}
		}

		private static IList<Vector2> LoadPolygon(XMLNode parent)
		{
			if (parent == null)
				return null;
			XMLNodeList polygonList = parent.GetNodeList("polygon");
			if (polygonList == null || polygonList.Count <= 0)
				return null;

			IList<Vector2> ret = null;

			XMLNode node = polygonList[0] as XMLNode;
			if (node == null)
				return ret;

			string str = node.GetValue("@points");
			if (string.IsNullOrEmpty(str))
				return ret;

			string[] vecsStr = str.Split(_cVecsSplit);
			if (vecsStr == null || vecsStr.Length <= 0)
				return ret;

			for (int i = 0; i < vecsStr.Length; ++i)
			{
				string s = vecsStr[i];
				if (string.IsNullOrEmpty(s))
					continue;

				string[] vec = s.Split(_cVecSplit);
				if (vec == null || vec.Length < 2)
					continue;
				
				string ss = vec[0];
				int x;
				if (!int.TryParse(ss, out x))
					continue;

				ss = vec[1];
				int y;
				if (!int.TryParse(ss, out y))
					continue;

				if (ret == null)
					ret = new List<Vector2>();

				Vector2 vec2 = new Vector2(x, y);
				ret.Add(vec2);
			}

			return ret;
		}

	}
}


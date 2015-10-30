﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace GAIPS.Serialization.SerializationGraph
{
	public partial class Graph
	{
		private int m_idCounter = 0;
		private StrongLinkDictionary<object, int> m_links = new StrongLinkDictionary<object, int>();
		private SortedDictionary<int, ObjectGraphNode> m_refs = new SortedDictionary<int, ObjectGraphNode>();

		private IGraphNode m_root = null;
		public IGraphNode Root {
			get
			{
				return m_root;
			}
			set
			{
				if (m_root != null)
				{
					((BaseGraphNode)m_root).IsRoot = false;
				}
				m_root = value;
				if (m_root != null)
				{
					((BaseGraphNode)m_root).IsRoot = true;
				}
			}
		}

		private byte m_typeidCounter = 0;
		private Dictionary<Type, TypeEntry> m_registedTypes = new Dictionary<Type, TypeEntry>();

		private BaseSerializer m_associatedSerializer;

		public Graph(BaseSerializer serializer)
		{
			m_associatedSerializer = serializer;
		}

		internal Graph(object objectToSerialize, BaseSerializer serializer)
			: this(serializer)
		{
			Root = BuildNode(objectToSerialize, null);
		}

		public TypeEntry GetTypeEntry(Type type)
		{
			TypeEntry t;
			if (m_registedTypes.TryGetValue(type, out t))
				return t;

			t = new TypeEntry(type, m_typeidCounter++);
			m_registedTypes[type] = t;
			return t;
		}

		public TypeEntry GetTypeEntry(byte typeId)
		{
			return m_registedTypes.Values.First(t => t.TypeId == typeId);
		}

		public void RegistTypeEntry(Type type, byte typeId)
		{
			TypeEntry entry;
			if (m_registedTypes.TryGetValue(type, out entry))
			{
				if (entry.TypeId != typeId)
					throw new Exception("Type already registed");	//TODO get a better exception
				return;
			}

			m_registedTypes[type] = new TypeEntry(type, typeId);
		}

		public IEnumerable<TypeEntry> GetRegistedTypes()
		{
			return m_registedTypes.Values;
		}

		public IObjectGraphNode CreateObjectData()
		{
			return new ObjectGraphNode(-1,this);
		}

		public IObjectGraphNode GetObjectDataForRefId(int refid)
		{
			ObjectGraphNode node;
			if (m_refs.TryGetValue(refid, out node))
				return node;

			node = new ObjectGraphNode(refid,this);
			m_refs[refid] = node;
			return node;
		}

		public bool GetObjectNode(object obj, out IObjectGraphNode dataNode)
		{
			int id;
			if (m_links.TryGetValue(obj, out id))
			{
				dataNode = m_refs[id];
				return false;
			}

			var node = new ObjectGraphNode(m_idCounter++, this);
			m_refs[node.RefId] = node;
			m_links[obj] = node.RefId;
			dataNode = node;
			return true;
		}

		public IEnumerable<IObjectGraphNode> GetReferences()
		{
			return m_refs.Values.Where(n => n.IsReferedMultipleTimes).Cast<IObjectGraphNode>();
		}

		public bool TryGetObjectForRefId(int refId, out object linkedObject)
		{
			return m_links.TryGetKey(refId, out linkedObject);
		}

		private void LinkObjectToNode(ObjectGraphNode node, object targetObject)
		{
			if(m_links.ContainsValue(node.RefId))
				m_links.RemoveValue(node.RefId);

			m_links.Add(targetObject,node.RefId);
		}

		#region Node Builders

		public IGraphNode BuildNode(object obj, Type fieldType)
		{
			if (obj == null)
				return null;

			IGraphNode result;
			Type objType = obj.GetType();
			if (objType.IsArray || objType.IsPrimitiveData())
			{
				//Boxable Values (arrays, bools, numbers, strings)
				IGraphNode valueNode;
				if (objType.IsArray)
				{
					Type elemType = objType.GetElementType();
					ISequenceGraphNode array = BuildSequenceNode();
					IEnumerator it = ((IEnumerable)obj).GetEnumerator();
					while (it.MoveNext())
					{
						IGraphNode elem = BuildNode(it.Current, elemType);
						array.Add(elem);
					}
					valueNode = array;
				}
				else
				{
					//Primitive data type
					if (objType == typeof(string))
						valueNode = BuildStringNode(obj as string);
					else if (objType.IsEnum)
						valueNode = m_associatedSerializer.EnumToGraphNode(obj as Enum, this);
					else
						valueNode = BuildPrimitiveNode(obj as ValueType);
				}

				if (objType != fieldType)
				{
					//Value needs to be boxed
					var boxNode = CreateObjectData();
					boxNode.ObjectType = GetTypeEntry(objType);
					boxNode["boxedValue"] = valueNode;
					valueNode = boxNode;
				}

				result = valueNode;
			}
			else
			{
				//Non-Boxable Values (structs and objects)
				IObjectGraphNode objReturnData;
				bool extractData=true;
				if(objType.IsValueType)
				{
					//Structure
					objReturnData = CreateObjectData();
				}
				else
				{
					//Classes
					if (!GetObjectNode(obj, out objReturnData))
						extractData=false;
				}

				if(extractData)
				{
					var surrogate = SerializationServices.SurrogateSelector.GetSurrogate(objType);
					surrogate.GetObjectData(obj, objReturnData);
				}

				if ((objReturnData.ObjectType == null) && (objType != fieldType))
					objReturnData.ObjectType = GetTypeEntry(objType);

				result = objReturnData;
			}
			return result;
		}

		public IPrimitiveGraphNode BuildPrimitiveNode(ValueType value)
		{
			return new PrimitiveGraphNode(value, this);
		}

		public IStringGraphNode BuildStringNode(string value)
		{
			return new StringGraphNode(value, this);
		}

		public ISequenceGraphNode BuildSequenceNode()
		{
			return new SequenceGraphNode(this);
		}

		#endregion

		#region Value Builders

		public object RebuildObject(Type requestedType)
		{
			return RebuildObject((BaseGraphNode)Root, requestedType);
		}

		private object RebuildObject(BaseGraphNode nodeToRebuild, Type requestedType)
		{
			if (nodeToRebuild == null)
				return null;
			if (requestedType!=null && requestedType.IsEnum)
				return m_associatedSerializer.GraphNodeToEnum(nodeToRebuild, requestedType);
			return nodeToRebuild.ExtractObject(requestedType);
		}

		#endregion
	}
}
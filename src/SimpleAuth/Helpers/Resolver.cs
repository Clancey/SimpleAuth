using System;
using System.Collections.Generic;
namespace SimpleAuth
{
	public static class Resolver
	{
		static readonly Dictionary<Type, Type> RegisteredTypes = new Dictionary<Type, Type> () {
			{typeof(IAuthStorage), typeof(AuthStorage)}
		};
		static readonly Dictionary<Type,object> Singletons = new Dictionary<Type, object>();

		public static void Register<TType, TType1>() where TType1 : TType
		{
			RegisteredTypes[typeof (TType)] = typeof (TType1);
		}

		public static void Register(Type type, Type cell)
		{
			RegisteredTypes[type] = cell;
		}

		public static T GetObject<T>(bool singleton = true)
		{
			Type cellType;
			if (!RegisteredTypes.TryGetValue(typeof(T), out cellType))
				return default(T);
			if(!singleton)
				return (T) Activator.CreateInstance(cellType);
			Object item;
			if (!Singletons.TryGetValue (cellType, out item)) {
				Singletons [cellType] = item = (T)Activator.CreateInstance (cellType);
			}

			return (T)item;
		}
	}
}


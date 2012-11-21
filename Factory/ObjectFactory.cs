using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Collections;

namespace Miata.Library.Factory
{
	public static class ObjectFactory<T>
	{
		public static Func<T> CreateObject { get; private set; } 
		
		static ObjectFactory() 
		{ 
			Type objType = typeof(T); 
			var dynMethod = new DynamicMethod("DM$OBJ_FACTORY_" + objType.Name, objType, null, objType); 
			ILGenerator ilGen = dynMethod.GetILGenerator(); 
			ilGen.Emit(OpCodes.Newobj, objType.GetConstructor(Type.EmptyTypes)); 
			ilGen.Emit(OpCodes.Ret); 
			CreateObject = (Func<T>)dynMethod.CreateDelegate(typeof(Func<T>)); 
		} 
	} 
}

using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WebExecutor
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class LuaFunctionAttribute : Attribute
    {
        public string Name { get; private set; }

        public LuaFunctionAttribute(string name = null)
        {
            Name = name;
        }
    }

    public static class LuaExtensions
    {
        public static void RegisterFunction(this Lua lua, object target, string name)
        {
            MethodInfo method;
            if (target is Type)
                method = ((Type)target).GetMethod(name);
            else
                method = target.GetType().GetMethod(name);
            RegisterFunction(lua, target, method);
        }

        public static void RegisterFunction(this Lua lua, object target, MethodInfo method)
        {
            object[] attributes = method.GetCustomAttributes(typeof(LuaFunctionAttribute), false);
            var attr = (LuaFunctionAttribute)attributes.FirstOrDefault();
            lua.RegisterFunction(
                (attr == null || attr.Name == null) ? method.Name : attr.Name,
                target is Type ? null : target,
                method);
        }
        
        public static LuaTable CreateTable(this Lua lua)
        {
            return (LuaTable)lua.DoString("return {}")[0];
        }

        public static LuaTable CreateTable(this Lua lua, params object[] content)
        {
            return CreateTable(lua, (System.Collections.IEnumerable)content);
        }

        public static LuaTable CreateTable(this Lua lua, System.Collections.IEnumerable content)
        {
            LuaTable table = CreateTable(lua);
            if (content != null)
            {
                var pairs = AsPairs(content);
                if (pairs == null)
                {
                    int index = 1;
                    foreach (object obj in content)
                    {
                        table[index] = obj;
                        index++;
                    }
                }
                else
                {
                    foreach (var pair in pairs)
                    {
                        table[pair.Key] = pair.Value;
                    }
                }
            }
            return table;
        }

        private static IEnumerable<KeyValuePair<object, object>> AsPairs(System.Collections.IEnumerable collection)
        {
            Type collectionType = collection.GetType();
            var reifiedEnumerable = (from iface in collectionType.GetInterfaces()
                                     let definition = iface.GetGenericTypeDefinition()
                                     where definition == typeof(IEnumerable<>)
                                     select iface).FirstOrDefault();
            if (reifiedEnumerable == null) { return null; }
            Type genericArg = reifiedEnumerable.GetGenericArguments()[0];
            if (!genericArg.IsGenericType || genericArg.GetGenericTypeDefinition() != typeof(KeyValuePair<,>)) { return null; }
            return EnumerateKeyValuePairs(collection);
        }

        private static IEnumerable<KeyValuePair<object, object>> EnumerateKeyValuePairs(System.Collections.IEnumerable collection)
        {
            foreach (dynamic pair in collection)
            {
                yield return new KeyValuePair<object, object>(pair.Key, pair.Value);
            }
        }
    }
}

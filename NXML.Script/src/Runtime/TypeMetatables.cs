using System;
using System.Collections.Generic;
using System.Reflection;
using NXML;

namespace NXML.Script {
    public partial class NXMLRuntime {
        public Dictionary<string, Type> MetatableMap = new Dictionary<string, Type>();

        public Dictionary<Type, Dictionary<string, FieldInfo>> TypeFieldMap = new Dictionary<Type, Dictionary<string, FieldInfo>>();
        public Dictionary<Type, MethodInfo> ContainerIndexMethodMap = new Dictionary<Type, MethodInfo>();
        public Dictionary<Type, MethodInfo> ContainerNewIndexMethodMap = new Dictionary<Type, MethodInfo>();
        public Dictionary<Type, MethodInfo> ContainerCountMethodMap = new Dictionary<Type, MethodInfo>();
       public Dictionary<Type, MethodInfo> ContainerAddMethodMap = new Dictionary<Type, MethodInfo>();
        public Dictionary<Type, MethodInfo> ContainerRemoveMethodMap = new Dictionary<Type, MethodInfo>();
        public Dictionary<Type, MethodInfo> ListInsertMethodMap = new Dictionary<Type, MethodInfo>();
        public Dictionary<Type, MethodInfo> ContainerClearMethodMap = new Dictionary<Type, MethodInfo>();
        public Dictionary<Type, MethodInfo> ContainerContainsMethodMap = new Dictionary<Type, MethodInfo>();
        public Dictionary<Type, MethodInfo> DictGetEnumeratorMethodMap = new Dictionary<Type, MethodInfo>();
        public Dictionary<Type, MethodInfo> DictEnumeratorCurrentMethodMap = new Dictionary<Type, MethodInfo>();
        public Dictionary<Type, MethodInfo> DictEnumeratorMoveNextMethodMap = new Dictionary<Type, MethodInfo>();
        public Dictionary<Type, MethodInfo> DictEnumeratorKVPairKeyMethodMap = new Dictionary<Type, MethodInfo>();
        public Dictionary<Type, MethodInfo> DictEnumeratorKVPairValueMethodMap = new Dictionary<Type, MethodInfo>();


        private void CreateListMethodMap(Type type) {
            ContainerIndexMethodMap[type] = type.GetMethod("get_Item");
            ContainerNewIndexMethodMap[type] = type.GetMethod("set_Item");
            ContainerCountMethodMap[type] = type.GetMethod("get_Count");
            ContainerAddMethodMap[type] = type.GetMethod("Add");
            ContainerRemoveMethodMap[type] = type.GetMethod("RemoveAt");
            ListInsertMethodMap[type] = type.GetMethod("Insert");
            ContainerClearMethodMap[type] = type.GetMethod("Clear");
            ContainerContainsMethodMap[type] = type.GetMethod("Contains");
        }

        private void CreateDictionaryMethodMap(Type type) {
            ContainerIndexMethodMap[type] = type.GetMethod("get_Item");
            ContainerNewIndexMethodMap[type] = type.GetMethod("set_Item");
            ContainerCountMethodMap[type] = type.GetMethod("get_Count");
            ContainerRemoveMethodMap[type] = type.GetMethod("Remove", new Type[] { type.GetGenericArguments()[0] });
            ContainerClearMethodMap[type] = type.GetMethod("Clear");
            ContainerContainsMethodMap[type] = type.GetMethod("ContainsKey");
            var enum_method = type.GetMethod("GetEnumerator");
            DictGetEnumeratorMethodMap[type] = enum_method;
            var enum_current_method = enum_method.ReturnType.GetMethod("get_Current");
            DictEnumeratorCurrentMethodMap[type] = enum_current_method;
            DictEnumeratorMoveNextMethodMap[type] = enum_method.ReturnType.GetMethod("MoveNext");
            var kv_pair_type = enum_current_method.ReturnType;
            DictEnumeratorKVPairKeyMethodMap[type] = kv_pair_type.GetMethod("get_Key", BindingFlags.Public | BindingFlags.Instance);
            DictEnumeratorKVPairValueMethodMap[type] = kv_pair_type.GetMethod("get_Value", BindingFlags.Public | BindingFlags.Instance);
        }

        private void CreateListMethodMaps() {
            CreateListMethodMap(typeof(List<Element>));
            CreateListMethodMap(typeof(List<string>));
        }

        private void CreateDictionaryMethodMaps() {
            CreateDictionaryMethodMap(typeof(Dictionary<string, string>));
        }

        private void CreateGenericMetamethods() {
            Lua.lua_pushcfunction(LuaStatePtr, LuaObjectFinalizer);
            Lua.lua_setfield(LuaStatePtr, -2, "__gc");
            Lua.lua_pushcfunction(LuaStatePtr, LuaObjectToString);
            Lua.lua_setfield(LuaStatePtr, -2, "__tostring");
        }

        private void CreateListMetatable(Type type, string name) {
            MetatableMap[name] = type;
            Lua.luaL_newmetatable(LuaStatePtr, name);
            CreateGenericMetamethods();
            Lua.lua_pushcfunction(LuaStatePtr, LuaListIndex);
            Lua.lua_setfield(LuaStatePtr, -2, "__index");
            Lua.lua_pushcfunction(LuaStatePtr, LuaListNewIndex);
            Lua.lua_setfield(LuaStatePtr, -2, "__newindex");
        }

        private void CreateDictionaryMetatable(Type type, string name) {
            MetatableMap[name] = type;
            Lua.luaL_newmetatable(LuaStatePtr, name);
            CreateGenericMetamethods();
            Lua.lua_pushcfunction(LuaStatePtr, LuaDictIndex);
            Lua.lua_setfield(LuaStatePtr, -2, "__index");
            Lua.lua_pushcfunction(LuaStatePtr, LuaDictNewIndex);
            Lua.lua_setfield(LuaStatePtr, -2, "__newindex");
        }

        private void CreateNodeMetatable(Type type, string name) {
            MetatableMap[name] = type;
            Lua.luaL_newmetatable(LuaStatePtr, name);
            CreateGenericMetamethods();

            var map = TypeFieldMap[type] = new Dictionary<string, FieldInfo>();
            var fields = type.GetFields();
            for (var i = 0; i < fields.Length; i++) {
                var field = fields[i];
                map[field.Name] = field;
            }

            Lua.lua_pushcfunction(LuaStatePtr, LuaNodeIndex);
            Lua.lua_setfield(LuaStatePtr, -2, "__index");
            Lua.lua_pushcfunction(LuaStatePtr, LuaNodeNewIndex);
            Lua.lua_setfield(LuaStatePtr, -2, "__newindex");
        }

        private string GetTypeMetatable(Type type) {
            if (type == typeof(Element)) return "Element";
            if (type == typeof(List<Element>)) return "List<NXML.Element>";
            if (type == typeof(List<string>)) return "List<string>";
            if (type == typeof(Dictionary<string, string>)) return "Dictionary<string, string>";

            throw new Exception($"Unsupported type: '{type}'");
        }

        private void CreateMetatables() {
            CreateNodeMetatable(typeof(Element), "Element");
            CreateListMetatable(typeof(List<Element>), "List<NXML.Element>");
            CreateListMetatable(typeof(List<string>), "List<string>");
            CreateDictionaryMetatable(typeof(Dictionary<string, string>), "Dictionary<string, string>");
        }
    }
}

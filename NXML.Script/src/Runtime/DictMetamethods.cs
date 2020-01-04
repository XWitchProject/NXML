using System;
using System.Collections.Generic;

namespace NXML.Script {
    public partial class NXMLRuntime {
        private static HashSet<string> SpecialDictIndices = new HashSet<string> {
            "count", "remove", "clear", "iter", "contains"
        };

        private int LuaDictIter(IntPtr state) {
            if (Lua.lua_type(state, -1) != LuaType.Function) throw new Exception($":iter must be given a function");
            var refid = ToReference(-2);
            var dict = ResolveReference(refid);

            var dict_type = dict.GetType();

            var enumerator = DictGetEnumeratorMethodMap[dict_type].Invoke(dict, new object[] { });
            var move_next = DictEnumeratorMoveNextMethodMap[dict_type];
            var current = DictEnumeratorCurrentMethodMap[dict_type];

            while ((bool)move_next.Invoke(enumerator, new object[] { })) {
                Lua.lua_pushvalue(state, -1);
                var cur_kv = current.Invoke(enumerator, null);
                var key = DictEnumeratorKVPairKeyMethodMap[dict_type].Invoke(cur_kv, new object[] { });
                var val = DictEnumeratorKVPairValueMethodMap[dict_type].Invoke(cur_kv, new object[] { });
                Push(key);
                Push(val);
                ProtCall(2, 0);
            }

            Lua.lua_pop(state, 2);

            return 0;
        }

        private int LuaDictClear(IntPtr state) {
            var refid = ToReference();
            var list = ResolveReference(refid);
            Lua.lua_pop(state, 1);

            ContainerClearMethodMap[list.GetType()].Invoke(list, null);

            return 0;
        } 

        private int LuaDictRemove(IntPtr state) {
            var idx = ToObject();
            Lua.lua_pop(state, 1);
            var refid = ToReference();
            var list = ResolveReference(refid);
            Lua.lua_pop(state, 1);

            ContainerRemoveMethodMap[list.GetType()].Invoke(list, new object[] { idx });

            return 0;
        }

        private int LuaDictContains(IntPtr state) {
            var valid = ToReference();
            var val = ResolveReference(valid);
            Lua.lua_pop(state, 1);
            var refid = ToReference();
            var list = ResolveReference(refid);
            Lua.lua_pop(state, 1);

            var result = (bool)ContainerContainsMethodMap[list.GetType()].Invoke(list, new object[] { val });
            Lua.lua_pushboolean(state, result);

            return 1;
        }

        private int LuaDictFieldIndex(IntPtr state) {
            var field_name = ToString(-1);
            Lua.lua_pop(state, 1);
            var refid = ToReference();
            var list = ResolveReference(refid);
            Lua.lua_pop(state, 1);

            if (field_name == "count") {
                var type = list.GetType();
                var count_method = ContainerCountMethodMap[type];
                var count = (int)count_method.Invoke(list, null);
                Lua.lua_pushinteger(state, (IntPtr)count);
            } else if (field_name == "remove") {
                Lua.lua_pushcfunction(state, LuaDictRemove);
            } else if (field_name == "clear") {
                Lua.lua_pushcfunction(state, LuaDictClear);
            } else if (field_name == "iter") {
                Lua.lua_pushcfunction(state, LuaDictIter);
            } else if (field_name == "contains") {
                Lua.lua_pushcfunction(state, LuaDictContains);
            } else {
                Lua.lua_pushnil(state);
            }

            return 1;
        }

        private int LuaDictIndex(IntPtr state) {
            if (Lua.lua_type(state, -1) == LuaType.String) {
                var str = ToString(-1);
                if (SpecialDictIndices.Contains(str)) return LuaDictFieldIndex(state);
            }
            var idx = ToObject();
            Lua.lua_pop(state, 1);
            var refid = ToReference();
            var dict = ResolveReference(refid);
            Lua.lua_pop(state, 1);

            var type = dict.GetType();
            var idx_type = idx.GetType();
            var contains = ContainerContainsMethodMap[type];
            if (!contains.GetParameters()[0].ParameterType.IsAssignableFrom(idx_type)) {
                Lua.lua_pushnil(state);
                return 1;
            }

            if (!(bool)contains.Invoke(dict, new object[] { idx })) {
                Lua.lua_pushnil(state);
                return 1;
            }

            var indexer = ContainerIndexMethodMap[type];

            Push(indexer.Invoke(dict, new object[] { idx }));

            return 1;
        }

        private int LuaDictNewIndex(IntPtr state) {
            if (Lua.lua_type(state, -1) == LuaType.String) {
                var str = ToString(-1);
                if (SpecialDictIndices.Contains(str)) {
                    PushString($"cannot assign to special dictionary function '{str}'");
                    Lua.lua_error(state);
                }
            }

            var val = ToObject();
            Lua.lua_pop(state, 1);
            var idx = ToObject();
            Lua.lua_pop(state, 1);
            var refid = ToReference();
            var dict = ResolveReference(refid);
            Lua.lua_pop(state, 1);

            var type = dict.GetType();
            var indexer = ContainerNewIndexMethodMap[type];

            indexer.Invoke(dict, new object[] { idx, val });
            return 0;
        }
    }
}

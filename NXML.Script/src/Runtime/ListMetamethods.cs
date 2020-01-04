using System;
namespace NXML.Script {
    public partial class NXMLRuntime {
        private int LuaListReverseIter(IntPtr state) {
            if (Lua.lua_type(state, -1) != LuaType.Function) throw new Exception($":reverse_iter must be given a function");
            var refid = ToReference(-2);
            var list = ResolveReference(refid);

            var count = (int)ContainerCountMethodMap[list.GetType()].Invoke(list, null);
            for (int i = count - 1; i >= 0; i--) {
                Lua.lua_pushvalue(state, -1);
                Lua.lua_pushinteger(state, (IntPtr)i + 1);
                var obj = ContainerIndexMethodMap[list.GetType()].Invoke(list, new object[] { i });
                PushObject(obj);
                ProtCall(2, 0);
            }

            Lua.lua_pop(state, 2);

            return 0;
        }

        private int LuaListIter(IntPtr state) {
            if (Lua.lua_type(state, -1) != LuaType.Function) throw new Exception($":iter must be given a function");
            var refid = ToReference(-2);
            var list = ResolveReference(refid);

            var count = (int)ContainerCountMethodMap[list.GetType()].Invoke(list, null);
            for (int i = 0; i < count; i++) {
                Lua.lua_pushvalue(state, -1);
                Lua.lua_pushinteger(state, (IntPtr)i + 1);
                var obj = ContainerIndexMethodMap[list.GetType()].Invoke(list, new object[] { i });
                PushObject(obj);
                ProtCall(2, 0);
            }

            Lua.lua_pop(state, 2);

            return 0;
        }

        private int LuaListClear(IntPtr state) {
            var refid = ToReference();
            var list = ResolveReference(refid);
            Lua.lua_pop(state, 1);

            ContainerClearMethodMap[list.GetType()].Invoke(list, null);

            return 0;
        }

        private int LuaListInsert(IntPtr state) {
            var valid = ToReference();
            var val = ResolveReference(valid);
            Lua.lua_pop(state, 1);
            var idx = (int)Lua.lua_tointeger(state, -1) - 1;
            if (Lua.lua_type(state, -1) != LuaType.Number) throw new Exception("Invalid index");
            Lua.lua_pop(state, 1);
            var refid = ToReference();
            var list = ResolveReference(refid);
            Lua.lua_pop(state, 1);

            ListInsertMethodMap[list.GetType()].Invoke(list, new object[] { idx, val });

            return 0;
        }

        private int LuaListRemove(IntPtr state) {
            var idx = (int)Lua.lua_tointeger(state, -1) - 1;
            if (Lua.lua_type(state, -1) != LuaType.Number) throw new Exception("Invalid index");
            Lua.lua_pop(state, 1);
            var refid = ToReference();
            var list = ResolveReference(refid);
            Lua.lua_pop(state, 1);

            ContainerRemoveMethodMap[list.GetType()].Invoke(list, new object[] { idx });

            return 0;
        }

        private int LuaListAdd(IntPtr state) {
            var valid = ToReference();
            var val = ResolveReference(valid);
            Lua.lua_pop(state, 1);
            var refid = ToReference();
            var list = ResolveReference(refid);
            Lua.lua_pop(state, 1);

            ContainerAddMethodMap[list.GetType()].Invoke(list, new object[] { val });

            return 0;
        }

        private int LuaListContains(IntPtr state) {
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

        private int LuaListFieldIndex(IntPtr state) {
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
            } else if (field_name == "add") {
                Lua.lua_pushcfunction(state, LuaListAdd);
            } else if (field_name == "remove") {
                Lua.lua_pushcfunction(state, LuaListRemove);
            } else if (field_name == "insert") {
                Lua.lua_pushcfunction(state, LuaListInsert);
            } else if (field_name == "clear") {
                Lua.lua_pushcfunction(state, LuaListClear);
            } else if (field_name == "iter") {
                Lua.lua_pushcfunction(state, LuaListIter);
            } else if (field_name == "reverse_iter") {
                Lua.lua_pushcfunction(state, LuaListReverseIter);
            } else if (field_name == "contains") {
                Lua.lua_pushcfunction(state, LuaListContains);
            } else {
                Lua.lua_pushnil(state);
            }

            return 1;
        }

        private int LuaListIndex(IntPtr state) {
            if (Lua.lua_type(state, -1) == LuaType.String) {
                return LuaListFieldIndex(state);
            }
            var idx = (int)Lua.lua_tointeger(state, -1) - 1;
            if (Lua.lua_type(state, -1) != LuaType.Number) throw new Exception("Invalid index");
            Lua.lua_pop(state, 1);
            var refid = ToReference();
            var list = ResolveReference(refid);
            Lua.lua_pop(state, 1);

            var type = list.GetType();
            var count_method = ContainerCountMethodMap[type];
            var count = (int)count_method.Invoke(list, null);
            if (idx < 0 || idx >= count) {
                Lua.lua_pushnil(state);
                return 1;
            }
            var indexer = ContainerIndexMethodMap[type];

            Push(indexer.Invoke(list, new object[] { idx }));

            return 1;
        }

        private int LuaListNewIndex(IntPtr state) {
            var valid = ToReference();
            var val = ResolveReference(valid);
            Lua.lua_pop(state, 1);
            if (Lua.lua_type(state, -1) != LuaType.Number) throw new Exception("Invalid index");
            var idx = (int)Lua.lua_tointeger(state, -1) - 1;
            Lua.lua_pop(state, 1);
            var refid = ToReference();
            var list = ResolveReference(refid);
            Lua.lua_pop(state, 1);

            var type = list.GetType();
            var count_method = ContainerCountMethodMap[type];
            var count = (int)count_method.Invoke(list, null);
            if (idx < 0 || idx >= count) {
                throw new Exception($"Invalid index: {idx}");
            }
            var indexer = ContainerNewIndexMethodMap[type];

            indexer.Invoke(list, new object[] { idx, val });
            return 0;
        }
    }
}

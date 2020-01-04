using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using NXML;

namespace NXML.Script {
    public partial class NXMLRuntime {
        public IntPtr LuaStatePtr;

        public NXMLRuntime() {
            LuaStatePtr = Lua.luaL_newstate();
            Lua.luaL_openlibs(LuaStatePtr);
            CreateListMethodMaps();
            CreateDictionaryMethodMaps();
            CreateMetatables();
            AddXMLFunctions();
        }

        private int LuaObjectToString(IntPtr state) {
            var refid = ToReference();
            var obj = ResolveReference(refid);
            PushString(obj.ToString());
            return 1;
        }

        private int LuaObjectFinalizer(IntPtr state) {
            RemoveReference(ToReference());
            return 0;
        }

        private int LuaNodeIndex(IntPtr state) {
            var key = ToString(-1);
            Lua.lua_pop(state, 1);
            var refid = ToReference();

            var node = ResolveReference(refid);

            if (key == "TextContent") {
                if (node is Element) {
                    PushString(((Element)node).TextContent);
                    return 1;
                }
            }

            if (TypeFieldMap.TryGetValue(node.GetType(), out Dictionary<string, FieldInfo> map)) {
                if (map.TryGetValue(key, out FieldInfo info)) {
                    Push(info.GetValue(node));
                    return 1;
                }
            }

            Lua.lua_pushnil(LuaStatePtr);
            return 1;
        }

        private int LuaNodeNewIndex(IntPtr state) {
            var val = ToObject();
            Lua.lua_pop(state, 1);
            var key = ToString(-1);
            Lua.lua_pop(state, 1);
            var refid = ToReference();

            var node = ResolveReference(refid);

            if (key == "TextContent") {
                if (node is Element) {
                    ((Element)node).TextContent = val?.ToString();
                    return 0;
                }
            }

            if (TypeFieldMap.TryGetValue(node.GetType(), out Dictionary<string, FieldInfo> map)) {
                if (map.TryGetValue(key, out FieldInfo info)) {
                    info.SetValue(node, val);
                    return 0;
                }
            }

            return 0;
        }

        public void ProtCall(int nargs, int nreturn) {
            var result = Lua.lua_pcall(LuaStatePtr, nargs, nreturn, 0);
            if (result != LuaResult.OK) {
                var msg = ToString(-1);
                throw new NXMLScriptException(msg);
            }
        }

        public void DoFile(string path) {
            var result = Lua.luaL_loadfile(LuaStatePtr, path);
            if (result != LuaResult.OK) {
                throw new NXMLScriptException(ToString(-1));
            }
            ProtCall(0, 0);
        }

        public void DoString(string str) {
            var result = Lua.luaL_loadstring(LuaStatePtr, str);
            if (result != LuaResult.OK) {
                throw new NXMLScriptException(ToString(-1));
            }
            ProtCall(0, 0);
        }
    }
}

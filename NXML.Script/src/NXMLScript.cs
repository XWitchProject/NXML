using System;
using NXML;
using System.Collections.Generic;

namespace NXML.Script {
    public class NXMLScript {
        public NXMLRuntime Runtime;

        public const string ELEMENT_FUNCTION = "Transform";

        public NXMLScript(NXMLRuntime runtime) {
            Runtime = runtime;
        }


        public void TransformElement(Element elem) {
            Lua.lua_getglobal(Runtime.LuaStatePtr, ELEMENT_FUNCTION);
            if (Lua.lua_type(Runtime.LuaStatePtr, -1) != LuaType.Function) {
                Lua.lua_pop(Runtime.LuaStatePtr, 1);
                return;
            }
            Runtime.PushObject(elem);
            Runtime.ProtCall(1, 0);
        }
    }
}

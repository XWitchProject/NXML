using System;
using System.IO;
using NXML;

namespace NXML.Script{
    public partial class NXMLRuntime {
        private int XMLNew(IntPtr state) {
            var s = ToString(-1);
            PushObject(new Element(s));
            return 1;
        }

        private void AddXMLFunctions() {
            Lua.lua_pushcfunction(LuaStatePtr, XMLNew);
            Lua.lua_setglobal(LuaStatePtr, "xmlnew");
        }
    }
}

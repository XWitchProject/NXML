using System;
using NXML;
using System.Collections.Generic;
using XTRuntime;

namespace NXML.Script {
    public class NXMLScript {
        public XTRuntime.XTRuntime Runtime;

        public const string ELEMENT_FUNCTION = "Transform";

        public NXMLScript(XTRuntime.XTRuntime runtime) {
            Runtime = runtime;
            CreateListMethodMaps();
            CreateDictionaryMethodMaps();
            CreateMetatables();
            AddXMLFunctions();
        }

        private void CreateMetatables() {
            Runtime.CreateNodeMetatable(typeof(Element), "Element");
            Runtime.RegisterSpecialIndexFunc(typeof(Element), "TextContent", (elem) => {
                Runtime.PushString(((Element)elem).TextContent);
                return 1;
            });
            Runtime.RegisterSpecialNewIndexFunc(typeof(Element), "TextContent", (elem, val) => {
                ((Element)elem).TextContent = val?.ToString();
                return 0;
            });

            Runtime.CreateListMetatable(typeof(List<Element>), "List<NXML.Element>");
            Runtime.CreateListMetatable(typeof(List<string>), "List<string>");
            Runtime.CreateDictionaryMetatable(typeof(Dictionary<string, string>), "Dictionary<string, string>");
        }

        private void CreateListMethodMaps() {
            Runtime.CreateListMethodMap(typeof(List<Element>));
            Runtime.CreateListMethodMap(typeof(List<string>));
        }

        private void CreateDictionaryMethodMaps() {
            Runtime.CreateDictionaryMethodMap(typeof(Dictionary<string, string>));
        }

        private int XMLNew(IntPtr state) {
            var s = Runtime.ToString(-1);
            Runtime.PushObject(new Element(s));
            return 1;
        }

        private void AddXMLFunctions() {
            Lua.lua_pushcfunction(Runtime.LuaStatePtr, XMLNew);
            Lua.lua_setglobal(Runtime.LuaStatePtr, "xmlnew");
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

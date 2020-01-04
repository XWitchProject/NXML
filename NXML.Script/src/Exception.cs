using System;
namespace NXML.Script {
    public class NXMLScriptException : Exception {
        public static string ResultToMessage(LuaResult r) {
            switch(r) {
            case LuaResult.ErrErr: return "Error in error handler";
            case LuaResult.ErrMem: return "Out of memory";
            case LuaResult.ErrRun: return "Runtime error";
            case LuaResult.ErrSyntax: return "Syntax error";
            default: return "?";
            }
        }

        public NXMLScriptException(string msg) : base(msg) {}
    }
}

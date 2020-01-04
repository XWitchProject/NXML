using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NXML {
    public class Element {
        public string Name;
        public Dictionary<string, string> Attributes;
        public List<Element> Children;
        internal StringBuilder TextContentBuilder;
        private string _TextContent;
        public bool HasTextContent {
            get { return (TextContentBuilder != null || _TextContent != null) && TextContent != ""; }
        }
        public string TextContent {
            get {
                if (TextContentBuilder == null) return "";
                if (_TextContent != null) return _TextContent;
                _TextContent = TextContentBuilder.ToString();
                TextContentBuilder = null;
                return _TextContent;
            }
            set {
                _TextContent = value;
            }
        }


        public Element(string name) {
            Name = name;
            Attributes = new Dictionary<string, string>();
            Children = new List<Element>();
        }

        internal void AddTextContent(string content) {
            if (TextContentBuilder == null) TextContentBuilder = new StringBuilder();
            else TextContentBuilder.Append(" ");
            TextContentBuilder.Append(content);
        }

        public string GetAttributeString(string name) {
            if (!Attributes.TryGetValue(name, out string val)) return null;
            return val;
        }

        public override string ToString() {
            var sw = new StringWriter();
            Write(new IndentAwareTextWriter(sw));
            return sw.ToString();
        }

        public void Write(IndentAwareTextWriter w) {
            w.Write("<");
            w.Write(Name);

            if (Attributes.Count > 0) w.Write(" ");

            var attr_idx = 0;
            foreach (var kv in Attributes) {
                w.Write(kv.Key);
                w.Write("=\"");
                w.Write(kv.Value);
                w.Write("\"");

                if (attr_idx != Attributes.Count - 1) w.Write(" ");

                attr_idx += 1;
            }

            if (Children.Count == 0 && !HasTextContent) {
                w.Write("/>");
                return;
            }

            w.Write(">");

            w.IncreaseIndent();
            w.WriteLine();

            if (HasTextContent) {
                w.Write(TextContent);
            }

            for (var i = 0; i < Children.Count; i++) {
                Children[i].Write(w);
                if (i != Children.Count - 1) w.WriteLine();
            }

            w.DecreaseIndent();
            w.WriteLine();
            w.Write("</");
            w.Write(Name);
            w.Write(">");
        }
    }
}

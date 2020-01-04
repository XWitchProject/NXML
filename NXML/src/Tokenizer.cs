using System;
using System.Collections.Generic;

namespace NXML {
    public enum TokenType {
        Unknown,
        OpenLess,
        CloseGreater,
        Slash,
        Equal,
        String,
        EOF
    }

    public struct Token {
        public TokenType Type;
        public string Value;

        public Token(TokenType type, string value) {
            Type = type;
            Value = value;
        }
    }

    public class Tokenizer {
        public static bool IsWhitespace(char c) {
            return c == ' ' || c == '\t' || c == '\n' || c == '\r';
        }

        public static bool IsPunctuationOrWhitespace(char c) {
            return IsWhitespace(c) || c == '<' || c == '>' || c == '=' || c == '/';
        }

        public Tokenizer(string data) {
            Data = data;
        }

        public string Data;
        public int CurrentIndex;

        public int CurrentLine = 1;
        public int CurrentColumn = 1;

        public bool EOF {
            get {
                return CurrentIndex >= Data.Length;
            }
        }

        public char CurChar {
            get {
                if (CurrentIndex >= Data.Length) return '\0';
                return Data[CurrentIndex];
            }
        }

        public void Move(int n = 1) {
            var prev_idx = CurrentIndex;
            CurrentIndex += n;
            if (CurrentIndex >= Data.Length) {
                CurrentIndex = Data.Length;
                return;
            }
            for (var i = prev_idx; i < CurrentIndex; i++) {
                if (Data[i] == '\n') {
                    CurrentLine += 1;
                    CurrentColumn = 1;
                } else {
                    CurrentColumn += 1;
                }
            }
        }

        public char Peek(int n = 1) {
            var idx = CurrentIndex + n;
            if (idx >= Data.Length) return '\0';

            return Data[idx];
        }

        public bool MatchString(string s) {
            for (var i = 0; i < s.Length; i++) {
                if (Peek(i) != s[i]) return false;
            }
            return true;
        }

        public void SkipWhitespace() {
            while (!EOF) {
                if (IsWhitespace(CurChar)) Move();
                else if (MatchString("<!--")) {
                    Move("<!--".Length);
                    while (!EOF && !MatchString("-->")) {
                        Move();
                    }

                    if (MatchString("-->")) {
                        Move("-->".Length);
                    }
                } else if (CurChar == '<' && Peek(1) == '!') {
                    Move(2);
                    while (!EOF && CurChar != '>') Move();
                    if (CurChar == '>') Move();
                } else if (MatchString("<?")) {
                    Move(2);
                    while (!EOF && !MatchString("?>")) Move();
                    if (MatchString("-->")) Move();
                } else {
                    break;
                }
            }
        }

        public string ReadQuotedString() {
            var start_idx = CurrentIndex;
            var len = 0;
            while (!EOF && CurChar != '"') {
                len += 1;
                Move();
            }
            Move(); // skip "
            return Data.Substring(start_idx, len);
        }

        public string ReadUnquotedString() {
            var start_idx = CurrentIndex - 1; // first char is Move()d
            var len = 1;
            while (!IsPunctuationOrWhitespace(CurChar)) {
                len += 1;
                Move();
            }
            return Data.Substring(start_idx, len);
        }

        public Token NextToken() {
            SkipWhitespace();

            if (EOF) return new Token(TokenType.EOF, "");

            var c = CurChar;
            Move();

            switch (c) {
            case '\0': return new Token(TokenType.EOF, "");
            case '<': return new Token(TokenType.OpenLess, "<");
            case '>': return new Token(TokenType.CloseGreater, ">");
            case '/': return new Token(TokenType.Slash, "/");
            case '=': return new Token(TokenType.Equal, "=");
            case '"': return new Token(TokenType.String, ReadQuotedString());
            default: return new Token(TokenType.String, ReadUnquotedString());
            }
        }
    }
}

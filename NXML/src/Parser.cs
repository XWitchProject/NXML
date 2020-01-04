using System;
using System.Collections.Generic;
using System.IO;

/*
 * The NXML Parser is heavily based on code from poro
 * https://github.com/gummikana/poro
 * 
 * The poro project is licensed under the Zlib license:
 * 
 * --------------------------------------------------------------------------
 * Copyright (c) 2010-2019 Petri Purho, Dennis Belfrage
 * Contributors: Martin Jonasson, Olli Harjola

 * This software is provided 'as-is', without any express or implied
 * warranty.  In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 * --------------------------------------------------------------------------
 */

namespace NXML {
    public class Parser {
        public enum ErrorType {
            NoClosingSymbolFound,
            NoOpeningSymbolFound,
            MismatchedClosingTag,
            MissingEqualsSign,
            MissingAttributeValue,
            MissingElementName
        }

        public class Error {
            public ErrorType Type;
            public string Message;
            public int Line;
            public int Column;

            public Error(Tokenizer tok, ErrorType type, string msg) {
                Line = tok.CurrentLine;
                Column = tok.CurrentColumn;
                Type = type;
                Message = msg;
            }

            public override string ToString() {
                return $"{Message} [{Line}:{Column}]";
            }
        }

        public static void DefaultErrorListener(Error err) {
            throw new NXMLException(err);
        }

        public Tokenizer Tokenizer;
        public List<Error> Errors;
        public Action<Error> ErrorListener;

        public Parser(Tokenizer tokenizer, bool lenient = true, Action<Error> custom_error_listener = null) {
            Tokenizer = tokenizer;
            Errors = new List<Error>();
            if (custom_error_listener != null && ErrorListener == null) ErrorListener = custom_error_listener;
            else if (custom_error_listener != null) ErrorListener += custom_error_listener;

            if (!lenient) ErrorListener = DefaultErrorListener;
        }

        public void ReportError(ErrorType type, string msg) {
            var err = new Error(Tokenizer, type, msg);
            Errors.Add(err);
            if (ErrorListener != null) {
                ErrorListener.Invoke(err);
            }
        }

        public void ParseAttribute(Element target, string name) {
            var tok = Tokenizer.NextToken();
            if (tok.Type == TokenType.Equal) {
                tok = Tokenizer.NextToken();
                if (tok.Type == TokenType.String) {
                    target.Attributes[name] = tok.Value;
                } else {
                    ReportError(ErrorType.MissingAttributeValue, $"parsing tag ({name}) - expected a \"string\"  after =, but none found");
                }
            } else {
                ReportError(ErrorType.MissingEqualsSign, $"parsing attribute ({name}) - expected '='");
            }
        }

        public Element ParseElement(bool skip_opening_tag = false) {
            Token tok;
            if (!skip_opening_tag) {
                tok = Tokenizer.NextToken();
                if (tok.Type != TokenType.OpenLess) ReportError(ErrorType.NoOpeningSymbolFound, "Couldn't find a '<' to start parsing with");
            }
            tok = Tokenizer.NextToken();
            if (tok.Type != TokenType.String) ReportError(ErrorType.MissingElementName, "Expected a name of the element after <");

            var elem = new Element(tok.Value);

            var self_closing = false;

            while (true) {
                tok = Tokenizer.NextToken();
                switch(tok.Type) {
                case TokenType.EOF: return elem;
                case TokenType.Slash:
                    if (Tokenizer.CurChar == '>') {
                        Tokenizer.Move();
                        self_closing = true;
                    }
                    goto break_loop;
                case TokenType.CloseGreater:
                    goto break_loop;
                case TokenType.String:
                    ParseAttribute(elem, tok.Value);
                    break;
                }
            }
        break_loop:

            if (self_closing) return elem;

            while (true) {
                tok = Tokenizer.NextToken();
                switch(tok.Type) {
                case TokenType.EOF: return elem;
                case TokenType.OpenLess:
                    if (Tokenizer.CurChar == '/') {
                        Tokenizer.Move();

                        var end_name = Tokenizer.NextToken();
                        if (end_name.Type == TokenType.String && end_name.Value == elem.Name) {
                            var close_greater = Tokenizer.NextToken();
                            if (close_greater.Type == TokenType.CloseGreater) {
                                return elem;
                            } else {
                                ReportError(ErrorType.NoClosingSymbolFound, $"No closing '>' found for ending element </{end_name.Value}");
                            }
                        } else {
                            ReportError(ErrorType.MismatchedClosingTag, $"Closing element is in wrong order. Expected '</{elem.Name}>', but instead got '{end_name.Value}'");
                        }
                        return elem;

                    } else {
                        elem.Children.Add(ParseElement(skip_opening_tag: true));
                    }
                    break;
                default: elem.AddTextContent(tok.Value); break;
                }
            }
        }
    }
}
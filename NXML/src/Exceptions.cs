using System;
namespace NXML {
    public class NXMLException : Exception {
        public Parser.Error Error;

        public NXMLException(Parser.Error err)
                        : base($"Failed parsing XML: {err}") {
            Error = err;
        }
    }
}

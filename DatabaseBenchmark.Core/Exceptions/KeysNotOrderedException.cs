using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Core.Exceptions
{
    /// <summary>
    /// Represents an error that occurs when a database does not read records ordered by key.
    /// </summary>
    public class KeysNotOrderedException : Exception
    {
        private string message;
        private Exception inner;

        public KeysNotOrderedException()
        {
        }

        public KeysNotOrderedException(string message)
        {
            this.message = message;
        }

        public KeysNotOrderedException(string message, Exception inner)
        {
            this.message = message;
            this.inner = inner;
        }

        public override string Message
        {
            get
            {
                return message;
            }
        }
    }
}

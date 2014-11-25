using System;

namespace Aucent.MAX.AXE.XBRLParser.Searching
{
    /// <summary>
    /// </summary>
    public class EventArgs<T> : EventArgs
    {
        private T value;
        /// <summary>
        /// </summary>
        public T Value
        {
            get { return value; }
            private set { this.value = value; }
        }

        /// <summary>
        /// </summary>
        public EventArgs(T value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// </summary>
    public class EventArgs<T1, T2> : EventArgs
    {
        private T1 value1;
        /// <summary>
        /// </summary>
        public T1 Value1
        {
            get { return value1; }
            private set { value1 = value; }
        }

        private T2 value2;
        /// <summary>
        /// </summary>
        public T2 Value2
        {
            get { return value2; }
            private set { value2 = value; }
        }

        /// <summary>
        /// </summary>
        public EventArgs(T1 value1, T2 value2)
        {
            Value1 = value1;
            Value2 = value2;
        }
    }

    /// <summary>
    /// </summary>
    public class EventArgs<T1, T2, T3> : EventArgs
    {
        private T1 value1;
        /// <summary>
        /// </summary>
        public T1 Value1
        {
            get { return value1; }
            private set { value1 = value; }
        }

        private T2 value2;
        /// <summary>
        /// </summary>
        public T2 Value2
        {
            get { return value2; }
            private set { value2 = value; }
        }

        private T3 value3;
        /// <summary>
        /// </summary>
        public T3 Value3
        {
            get { return value3; }
            private set { value3 = value; }
        }

        /// <summary>
        /// </summary>
        public EventArgs(T1 value1, T2 value2, T3 value3)
        {
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
        }
    }
}

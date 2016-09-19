using System;
using System.Collections;
using System.Collections.Generic;
using Should.Core.Assertions;

namespace Should.Core.Exceptions
{
    public class AssertActualExpectedException : AssertException
    {
        readonly string actual;
        readonly string differencePosition = "";
        readonly string expected;

        public AssertActualExpectedException(object expected,
                                             object actual,
                                             string userMessage)
            : this(expected, actual, userMessage, false) { }

        public AssertActualExpectedException(object expected,
                                             object actual,
                                             string userMessage,
                                             bool skipPositionCheck)
            : base(userMessage)
        {
            if (!skipPositionCheck)
            {
                var enumerableActual = actual as IEnumerable;
                var enumerableExpected = expected as IEnumerable;

                if (enumerableActual != null && enumerableExpected != null)
                {
                    var comparer = new EnumerableEqualityComparer();
                    comparer.Equals(enumerableActual, enumerableExpected);

                    differencePosition = "Position: First difference is at position " + comparer.Position + Environment.NewLine;
                }
            }

            this.actual = actual == null ? null : ConvertToString(actual);
            this.expected = expected == null ? null : ConvertToString(expected);

            if (actual != null &&
                expected != null &&
                actual.ToString() == expected.ToString() &&
                actual.GetType() != expected.GetType())
            {
                this.actual += $" ({actual.GetType().FullName})";
                this.expected += $" ({expected.GetType().FullName})";
            }
        }

        public string Actual
        {
            get { return actual; }
        }

        public string Expected
        {
            get { return expected; }
        }

        public override string Message
        {
            get
            {
                return string.Format("{0}{4}{1}Expected: {2}{4}Actual:   {3}",
                                     base.Message,
                                     differencePosition,
                                     FormatMultiLine(Expected ?? "(null)"),
                                     FormatMultiLine(Actual ?? "(null)"),
                                     Environment.NewLine);
            }
        }

        static string ConvertToString(object value)
        {
            var valueArray = value as Array;
            if (valueArray == null)
                return value.ToString();

            var valueStrings = new List<string>();

            foreach (object valueObject in valueArray)
                valueStrings.Add(valueObject == null ? "(null)" : valueObject.ToString());

            return value.GetType().FullName + " { " + String.Join(", ", valueStrings.ToArray()) + " }";
        }

        static string FormatMultiLine(string value)
        {
            return value.Replace(Environment.NewLine, Environment.NewLine + "          ");
        }
    }
}
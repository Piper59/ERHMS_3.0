﻿using ERHMS.Utility;
using NUnit.Framework;
using System;
using System.Linq;

namespace ERHMS.Test.Utility
{
    public class StringExtensionsTest
    {
        [Test]
        public void EqualsIgnoreCaseTest()
        {
            Assert.IsTrue("string".EqualsIgnoreCase("STRING"));
            Assert.IsTrue(StringExtensions.EqualsIgnoreCase(null, null));
        }

        [Test]
        public void ContainsIgnoreCaseTest()
        {
            Assert.IsTrue("Hello, world!".ContainsIgnoreCase("WORLD"));
            Assert.IsTrue("one two three".Split().ContainsIgnoreCase("THREE"));
        }

        [Test]
        public void ToPrintableTest()
        {
            char[] chars = Enumerable.Range(0, 0xffff)
                .Select(codePoint => Convert.ToChar(codePoint))
                .ToArray();
            char[] printableChars = new string(chars).ToPrintable().ToCharArray();
            Assert.AreEqual(95, printableChars.Length);
            Assert.AreEqual(0x20, printableChars.Min());
            Assert.AreEqual(0x7e, printableChars.Max());
        }

        [Test]
        public void MakeUniqueTest()
        {
            string[] values = new string[]
            {
                "test",
                "test (2)",
                "TEST (3)"
            };
            string format = "{0} ({1})";
            Assert.AreEqual("test", "test".MakeUnique(format, value => false));
            Assert.AreEqual("test (3)", "test".MakeUnique(format, value => values.Contains(value)));
            Assert.AreEqual("test (4)", "test".MakeUnique(format, value => values.ContainsIgnoreCase(value)));
        }
    }
}

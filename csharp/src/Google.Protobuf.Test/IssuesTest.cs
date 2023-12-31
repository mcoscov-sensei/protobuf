#region Copyright notice and license
// Protocol Buffers - Google's data interchange format
// Copyright 2008 Google Inc.  All rights reserved.
//
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file or at
// https://developers.google.com/open-source/licenses/bsd
#endregion

using Google.Protobuf.Reflection;
using UnitTest.Issues.TestProtos;
using NUnit.Framework;
using System.IO;
using static UnitTest.Issues.TestProtos.OneofMerging.Types;

namespace Google.Protobuf
{
    /// <summary>
    /// Tests for issues which aren't easily compartmentalized into other unit tests.
    /// </summary>
    public class IssuesTest
    {
        // Issue 45
        [Test]
        public void FieldCalledItem()
        {
            ItemField message = new ItemField { Item = 3 };
            FieldDescriptor field = ItemField.Descriptor.FindFieldByName("item");
            Assert.NotNull(field);
            Assert.AreEqual(3, (int)field.Accessor.GetValue(message));
        }

        [Test]
        public void ReservedNames()
        {
            var message = new ReservedNames { Types_ = 10, Descriptor_ = 20 };
            // Underscores aren't reflected in the JSON.
            Assert.AreEqual("{ \"types\": 10, \"descriptor\": 20 }", message.ToString());
        }

        [Test]
        public void JsonNameParseTest()
        {
            var settings = new JsonParser.Settings(10, TypeRegistry.FromFiles(UnittestIssuesReflection.Descriptor));
            var parser = new JsonParser(settings);

            // It is safe to use either original field name or explicitly specified json_name
            Assert.AreEqual(new TestJsonName { Name = "test", Description = "test2", Guid = "test3" },
                parser.Parse<TestJsonName>("{ \"name\": \"test\", \"desc\": \"test2\", \"guid\": \"test3\" }"));
        }

        [Test]
        public void JsonNameFormatTest()
        {
            var message = new TestJsonName { Name = "test", Description = "test2", Guid = "test3" };
            Assert.AreEqual("{ \"name\": \"test\", \"desc\": \"test2\", \"exid\": \"test3\" }",
                JsonFormatter.Default.Format(message));
        }

        [Test]
        public void OneofMerging()
        {
            var message1 = new OneofMerging { Nested = new Nested { X = 10 } };
            var message2 = new OneofMerging { Nested = new Nested { Y = 20 } };
            var expected = new OneofMerging { Nested = new Nested { X = 10, Y = 20 } };

            var merged = message1.Clone();
            merged.MergeFrom(message2);
            Assert.AreEqual(expected, merged);
        }

        // Check that a tag immediately followed by end of limit can still be read.
        [Test]
        public void CodedInputStream_LimitReachedRightAfterTag()
        {
            MemoryStream ms = new MemoryStream();
            var cos = new CodedOutputStream(ms);
            cos.WriteTag(11, WireFormat.WireType.Varint);
            Assert.AreEqual(1, cos.Position);
            cos.WriteString("some extra padding");  // ensure is currentLimit distinct from the end of the buffer.
            cos.Flush();

            var cis = new CodedInputStream(ms.ToArray());
            cis.PushLimit(1);  // make sure we reach the limit right after reading the tag.

            // we still must read the tag correctly, even though the tag is at the very end of our limited input
            // (which is a corner case and will most likely result in an error when trying to read value of the field
            // described by this tag, but it would be a logical error not to read the tag that's actually present).
            // See https://github.com/protocolbuffers/protobuf/pull/7289
            cis.AssertNextTag(WireFormat.MakeTag(11, WireFormat.WireType.Varint));
        }

        [Test]
        public void NoneFieldInOneof()
        {
            var message = new OneofWithNoneField();
            var emptyHashCode = message.GetHashCode();
            Assert.AreEqual(OneofWithNoneField.TestOneofCase.None, message.TestCase);
            message.None = "test";
            Assert.AreEqual(OneofWithNoneField.TestOneofCase.None_, message.TestCase);
            Assert.AreNotEqual(emptyHashCode, message.GetHashCode());

            var bytes = message.ToByteArray();
            var parsed = OneofWithNoneField.Parser.ParseFrom(bytes);
            Assert.AreEqual(message, parsed);
            Assert.AreEqual("test", parsed.None);
        }
    }
}

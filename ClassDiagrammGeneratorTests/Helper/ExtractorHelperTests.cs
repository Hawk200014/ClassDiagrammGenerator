using ClassDiagrammGenerator.Helper;
using ClassDiagrammGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ClassDiagrammGeneratorTests.Helper
{

    public class ExtractorHelperTests
    {
        #region ExtractClass

        [Fact]
        public void ExtractClass_SimpleClass_ParsesNameAndNamespace()
        {
            var lines = new[]
            {
                "public class MyClass {",
                "}"
            };
            int index = 0;
            var model = ExtractorHelper.ExtractClass(ref index, lines, "MyNamespace");

            Assert.Equal("MyClass", model.Name);
            Assert.Equal("MyNamespace", model.Namespace);
            Assert.Equal(EAccessmodifier.Public, model.AccessModifier);
            Assert.Empty(model.BaseClasses);
            Assert.Empty(model.Interfaces);
        }

        [Fact]
        public void ExtractClass_ClassWithInheritanceAndInterfaces_ParsesCorrectly()
        {
            var lines = new[]
            {
                "internal class DerivedClass : BaseClass, IDisposable, ICloneable {",
                "}"
            };
            int index = 0;
            var model = ExtractorHelper.ExtractClass(ref index, lines, "TestNS");

            Assert.Equal("DerivedClass", model.Name);
            Assert.Equal(EAccessmodifier.Internal, model.AccessModifier);
            Assert.Single(model.BaseClasses);
            Assert.Equal("BaseClass", model.BaseClasses[0]);
            Assert.Equal(2, model.Interfaces.Count);
            Assert.Contains("IDisposable", model.Interfaces);
            Assert.Contains("ICloneable", model.Interfaces);
        }

        [Fact]
        public void ExtractClass_ClassWithProperties_ParsesProperties()
        {
            var lines = new[]
            {
                "public class WithProps {",
                "public int Id { get; set; }",
                "private string Name { get; set; }",
                "}"
            };
            int index = 0;
            var model = ExtractorHelper.ExtractClass(ref index, lines, "NS");

            Assert.Equal(2, model.Properties.Count);
            Assert.Contains(model.Properties, p => p.Name.Contains("Id"));
            Assert.Contains(model.Properties, p => p.Name.Contains("Name"));
        }

        [Fact]
        public void ExtractClass_ClassWithMethods_ParsesMethods()
        {
            var lines = new[]
            {
                "public class WithMethods {",
                "public void Foo() { }",
                "private int Bar(int x) { return x; }",
                "}"
            };
            int index = 0;
            var model = ExtractorHelper.ExtractClass(ref index, lines, "NS");

            Assert.True(model.Methods.Count == 2);
            Assert.Contains(model.Methods, m => m.Name == "Foo");
            Assert.Contains(model.Methods, m => m.Name == "Bar");
        }

        [Fact]
        public void ExtractClass_IgnoresCommentsAndEmptyLines()
        {
            var lines = new[]
            {
                "public class Commented {",
                "// This is a comment",
                "",
                "public int Value { get; set; } // property",
                "/* block comment",
                "   still comment */",
                "public void Do() { }",
                "}"
            };
            int index = 0;
            var model = ExtractorHelper.ExtractClass(ref index, lines, "NS");

            Assert.Single(model.Properties);
            Assert.Single(model.Methods);
        }

        [Fact]
        public void ExtractClass_ParsesStaticAndReadonlyProperties()
        {
            var lines = new[]
            {
                "public class StaticReadonlyProps {",
                "public static int Count { get; set; }",
                "public readonly string Name { get; set; }",
                "}"
            };
            int index = 0;
            var model = ExtractorHelper.ExtractClass(ref index, lines, "NS");

            Assert.Equal(2, model.Properties.Count);
            Assert.Contains(model.Properties, p => p.Name.Contains("Count"));
            Assert.Contains(model.Properties, p => p.Name.Contains("Name"));
        }

        [Fact]
        public void ExtractClass_ParsesMultiLineMethodSignature()
        {
            var lines = new[]
            {
                "public class MultiLineMethod {",
                "public void DoSomething(",
                "   int x,",
                "   string y",
                ") { }",
                "}"
            };
            int index = 0;
            var model = ExtractorHelper.ExtractClass(ref index, lines, "NS");

            Assert.Contains(model.Methods, m => m.Name == "DoSomething");
        }

        [Fact]
        public void ExtractClass_ParsesMultiLineProperty()
        {
            var lines = new[]
            {
                "public class MultiLineProp {",
                "public int Number",
                "{",
                "    get; set;",
                "}",
                "}"
            };
            int index = 0;
            var model = ExtractorHelper.ExtractClass(ref index, lines, "NS");

            Assert.Contains(model.Properties, p => p.Name.Contains("Number"));
        }
        #endregion

        #region ExtractEnum

        [Theory]
        [InlineData("public enum Color", EAccessmodifier.Public)]
        [InlineData("private enum Color", EAccessmodifier.Private)]
        [InlineData("protected internal enum Color", EAccessmodifier.ProtectedInternal)]
        [InlineData("private protected enum Color", EAccessmodifier.PrivateProtected)]
        [InlineData("protected enum Color", EAccessmodifier.Protected)]
        [InlineData("enum Color", EAccessmodifier.Internal)]
        public void ExtractEnum_ParsesAccessModifierAndName(string header, EAccessmodifier expectedAccess)
        {
            var lines = new[]
            {
                header,
                "{",
                "    Red,",
                "    Green,",
                "    Blue",
                "}"
            };
            int index = 0;
            var model = ExtractorHelper.ExtractEnum(ref index, lines, "TestNamespace");
            Assert.Equal("Color", model.Name);
            Assert.Equal("TestNamespace", model.Namespace);
            Assert.Equal(expectedAccess, model.AccessModifier);
            Assert.Equal(new[] { "Red", "Green", "Blue" }, model.Values);
        }

        [Fact]
        public void ExtractEnum_ParsesValuesWithCommentsAndWhitespace()
        {
            var lines = new[]
            {
                "public enum Status",
                "{",
                "    Active, // active status",
                "    Inactive , // not active",
                "    Pending // waiting",
                "}"
            };
            int index = 0;
            var model = ExtractorHelper.ExtractEnum(ref index, lines, "NS");
            Assert.Equal(new[] { "Active", "Inactive", "Pending" }, model.Values);
        }

        [Fact]
        public void ExtractEnum_IgnoresEmptyLinesAndBraces()
        {
            var lines = new[]
            {
                "enum EmptyTest",
                "{",
                "",
                "    // comment",
                "    Value1,",
                "    {",
                "    Value2",
                "}"
            };
            int index = 0;
            var model = ExtractorHelper.ExtractEnum(ref index, lines, "NS2");
            Assert.Equal(new[] { "Value1", "Value2" }, model.Values);
        }

        [Fact]
        public void ExtractEnum_UnknownEnumNameIfMalformed()
        {
            var lines = new[]
            {
                "public enum", // missing name
                "{",
                "    A,",
                "    B",
                "}"
            };
            int index = 0;
            var model = ExtractorHelper.ExtractEnum(ref index, lines, "NS3");
            Assert.Equal("UnknownEnum", model.Name);
            Assert.Equal(new[] { "A", "B" }, model.Values);
        }

        #endregion

        #region ExtractInterface

        [Theory]
        [InlineData("public interface IFoo", EAccessmodifier.Public)]
        [InlineData("private interface IFoo", EAccessmodifier.Private)]
        [InlineData("protected internal interface IFoo", EAccessmodifier.ProtectedInternal)]
        [InlineData("private protected interface IFoo", EAccessmodifier.PrivateProtected)]
        [InlineData("protected interface IFoo", EAccessmodifier.Protected)]
        [InlineData("interface IFoo", EAccessmodifier.Internal)]
        public void ExtractInterface_ParsesAccessModifierAndName(string header, EAccessmodifier expectedAccess)
        {
            var lines = new[]
            {
                header,
                "{",
                "    void Bar();",
                "}"
            };
            int index = 0;
            var model = ExtractorHelper.ExtractInterface(ref index, lines, "TestNS");
            Assert.Equal("IFoo", model.Name);
            Assert.Equal("TestNS", model.Namespace);
            Assert.Equal(expectedAccess, model.AccessModifier);
            Assert.Single(model.Methods);
            Assert.Equal("Bar", model.Methods[0].Name);
            Assert.Equal("void", model.Methods[0].ReturnType);
        }

        [Fact]
        public void ExtractInterface_ParsesMultipleMethods()
        {
            var lines = new[]
            {
                "public interface ITest",
                "{",
                "    void Foo();",
                "    int Bar(string s);",
                "    // comment",
                "    bool Baz(int x, int y);",
                "}"
            };
            int index = 0;
            var model = ExtractorHelper.ExtractInterface(ref index, lines, "NS");
            Assert.Equal(3, model.Methods.Count);

            Assert.Contains(model.Methods, m => m.Name == "Foo" && m.ReturnType == "void");
            Assert.Contains(model.Methods, m => m.Name == "Bar" && m.ReturnType == "int" && m.Parameters.SequenceEqual(new[] { "string s" }));
            Assert.Contains(model.Methods, m => m.Name == "Baz" && m.ReturnType == "bool" && m.Parameters.SequenceEqual(new[] { "int x", "int y" }));
        }

        [Fact]
        public void ExtractInterface_IgnoresEmptyLinesAndBraces()
        {
            var lines = new[]
            {
                "interface IEmpty",
                "{",
                "",
                "    // comment",
                "    void DoWork();",
                "    {",
                "    void DoMore();",
                "}"
            };
            int index = 0;
            var model = ExtractorHelper.ExtractInterface(ref index, lines, "NS2");
            Assert.Equal(2, model.Methods.Count);
            Assert.Equal("DoWork", model.Methods[0].Name);
            Assert.Equal("DoMore", model.Methods[1].Name);
        }

        [Fact]
        public void ExtractInterface_UnknownInterfaceNameIfMalformed()
        {
            var lines = new[]
            {
                "public interface", // missing name
                "{",
                "    void A();",
                "    void B();",
                "}"
            };
            int index = 0;
            var model = ExtractorHelper.ExtractInterface(ref index, lines, "NS3");
            Assert.Equal("UnknownInterface", model.Name);
            Assert.Equal(2, model.Methods.Count);
            Assert.Equal("A", model.Methods[0].Name);
            Assert.Equal("B", model.Methods[1].Name);
        }

        #endregion

        #region ExractBracketContent
        [Fact]
        public void GetContentOfBrackets_ReturnsEmptyList_WhenNoLines()
        {
            // Arrange
            var lines = new string[0];

            // Act
            var result = ExtractorHelper.GetContentOfBrackets(0, lines);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetContentOfBrackets_ReturnsAllLines_WhenNoBrackets()
        {
            // Arrange
            var lines = new[] { "line1", "line2", "line3" };

            // Act
            var result = ExtractorHelper.GetContentOfBrackets(0, lines);

            // Assert
            Assert.Equal(lines, result);
        }

        [Fact]
        public void GetContentOfBrackets_StopsAtMatchingClosingBracket()
        {
            // Arrange
            var lines = new[]
            {
                "{",
                "int x = 1;",
                "if (x > 0) {",
                "x++;",
                "}",
                "}",
                "outside"
            };

            // Act
            var result = ExtractorHelper.GetContentOfBrackets(1, lines);

            // Assert
            // Should include all lines up to and including the line with the last closing bracket
            Assert.Equal(new List<string>
            {
                "{",
                "int x = 1;",
                "if (x > 0) {",
                "x++;",
                "}",
                "}"
            }, result);
        }

        [Fact]
        public void GetContentOfBrackets_HandlesNestedBrackets()
        {
            // Arrange
            var lines = new[]
            {
                "{",
                "if (true) {",
                "doSomething();",
                "}",
                "}"
            };

            // Act
            var result = ExtractorHelper.GetContentOfBrackets(1, lines);

            // Assert
            Assert.Equal(new List<string>
            {
                "{",
                "if (true) {",
                "doSomething();",
                "}",
                "}"
            }, result);
        }

        [Fact]
        public void GetContentOfBrackets_StartsAtGivenLine()
        {
            // Arrange
            var lines = new[]
            {
                "// comment",
                "{",
                "code;",
                "}"
            };

            // Act
            var result = ExtractorHelper.GetContentOfBrackets(1, lines, 1);

            // Assert
            Assert.Equal(new List<string>
            {
                "{",
                "code;",
                "}"
            }, result);
        }
        #endregion
    }
}


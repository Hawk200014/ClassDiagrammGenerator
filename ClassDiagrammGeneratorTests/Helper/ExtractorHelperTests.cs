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

        [Theory]
        [InlineData("public class MyClass", EAccessmodifier.Public)]
        [InlineData("private class MyClass", EAccessmodifier.Private)]
        [InlineData("protected internal class MyClass", EAccessmodifier.ProtectedInternal)]
        [InlineData("private protected class MyClass", EAccessmodifier.PrivateProtected)]
        [InlineData("protected class MyClass", EAccessmodifier.Protected)]
        [InlineData("class MyClass", EAccessmodifier.Internal)]
        public void ExtractClass_ParsesAccessModifierAndName(string header, EAccessmodifier expectedAccess)
        {
            var lines = new[]
            {
                header,
                "{",
                "}"
            };
            int index = 0;
            var model = ExtractorHelper.ExtractClass(ref index, lines, "TestNS");
            Assert.Equal("MyClass", model.Name);
            Assert.Equal("TestNS", model.Namespace);
            Assert.Equal(expectedAccess, model.AccessModifier);
        }

        [Fact]
        public void ExtractClass_ParsesBaseClassAndInterfaces()
        {
            var lines = new[]
            {
                "public class Derived : BaseClass, IFoo, IBar",
                "{",
                "}"
            };
            int index = 0;
            var model = ExtractorHelper.ExtractClass(ref index, lines, "NS");
            Assert.Equal(new[] { "BaseClass" }, model.BaseClasses);
            Assert.Equal(new[] { "IFoo", "IBar" }, model.Interfaces);
        }

        [Fact]
        public void ExtractClass_ParsesProperties()
        {
            var lines = new[]
            {
                "public class TestClass",
                "{",
                "    public int Id { get; set; }",
                "    private string Name { get; set; }",
                "    protected static readonly bool IsReady { get; set; }",
                "}"
            };
            int index = 0;
            var model = ExtractorHelper.ExtractClass(ref index, lines, "NS");
            Assert.Equal(3, model.Properties.Count);

            var idProp = model.Properties.FirstOrDefault(p => p.Name == "Id");
            Assert.NotNull(idProp);
            Assert.Equal("int", idProp.Type);
            Assert.Equal(EAccessmodifier.Public, idProp.AccessModifier);
            Assert.False(idProp.IsReadOnly);
            Assert.False(idProp.IsStatic);

            var nameProp = model.Properties.FirstOrDefault(p => p.Name == "Name");
            Assert.NotNull(nameProp);
            Assert.Equal("string", nameProp.Type);
            Assert.Equal(EAccessmodifier.Private, nameProp.AccessModifier);

            var isReadyProp = model.Properties.FirstOrDefault(p => p.Name == "IsReady");
            Assert.NotNull(isReadyProp);
            Assert.Equal("bool", isReadyProp.Type);
            Assert.Equal(EAccessmodifier.Protected, isReadyProp.AccessModifier);
            Assert.True(isReadyProp.IsReadOnly);
            Assert.True(isReadyProp.IsStatic);
        }

        [Fact]
        public void ExtractClass_ParsesMethods()
        {
            var lines = new[]
            {
                "public class TestClass",
                "{",
                "    public void Foo() {",
                "    private int Bar(string s) {",
                "    protected static bool Baz(int x, int y) {",
                "}"
            };
            int index = 0;
            var model = ExtractorHelper.ExtractClass(ref index, lines, "NS");
            Assert.Equal(3, model.Methods.Count);

            var foo = model.Methods.FirstOrDefault(m => m.Name == "Foo");
            Assert.NotNull(foo);
            Assert.Equal("void", foo.ReturnType);
            Assert.Equal(EAccessmodifier.Public, foo.AccessModifier);
            Assert.Empty(foo.Parameters);

            var bar = model.Methods.FirstOrDefault(m => m.Name == "Bar");
            Assert.NotNull(bar);
            Assert.Equal("int", bar.ReturnType);
            Assert.Equal(EAccessmodifier.Private, bar.AccessModifier);
            Assert.Single(bar.Parameters);
            Assert.Equal("string s", bar.Parameters[0]);

            var baz = model.Methods.FirstOrDefault(m => m.Name == "Baz");
            Assert.NotNull(baz);
            Assert.Equal("bool", baz.ReturnType);
            Assert.Equal(EAccessmodifier.Protected, baz.AccessModifier);
            Assert.Equal(2, baz.Parameters.Count);
            Assert.Equal("int x", baz.Parameters[0]);
            Assert.Equal("int y", baz.Parameters[1]);
        }

        [Fact]
        public void ExtractClass_HandlesUnknownClassName()
        {
            var lines = new[]
            {
                "public class", // missing name
                "{",
                "}"
            };
            int index = 0;
            var model = ExtractorHelper.ExtractClass(ref index, lines, "NS");
            Assert.Equal("UnknownClass", model.Name);
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
    }
}


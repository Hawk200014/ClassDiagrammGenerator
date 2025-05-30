using System;
using ClassDiagrammGenerator.Models;
using ClassDiagrammGenerator.ViewModels;
using Xunit;

namespace ClassDiagrammGenerator.Tests.ViewModels
{
    public class MainViewModelTests
    {
        private MainViewModel CreateViewModel() => new MainViewModel();

        [Theory]
        [InlineData("public enum Color", EAccessmodifier.Public)]
        [InlineData("private enum Color", EAccessmodifier.Private)]
        [InlineData("protected internal enum Color", EAccessmodifier.ProtectedInternal)]
        [InlineData("private protected enum Color", EAccessmodifier.PrivateProtected)]
        [InlineData("protected enum Color", EAccessmodifier.Protected)]
        [InlineData("enum Color", EAccessmodifier.Internal)]
        public void ExtractEnum_ParsesAccessModifierAndName(string header, EAccessmodifier expectedAccess)
        {
            // Arrange
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
            var vm = CreateViewModel();

            // Act
            var enumModel = InvokeExtractEnum(vm, ref index, lines, "TestNamespace");

            // Assert
            Assert.Equal("Color", enumModel.Name);
            Assert.Equal("TestNamespace", enumModel.Namespace);
            Assert.Equal(expectedAccess, enumModel.AccessModifier);
            Assert.Equal(new[] { "Red", "Green", "Blue" }, enumModel.Values);
        }

        [Fact]
        public void ExtractEnum_ParsesValuesWithCommentsAndWhitespace()
        {
            // Arrange
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
            var vm = CreateViewModel();

            // Act
            var enumModel = InvokeExtractEnum(vm, ref index, lines, "NS");

            // Assert
            Assert.Equal(new[] { "Active", "Inactive", "Pending" }, enumModel.Values);
        }

        [Fact]
        public void ExtractEnum_IgnoresEmptyLinesAndBraces()
        {
            // Arrange
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
            var vm = CreateViewModel();

            // Act
            var enumModel = InvokeExtractEnum(vm, ref index, lines, "NS2");

            // Assert
            Assert.Equal(new[] { "Value1", "Value2" }, enumModel.Values);
        }

        [Fact]
        public void ExtractEnum_UnknownEnumNameIfMalformed()
        {
            // Arrange
            var lines = new[]
            {
                "public enum", // missing name
                "{",
                "    A,",
                "    B",
                "}"
            };
            int index = 0;
            var vm = CreateViewModel();

            // Act
            var enumModel = InvokeExtractEnum(vm, ref index, lines, "NS3");

            // Assert
            Assert.Equal("UnknownEnum", enumModel.Name);
            Assert.Equal(new[] { "A", "B" }, enumModel.Values);
        }

        // Helper to invoke the private ExtractEnum method using reflection
        private EnumModel InvokeExtractEnum(MainViewModel vm, ref int index, string[] lines, string ns)
        {
            var method = typeof(MainViewModel).GetMethod("ExtractEnum", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            object[] parameters = { index, lines, ns };
            var result = (EnumModel)method.Invoke(vm, parameters);
            index = (int)parameters[0]; // update ref index
            return result;
        }
    }
}
using ClassDiagrammGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassDiagrammGenerator.Helper
{
    public class ExtractorHelper
    {
        public static InterfaceModel ExtractInterface(ref int index, string[] lines, string currentNamespace)
        {
            string line = lines[index].Trim();
            // Extract access modifier
            EAccessmodifier accessModifier = EAccessmodifier.Internal; // default
            if (line.StartsWith("protected internal ")) accessModifier = EAccessmodifier.ProtectedInternal;
            else if (line.StartsWith("private protected ")) accessModifier = EAccessmodifier.PrivateProtected;
            else if (line.StartsWith("public ")) accessModifier = EAccessmodifier.Public;
            else if (line.StartsWith("private ")) accessModifier = EAccessmodifier.Private;


            else if (line.StartsWith("protected ")) accessModifier = EAccessmodifier.Protected;
            // Extract name
            string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            int interfaceIndex = Array.IndexOf(parts, "interface");
            string interfaceName = (interfaceIndex >= 0 && interfaceIndex < parts.Length - 1) ? parts[interfaceIndex + 1] : "UnknownInterface";
            // Create model
            // Fix for CS1729: Modify the creation of InterfaceModel to use property initialization instead of a non-existent constructor.
            // Fix for IDE0090: Simplify the 'new' expression by directly initializing properties.

            InterfaceModel interfaceModel = new InterfaceModel
            {
                Name = interfaceName,
                Namespace = currentNamespace,
                AccessModifier = accessModifier
            };
            // Parse methods and properties (assume they are in the next lines until '}')
            index++; // move to the next line after 'interface ...'
            while (index < lines.Length && !lines[index].Contains("}"))
            {
                string methodLine = lines[index].Trim();
                if (!string.IsNullOrEmpty(methodLine) && !methodLine.StartsWith("//") && methodLine != "{")
                {
                    int commentIndex = methodLine.IndexOf("//");
                    string methodSignature = commentIndex >= 0 ? methodLine.Substring(0, commentIndex).TrimEnd() : methodLine.TrimEnd();
                    if (methodSignature.EndsWith(","))
                        methodSignature = methodSignature.Substring(0, methodSignature.Length - 1).TrimEnd();
                    if (!string.IsNullOrEmpty(methodSignature))
                        interfaceModel.AddMethod(methodSignature);
                }
                index++;
            }
            return interfaceModel;
        }

        public static EnumModel ExtractEnum(ref int index, string[] lines, string currentNamespace)
        {
            string line = lines[index].Trim();
            // Extract access modifier
            EAccessmodifier accessModifier = EAccessmodifier.Internal; // default
            if (line.StartsWith("protected internal ")) accessModifier = EAccessmodifier.ProtectedInternal;
            else if (line.StartsWith("private protected ")) accessModifier = EAccessmodifier.PrivateProtected;
            else if (line.StartsWith("public ")) accessModifier = EAccessmodifier.Public;
            else if (line.StartsWith("private ")) accessModifier = EAccessmodifier.Private;

            else if (line.StartsWith("protected ")) accessModifier = EAccessmodifier.Protected;

            // Extract name
            string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            int enumIndex = Array.IndexOf(parts, "enum");
            string enumName = (enumIndex >= 0 && enumIndex < parts.Length - 1) ? parts[enumIndex + 1] : "UnknownEnum";

            // Create model
            EnumModel enumModel = new EnumModel(enumName, currentNamespace, accessModifier);

            // Parse values (assume values are in the next lines until '}')
            index++; // move to the next line after 'enum ...'
            while (index < lines.Length && !lines[index].Contains("}"))
            {
                string valueLine = lines[index].Trim();
                if (!string.IsNullOrEmpty(valueLine) && !valueLine.StartsWith("//") && valueLine != "{")
                {
                    // Remove trailing commas and comments
                    string value = valueLine.Split(new[] { ',', '/' }, 2)[0].Trim();
                    if (!string.IsNullOrEmpty(value))
                        enumModel.AddValue(value);
                }
                index++;
            }
            return enumModel;
        }

        public static ClassModel ExtractClass(ref int index, string[] lines, string currentNamespace)
        {
            string line = lines[index].Trim();

            // Extract access modifier
            EAccessmodifier accessModifier = EAccessmodifier.Internal; // default
            if (line.StartsWith("protected internal ")) accessModifier = EAccessmodifier.ProtectedInternal;
            else if (line.StartsWith("private protected ")) accessModifier = EAccessmodifier.PrivateProtected;
            else if (line.StartsWith("public ")) accessModifier = EAccessmodifier.Public;
            else if (line.StartsWith("private ")) accessModifier = EAccessmodifier.Private;
            else if (line.StartsWith("protected ")) accessModifier = EAccessmodifier.Protected;

            // Extract class name and base types/interfaces
            string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            int classIndex = Array.IndexOf(parts, "class");
            string className = (classIndex >= 0 && classIndex < parts.Length - 1) ? parts[classIndex + 1] : "UnknownClass";

            // Check for inheritance/interfaces (after ':')
            List<string> baseClasses = new();
            List<string> interfaces = new();
            int colonIndex = line.IndexOf(':');
            if (colonIndex != -1)
            {
                var inheritancePart = line.Substring(colonIndex + 1).Trim();
                var types = inheritancePart.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var type in types)
                {
                    // Simple heuristic: assume first is base class, rest are interfaces
                    if (baseClasses.Count == 0)
                        baseClasses.Add(type);
                    else
                        interfaces.Add(type);
                }
            }

            ClassModel classModel = new ClassModel
            {
                Name = className,
                Namespace = currentNamespace,
                AccessModifier = accessModifier,
                BaseClasses = baseClasses,
                Interfaces = interfaces
            };

            int openedBrackets = 0;

            // Parse properties and methods (assume they are in the next lines until '}')
            if (lines[index].Contains("{"))
            {
                openedBrackets++;
            }
            index++; // move to the next line after 'class ...'
            if (lines[index].Trim().StartsWith("{"))
            {
                openedBrackets++;
                index++;
            }
            while (index < lines.Length && !(openedBrackets == 0))
            {
                string memberLine = lines[index].Trim();

                if (memberLine == "{")
                {
                    openedBrackets++;
                }
                if(memberLine == "}")
                {
                    openedBrackets--;
                }


                if (string.IsNullOrEmpty(memberLine) || memberLine.StartsWith("//") || memberLine == "{")
                {
                    
                    index++;
                    continue;
                }

                // Simple property detection: [access] [type] [name] { get; set; }
                if (memberLine.Contains("{ get;") && memberLine.Contains("set; }"))
                {
                    // Example: public int MyProperty { get; set; }
                    var tokens = memberLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    int typeIndex = 0;
                    EAccessmodifier propAccess = EAccessmodifier.Private;
                    if (tokens[0] == "public") { propAccess = EAccessmodifier.Public; typeIndex = 1; }
                    else if (tokens[0] == "private") { propAccess = EAccessmodifier.Private; typeIndex = 1; }
                    else if (tokens[0] == "protected") { propAccess = EAccessmodifier.Protected; typeIndex = 1; }
                    else if (tokens[0] == "internal") { propAccess = EAccessmodifier.Internal; typeIndex = 1; }

                    bool isStatic = false;
                    if (memberLine.Contains("static"))
                    {
                        isStatic = true;
                        typeIndex++;
                    }

                    bool isReadonly = false;
                    if (memberLine.Contains("readonly"))
                    {
                        isReadonly = true;
                        typeIndex++;
                    }

                    string type = tokens[typeIndex];
                    string name = tokens[typeIndex + 1];

                    classModel.Properties.Add(new PropertyModel(name, type, propAccess, isReadonly, isStatic));
                }

                // Simple method detection: [access] [returnType] [name](...)
                else if (memberLine.Contains("(") && memberLine.Contains(")") && memberLine.EndsWith("{"))
                {
                    // Example: public void MyMethod(int x) {
                    var tokens = memberLine.Split(new[] { ' ', '\t', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                    int typeIndex = 0;
                    EAccessmodifier methodAccess = EAccessmodifier.Private;
                    if (tokens[0] == "public") { methodAccess = EAccessmodifier.Public; typeIndex = 1; }
                    else if (tokens[0] == "private") { methodAccess = EAccessmodifier.Private; typeIndex = 1; }
                    else if (tokens[0] == "protected") { methodAccess = EAccessmodifier.Protected; typeIndex = 1; }
                    else if (tokens[0] == "internal") { methodAccess = EAccessmodifier.Internal; typeIndex = 1; }

                    bool isStatic = false;
                    if (tokens[typeIndex] == "static") { isStatic = true; typeIndex++; }

                    bool isAsync = false;
                    if (tokens[typeIndex] == "async") { isAsync = true; typeIndex++; }

                    string returnType = tokens[typeIndex];
                    string name = tokens[typeIndex + 1];

                    // Parameters
                    int parenStart = memberLine.IndexOf('(');
                    int parenEnd = memberLine.IndexOf(')');
                    string paramList = memberLine.Substring(parenStart + 1, parenEnd - parenStart - 1);
                    var parameters = paramList.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    classModel.Methods.Add(new MethodModel
                    {
                        IsAsync = isAsync,
                        IsStatic = isStatic,
                        Name = name,
                        ReturnType = returnType,
                        Parameters = parameters.ToList(),
                        AccessModifier = methodAccess
                    });
                }

                index++;
            }

            return classModel;
        }
    }
}

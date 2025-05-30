using ClassDiagrammGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClassDiagrammGenerator.Helper
{
    public class ExtractorHelper
    {
        public static Regex SingleLineProperty = new(
            @"^\s*(public|private|protected|internal|protected\s+internal|private\s+protected)?\s*(static\s+)?(readonly\s+)?[\w<>\[\],\s]+\s+\w+\s*\{\s*get;\s*set;\s*\}\s*(//.*)?$",
            RegexOptions.Compiled);

        public static Regex SingleLineMethod = new(
            @"^\s*(public|private|protected|internal|protected\s+internal|private\s+protected)?\s*(static\s+)?(async\s+)?[\w<>\[\],\s]+\s+\w+\s*\([^\)]*\)\s*(\{.*\}|where|\=\>|;)?\s*(//.*)?$",
            RegexOptions.Compiled);
        public static Regex MultiLineMethod = new(@" ^\s*(public|private|protected|internal|protected\s+internal|private\s+protected)?\s*(static\s+)?(async\s+)?[\w<>\[\],\s]+\s+\w+\s*\([^\)]*\)\s*(\{|\=\>|where|\;)?$", RegexOptions.Compiled);
        public static Regex MultiLineProperty = new(@"^\s*(public|private|protected|internal|protected\s+internal|private\s+protected)?\s*(static\s+)?(readonly\s+)?[\w<>\[\],\s]+\s+\w+\s*\{[\s\S]*?\}$", RegexOptions.Compiled | RegexOptions.Multiline);

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

            ClassModel classModel = new ClassModel
            {
                Name = className,
                Namespace = currentNamespace,
                AccessModifier = accessModifier
            };

            // Check for inheritance/interfaces (after ':')
            int colonIndex = line.IndexOf(':');
            if (colonIndex != -1)
            {
                var inheritancePart = line.Substring(colonIndex + 1).Trim();
                var types = inheritancePart.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var type in types)
                {
                    string cleanType = type.Replace("{", "").Trim();
                    // Simple heuristic: assume first is base class, rest are interfaces
                    if (type.StartsWith("I"))
                    {
                        classModel.Interfaces.Add(cleanType);
                    }
                    else
                    {
                        classModel.BaseClasses.Add(cleanType);
                    }
                }
            }

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

            List<string> classContent = GetContentOfBrackets(openedBrackets, lines, index);

            string classContentStr = string.Join("\n", classContent);

            foreach (Match match in SingleLineMethod.Matches(classContentStr))
            {
                string methodSignature = match.Value.Trim();
                if (!string.IsNullOrEmpty(methodSignature))
                {
                    classModel.AddMethod(methodSignature);
                }
            }

            foreach (Match match in MultiLineMethod.Matches(classContentStr))
            {
                string methodSignature = match.Value.Trim();
                if (!string.IsNullOrEmpty(methodSignature))
                {
                    classModel.AddMethod(methodSignature);
                }
            }

            foreach (Match match in SingleLineProperty.Matches(classContentStr))
            {
                string propertySignature = match.Value.Trim();
                if (!string.IsNullOrEmpty(propertySignature))
                {
                    classModel.Properties.Add(new PropertyModel(propertySignature, accessModifier));
                }
            }

            foreach (Match match in MultiLineProperty.Matches(classContentStr))
            {
                string propertySignature = match.Value.Trim();
                if (!string.IsNullOrEmpty(propertySignature))
                {
                    classModel.Properties.Add(new PropertyModel(propertySignature, accessModifier));
                }
            }



            return classModel;
        }

        public static List<string> GetContentOfBrackets(int bracketsCount, string[] lines, int startLine = 0)
        {
            List<string> content = new List<string>();
            bool commentBlock = false;
            for (int i = startLine; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("/*"))
                {
                    commentBlock = true;
                }
                else if (line.EndsWith("*/"))
                {
                    commentBlock = false;
                    continue; // skip this line
                }
                if (commentBlock || line.StartsWith("//") || string.IsNullOrWhiteSpace(line))
                {
                    continue; // skip comments and empty lines
                }
                if (line == "{")
                {
                    bracketsCount++;
                }
                else if (line == "}")
                {
                    bracketsCount--;
                    if (bracketsCount == 0)
                    {
                        break;
                    }
                }
                content.Add(line);
            }
            return content;
        }
    }
}

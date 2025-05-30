using ClassDiagrammGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassDiagrammGenerator.Helper
{
    public class MermaidDrawerHelper
    {

        public static string GenerateMermaidDiagram(List<ClassModel> classModels, List<InterfaceModel> interfaceModels, List<EnumModel> enumModels)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("classDiagram");
            // Add classes
            foreach (var classModel in classModels)
            {
                sb.AppendLine($"class {classModel.Name} {{");
                foreach (var property in classModel.Properties)
                {
                    sb.AppendLine($"  {property.Type} {property.Name}");
                }
                foreach (var method in classModel.Methods)
                {
                    sb.AppendLine($"  {method.ReturnType} {method.Name}()");
                }
                sb.AppendLine("}");
            }
            // Add interfaces
            foreach (var interfaceModel in interfaceModels)
            {
                sb.AppendLine($"interface {interfaceModel.Name} {{");
                foreach (var method in interfaceModel.Methods)
                {
                    sb.AppendLine($"  {method.ReturnType} {method.Name}()");
                }
                sb.AppendLine("}");
            }
            // Add enums
            foreach (var enumModel in enumModels)
            {
                sb.AppendLine($"enum {enumModel.Name} {{");
                foreach (var value in enumModel.Values)
                {
                    sb.AppendLine($"  {value},");
                }
                sb.AppendLine("}");
            }
            return sb.ToString();
        }

    }
}

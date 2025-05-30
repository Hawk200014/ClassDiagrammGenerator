using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassDiagrammGenerator.Models
{
    public class ClassModel
    {

        public string Name { get; set; }
        public string Namespace { get; set; }
        public List<string> BaseClasses { get; set; } = new List<string>();
        public List<string> Interfaces { get; set; } = new List<string>();
        public List<PropertyModel> Properties { get; set; } = new List<PropertyModel>();
        public List<MethodModel> Methods { get ; set; } = new List<MethodModel>();
        public EAccessmodifier AccessModifier { get; set; }

        internal void AddMethod(string methodSignature)
        {
            // Example signature: "public static async Task<int> GetDataAsync(string url, int timeout)"
            var tokens = methodSignature.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var accessModifier = EAccessmodifier.Private;
            bool isStatic = false;
            bool isAsync = false;

            // Parse modifiers
            int i = 0;
            while (i < tokens.Count)
            {
                if (Enum.TryParse<EAccessmodifier>(tokens[i], true, out var am))
                {
                    accessModifier = am;
                    i++;
                }
                else if (tokens[i].Equals("static", StringComparison.OrdinalIgnoreCase))
                {
                    isStatic = true;
                    i++;
                }
                else if (tokens[i].Equals("async", StringComparison.OrdinalIgnoreCase))
                {
                    isAsync = true;
                    i++;
                }
                else
                {
                    break;
                }
            }

            // Parse return type
            if (i >= tokens.Count) return;
            var returnType = tokens[i++];
            if (i >= tokens.Count) return;

            // Parse method name and parameters
            var nameAndParams = string.Join(" ", tokens.Skip(i));
            var nameEnd = nameAndParams.IndexOf('(');
            if (nameEnd < 0) return;

            var methodName = nameAndParams.Substring(0, nameEnd).Trim();
            var paramsStart = nameAndParams.IndexOf('(') + 1;
            var paramsEnd = nameAndParams.LastIndexOf(')');
            if (paramsEnd < paramsStart) return;

            var paramsString = nameAndParams.Substring(paramsStart, paramsEnd - paramsStart).Trim();
            var parameters = new List<string>();
            if (!string.IsNullOrWhiteSpace(paramsString))
            {
                parameters = paramsString.Split(',')
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToList();
            }

            var method = new MethodModel
            {
                Name = methodName,
                ReturnType = returnType,
                Parameters = parameters,
                AccessModifier = accessModifier,
                IsStatic = isStatic,
                IsAsync = isAsync
            };

            Methods.Add(method);
        }
    }
}

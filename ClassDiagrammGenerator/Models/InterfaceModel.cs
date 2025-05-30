using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassDiagrammGenerator.Models
{
    public class InterfaceModel
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public List<string> BaseInterfaces { get; set; } = new List<string>();
        public List<PropertyModel> Properties { get; set; } = new List<PropertyModel>();
        public List<MethodModel> Methods { get; set; } = new List<MethodModel>();
        public EAccessmodifier AccessModifier { get; set; }

        internal void AddMethod(string methodSignature)
        {
            var methodParts = methodSignature.Split(new[] { ' ' }, 2);
            if (methodParts.Length < 2) return; // Invalid signature
            
            var methodModel = new MethodModel
            {
                Name = methodParts[1].Split('(')[0].Trim(),
                ReturnType = methodParts[0].Trim(),
                AccessModifier = AccessModifier
            };
            
            // Extract parameters if any
            var parametersPart = methodSignature.Split('(').Skip(1).FirstOrDefault()?.Split(')')[0];
            if (!string.IsNullOrEmpty(parametersPart))
            {
                methodModel.Parameters = parametersPart.Split(',').Select(p => p.Trim()).ToList();
            }
            
            Methods.Add(methodModel);

        }
    }
}

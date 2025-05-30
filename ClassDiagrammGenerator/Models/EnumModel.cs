using System.Collections.Generic;

namespace ClassDiagrammGenerator.Models
{
    public class EnumModel
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public List<string> Values { get; set; } = new List<string>();
        public EAccessmodifier AccessModifier { get; set; }
        public EnumModel(string name, string @namespace, EAccessmodifier accessModifier)
        {
            Name = name;
            Namespace = @namespace;
            AccessModifier = accessModifier;
        }
        public void AddValue(string value)
        {
            Values.Add(value);
        }
    }
}

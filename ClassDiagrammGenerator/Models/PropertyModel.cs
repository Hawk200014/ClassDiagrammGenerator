namespace ClassDiagrammGenerator.Models
{
    public class PropertyModel
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public EAccessmodifier AccessModifier { get; set; }
        public bool IsStatic { get; set; }
        public bool IsReadOnly { get; set; }
        public PropertyModel(string name, string type, EAccessmodifier accessModifier, bool isStatic = false, bool isReadOnly = false)
        {
            Name = name;
            Type = type;
            AccessModifier = accessModifier;
            IsStatic = isStatic;
            IsReadOnly = isReadOnly;
        }
    }
}
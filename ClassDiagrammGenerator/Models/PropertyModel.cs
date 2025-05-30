using System;

namespace ClassDiagrammGenerator.Models
{
    public class PropertyModel
    {

        public string Name { get; set; }
        public string Type { get; set; }
        public EAccessmodifier AccessModifier { get; set; }
        public bool IsStatic { get; set; }
        public bool IsReadOnly { get; set; }

        public PropertyModel(string propertySignature, EAccessmodifier accessModifier)
        {
            // Example signature: "public static readonly int MyProperty"
            // Remove extra spaces and split by space
            var tokens = propertySignature.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            int index = 0;
            IsStatic = false;
            IsReadOnly = false;

            // Check for access modifier in signature (optional, as it's passed separately)
            if (tokens[index].Equals("public", StringComparison.OrdinalIgnoreCase) ||
                tokens[index].Equals("private", StringComparison.OrdinalIgnoreCase) ||
                tokens[index].Equals("protected", StringComparison.OrdinalIgnoreCase) ||
                tokens[index].Equals("internal", StringComparison.OrdinalIgnoreCase))
            {
                index++;
            }

            // Check for 'static'
            if (index < tokens.Length && tokens[index].Equals("static", StringComparison.OrdinalIgnoreCase))
            {
                IsStatic = true;
                index++;
            }

            // Check for 'readonly'
            if (index < tokens.Length && tokens[index].Equals("readonly", StringComparison.OrdinalIgnoreCase))
            {
                IsReadOnly = true;
                index++;
            }

            // Next token is type
            if (index < tokens.Length)
            {
                Type = tokens[index];
                index++;
            }

            // Next token is name
            if (index < tokens.Length)
            {
                Name = tokens[index];
            }

            AccessModifier = accessModifier;
        }
    }
}
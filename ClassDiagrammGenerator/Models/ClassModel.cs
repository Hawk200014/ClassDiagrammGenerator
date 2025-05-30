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
    }
}

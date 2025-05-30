using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassDiagrammGenerator.Models
{
    public class MethodModel
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public List<string> Parameters { get; set; } = new List<string>();
        public EAccessmodifier AccessModifier { get; set; }
        public bool IsStatic { get; set; }
        public bool IsAsync { get; set; }
    }
}

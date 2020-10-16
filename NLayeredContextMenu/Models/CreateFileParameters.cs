using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLayeredContextMenu.Models
{
    public class CreateFileParameters
    {
        public ProjectItem ProjectItem { get; set; }
        public string ProjectTemplate { get; set; }
        public string FileNameWithoutExtension { get; set; }
        public string ProjectName { get; set; }
        public string FileContent { get; set; }
    }
}

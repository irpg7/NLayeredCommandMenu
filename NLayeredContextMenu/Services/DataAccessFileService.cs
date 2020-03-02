using EnvDTE;
using Microsoft.VisualStudio.Shell;
using NLayeredContextMenu.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLayeredContextMenu.Services
{
    public static class DataAccessFileService
    {
        #region Abstract
        public static void CreateDalAbstract(CreateFileParameters fileParameters)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                var addedItem = fileParameters.ProjectItem.ProjectItems.AddFromTemplate(fileParameters.ProjectTemplate,
                                                                        $"I{fileParameters.FileNameWithoutExtension}Dal.cs");

                var addedItemDocument = addedItem.Document;
                var textDocument = addedItemDocument.Object() as TextDocument;
                var p = textDocument.StartPoint.CreateEditPoint();
                p.Delete(textDocument.EndPoint);
                p.Insert(CreateDalAbstractFileContent(fileParameters.FileNameWithoutExtension, fileParameters.ProjectName));
                p.SmartFormat(textDocument.StartPoint);
                addedItemDocument.Save();
            }
            catch
            {
                throw;
            }
        }

        private static string CreateDalAbstractFileContent(string fileName, string projectName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("using System;");
            stringBuilder.AppendLine("using Core.DataAccess;");
            stringBuilder.AppendLine("using Entities.Concrete;");
            stringBuilder.AppendLine("\n");
            stringBuilder.AppendLine($"namespace {projectName}.Abstract");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"public interface I{fileName}Dal:IEntityRepository<{fileName}>");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine("}");
            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }
        #endregion

        #region Concrete
        public static void CreateDalConcrete(CreateFileParameters fileParameters)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                var addedItem = fileParameters.ProjectItem.ProjectItems.AddFromTemplate(fileParameters.ProjectTemplate, $"Ef{fileParameters.FileNameWithoutExtension}Dal.cs");
                var addedItemDocument = addedItem.Document;
                var textDocument = addedItemDocument.Object() as TextDocument;
                var p = textDocument.StartPoint.CreateEditPoint();
                p.Delete(textDocument.EndPoint);
                p.Insert(CreateDalConcreteFileContent(fileParameters.FileNameWithoutExtension, fileParameters.ProjectName));
                p.SmartFormat(textDocument.StartPoint);
                addedItemDocument.Save();
            }
            catch
            {
                throw;
            }

        }
        private static string CreateDalConcreteFileContent(string fileName, string projectName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("using System;");
            stringBuilder.AppendLine("using System.Linq;");
            stringBuilder.AppendLine("using Core.DataAccess.EntityFramework;");
            stringBuilder.AppendLine("using Entities.Concrete;");
            stringBuilder.AppendLine($"using {projectName}.Abstract;");
            stringBuilder.AppendLine("\n");
            stringBuilder.AppendLine($"namespace {projectName}.Concrete.EntityFramework");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"public class Ef{fileName}Dal:EfEntityRepositoryBase<{fileName},context>,I{fileName}Dal");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine("}");
            stringBuilder.AppendLine("}");
            return stringBuilder.ToString();
        }
        #endregion
    }
}

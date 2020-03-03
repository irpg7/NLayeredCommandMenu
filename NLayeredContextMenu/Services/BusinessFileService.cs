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
    public static class BusinessFileService
    {
        #region Abstract

        public static void CreateBusinessAbstract(CreateFileParameters fileParameters)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                var addedItem = fileParameters.ProjectItem.ProjectItems.AddFromTemplate(fileParameters.ProjectTemplate,
                                                                        $"I{fileParameters.FileNameWithoutExtension}Service.cs");

                var addedItemDocument = addedItem.Document;
                var textDocument = addedItemDocument.Object() as TextDocument;
                var p = textDocument.StartPoint.CreateEditPoint();
                p.Delete(textDocument.EndPoint);
                p.Insert(CreateBusinessAbstractFileContent(fileParameters.FileNameWithoutExtension, fileParameters.ProjectName));
                p.SmartFormat(textDocument.StartPoint);
                addedItemDocument.Save();
            }
            catch
            {
                throw;
            }
        }
        private static string CreateBusinessAbstractFileContent(string fileName, string projectName)
        {
            return _fmtClassFile
                    .Replace("[projectName]", projectName)
                    .Replace("[fileName]", fileName);

        }
        private const string _fmtClassFile = @"
using Entities.Concrete;
using System.Collections.Generic;

namespace [projectName].Abstract
{
public interface I[fileName]Service
{
[fileName] Get(int id);
List<[fileName]> GetList();
void Add([fileName] entity);
void Update([fileName] entity);
void Delete(int id);
}
}
";
        #endregion

        #region Concrete
        public static void CreateBusinessConcrete(CreateFileParameters fileParameters)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                var addedItem = fileParameters.ProjectItem.ProjectItems.AddFromTemplate(fileParameters.ProjectTemplate, $"{fileParameters.FileNameWithoutExtension}Manager.cs");
                var addedItemDocument = addedItem.Document;
                var textDocument = addedItemDocument.Object() as TextDocument;
                var p = textDocument.StartPoint.CreateEditPoint();
                p.Delete(textDocument.EndPoint);
                p.Insert(CreateBusinessConcreteFileContent(fileParameters.FileNameWithoutExtension, fileParameters.ProjectName));
                p.SmartFormat(textDocument.StartPoint);
                addedItemDocument.Save();
            }
            catch
            {
                throw;
            }
        }

        private static string CreateBusinessConcreteFileContent(string fileName, string projectName)
        {
            string camelCasedFileName = char.ToLowerInvariant(fileName[0]) + fileName.Substring(1);
            return _fmtConcreteFile
                  .Replace("[projectName]", projectName)
                  .Replace("[fileName]", fileName)
                  .Replace("[camelCasedFileName]", camelCasedFileName);
        }
        private const string _fmtConcreteFile = @"
           using System;
           using System.Linq;
           using System.Collections.Generic;
           using Entities.Concrete;
           using DataAccess.Abstract;
            using [projectName].Abstract;
          
            namespace [projectName].Concrete
           {
            public class [fileName]Manager:I[fileName]Service
           {
            private I[fileName]Dal _[camelCasedFileName]Dal;
            public [fileName]Manager(I[fileName]Dal [camelCasedFileName]Dal)
           {
            _[camelCasedFileName]Dal=[camelCasedFileName]Dal;
           }

            public [fileName] Get(int id)
           {
            return _[camelCasedFileName]Dal.Get(x=>x.Id == id);
           }

            public List<[fileName]> GetList()
           {
            return _[camelCasedFileName]Dal.GetList();
           }

            public void Add([fileName] entity)
           {
            _[camelCasedFileName]Dal.Add(entity);
           }

            public void Update([fileName] entity)
           {
            _[camelCasedFileName]Dal.Update(entity);
           }

            public void Delete(int id)
           {
            var entity = Get(id);
            _[camelCasedFileName]Dal.Delete(entity);
           }

           }

           }";
        #endregion
    }
}

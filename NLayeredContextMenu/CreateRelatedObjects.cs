using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using System.Linq;
using EnvDTE80;
using System.IO;
using System.Text;
using EnvDTE;
using NLayeredContextMenu.Models;
using NLayeredContextMenu.Services;

namespace NLayeredContextMenu
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CreateRelatedObjects
    {
       
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("d1103015-8d0f-4e7a-be44-465e8893d23e");
        private readonly AsyncPackage package;
      
        private CreateRelatedObjects(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static CreateRelatedObjects Instance
        {
            get;
            private set;
        }

      
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return package;
            }
        }
        private IServiceProvider SyncServiceProvider
        {
            get
            {
                return package;
            }
        }
   
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in CreateRelatedObjects's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new CreateRelatedObjects(package, commandService);
        }

       
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "CreateRelatedObjects";
            EnvDTE.DTE dte;
            EnvDTE.SelectedItems selectedItems;
            EnvDTE.ProjectItem projectItem;
            EnvDTE80.Solution2 solution2;
            dte = SyncServiceProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE ?? throw new ArgumentNullException();
            solution2 = (Solution2)dte.Solution;
            selectedItems = dte.SelectedItems;
            if (selectedItems.Count > 1)
            {
                message = "Can't Support multiple creation yet.";
                VsShellUtilities.ShowMessageBox((IServiceProvider)ServiceProvider, message, title, OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
            if (selectedItems == null)
            {
                message = "There isn't any item selected";
                VsShellUtilities.ShowMessageBox((IServiceProvider)ServiceProvider, message, title, OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }
            foreach (EnvDTE.SelectedItem selectedItem in selectedItems)
            {
                projectItem = selectedItem.ProjectItem as ProjectItem;
                if (!IsIEntityImplementation(projectItem))
                {
                    message = "Object must be Implemented from IEntity in order to generate";
                    VsShellUtilities.ShowMessageBox((IServiceProvider)ServiceProvider, message, title,
                        OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    return;
                }

                if (projectItem != null)
                {
                    foreach (EnvDTE.Project project in dte.Solution.Projects)
                    {

                        if (project.Name.EndsWith("DataAccess") || project.Name.EndsWith("Dal"))
                        {
                            if (!DoesProjectFolderExists(project.FullName, "Abstract"))
                                project.ProjectItems.AddFolder("Abstract");

                            if (!DoesProjectFolderExists(project.FullName, "Concrete\\EntityFramework"))
                                project.ProjectItems.AddFolder("Concrete\\EntityFramework");

                            foreach (EnvDTE.ProjectItem item in project.ProjectItems)
                            {
                                var projectTemplate = solution2.GetProjectItemTemplate("Interface", "CSharp");
                                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(projectItem.Name);
                                var fileParameters = new CreateFileParameters
                                {
                                    ProjectItem = item,
                                    ProjectTemplate = projectTemplate,
                                    ProjectName = project.Name,
                                    FileNameWithoutExtension = fileNameWithoutExtension,

                                };
                                if (item.Name == "Abstract")
                                {
                                    DataAccessFileService.CreateDalAbstract(fileParameters);
                                }
                                if (item.Name == "Concrete")
                                {
                                    foreach (ProjectItem concrete in item.ProjectItems)
                                    {
                                        if (concrete.Name == "EntityFramework")
                                        {
                                            fileParameters.ProjectItem = concrete;
                                            DataAccessFileService.CreateDalConcrete(fileParameters);
                                        }
                                    }
                                }
                            }

                        }

                        if (project.Name.EndsWith("Business") || project.Name.EndsWith("Bll"))
                        {
                            if (!DoesProjectFolderExists(project.FullName, "Abstract"))
                                project.ProjectItems.AddFolder("Abstract");

                            if (!DoesProjectFolderExists(project.FullName, "Concrete"))
                                project.ProjectItems.AddFolder("Concrete");

                            foreach (EnvDTE.ProjectItem item in project.ProjectItems)
                            {
                                var projectTemplate = solution2.GetProjectItemTemplate("Interface", "CSharp");
                                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(projectItem.Name);
                                var fileParameters = new CreateFileParameters
                                {
                                    FileNameWithoutExtension = fileNameWithoutExtension,
                                    ProjectItem = item,
                                    ProjectName = project.Name,
                                    ProjectTemplate = projectTemplate
                                };
                                if (item.Name == "Abstract")
                                {
                                    BusinessFileService.CreateBusinessAbstract(fileParameters);
                                }
                                if (item.Name == "Concrete")
                                {
                                    BusinessFileService.CreateBusinessConcrete(fileParameters);
                                }
                            }
                        }
                    }

                }
            }
        }

        private static bool IsIEntityImplementation(ProjectItem projectItem)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            foreach (CodeElement2 codeElement in projectItem.FileCodeModel.CodeElements)
            {
                if (codeElement is CodeNamespace)
                {
                    var nspace = codeElement as CodeNamespace;

                    foreach (CodeClass property in nspace.Members)
                    {

                        if (property is null)
                            continue;

                        foreach (CodeInterface iface in property.ImplementedInterfaces)
                        {
                            if (iface.Name == "IEntity")
                                return true;
                        }

                    }
                }
            }
            return false;
        }

        private bool DoesProjectFolderExists(string projectFullName, string folderNameToCheck)
        {
            var projectPath = Path.GetDirectoryName(projectFullName);

            return Directory.Exists(Path.Combine(projectPath, folderNameToCheck));
        }

    }
}

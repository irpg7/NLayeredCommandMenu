using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLayeredContextMenu.Constants
{
    public class FileContents
    {
        public const string DataAccessAbstract = @"
           using System;
           using Core.DataAccess;
           using Entities.Concrete;         
           namespace [projectName].Abstract
           {
           public interface I[fileName]Dal:IEntityRepository<[fileName]>
           {
           }
           }
           ";

        public const string DataAccessConcrete = @"
            using System;
            using System.Linq;
            using Core.DataAccess.EntityFramework;
            using Entities.Concrete;
            using [projectName].Abstract;
            using DataAccess.Concrete.EntityFramework.Contexts;
            
            namespace [projectName].Concrete.EntityFramework
            {
            public class Ef[fileName]Dal:EfEntityRepositoryBase<[fileName],[DbContextName]>,I[fileName]Dal
            {
            }
            }
        ";

        public const string BusinessAbstractContent = @"
            using Entities.Concrete;
            using System.Collections.Generic;
            using Core.Utilities.Results;
            
            
            namespace [projectName].Abstract
            {
                public interface I[fileName]Service
                {
                    IDataResult<[fileName]> Get(int id);
                    IDataResult<List<[fileName]>> GetList();
                    IResult Add([fileName] entity);
                    IResult Update([fileName] entity);
                    IResult Delete(int id);
                }
            }
            ";

        public const string BusinessConcreteContent = @"
           using System;
           using System.Linq;
           using System.Collections.Generic;
           using Entities.Concrete;
           using DataAccess.Abstract;
           using [projectName].Abstract;
           using Core.Utilities.Results;
           using Business.Constants;
          
            namespace [projectName].Concrete
           {
            public class [fileName]Manager:I[fileName]Service
           {
            private I[fileName]Dal _[camelCasedFileName]Dal;
            public [fileName]Manager(I[fileName]Dal [camelCasedFileName]Dal)
           {
            _[camelCasedFileName]Dal=[camelCasedFileName]Dal;
           }

            public IDataResult<[fileName]> Get(int id)
           {
            return new SuccessDataResult<[fileName]>(_[camelCasedFileName]Dal.Get(x=>x.Id == id));
           }

            public IDataResult<List<[fileName]>> GetList()
           {
            return new SuccessDataResult<List<[fileName]>>(_[camelCasedFileName]Dal.GetList());
           }

            public IResult Add([fileName] entity)
           {
              _[camelCasedFileName]Dal.Add(entity);
              return new SuccessResult(Messages.[fileName]Added);
           }

            public IResult Update([fileName] entity)
           {
              _[camelCasedFileName]Dal.Update(entity);
              return new SuccessResult(Messages.[fileName]Updated);
           }

            public IResult Delete(int id)
           {
             var entity = Get(id).Data;
             _[camelCasedFileName]Dal.Delete(entity);
             return new SuccessResult(Messages.[fileName]Deleted);
           }

           }

           }";


    }
}

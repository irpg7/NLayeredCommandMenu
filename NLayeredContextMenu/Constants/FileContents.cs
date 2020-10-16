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
           public interface I[fileName]Repository:IEntityRepositoryBase<[fileName]>
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
            public class Ef[fileName]Repository:EfEntityRepositoryBase<[fileName],[DbContextName]>,I[fileName]Repository
            {
            }
            }
        ";

        public const string BusinessCreateCommand = @"
using [projectName].Common.Constants;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using MediatR;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace [projectName].Handlers.[pluralizedFileName].Commands
{
	public class Create[fileName]Command : IRequest<IResult>
	{


		public class Create[fileName]CommandHandler : IRequestHandler<Create[fileName]Command, IResult>
		{
			  private readonly I[fileName]Repository _[camelCasedFileName]Repository;
			  private readonly IMediator _mediator;
              private IMapper _mapper;
			  public Create[fileName]CommandHandler(I[fileName]Repository [camelCasedFileName]Repository,IMapper mapper)
			  {
			  	    _[camelCasedFileName]Repository = [camelCasedFileName]Repository;
                    _mapper = mapper     
			  }
              
			  public async Task<IResult> Handle(Create[fileName]Command request, CancellationToken cancellationToken)
			  {
			  				var added[fileName] = _mapper.Map<DestinationType>(source);
			  				_[camelCasedFileName]Repository.AddAsync(added[fileName]);
			  				return new SuccessResult(Messages.[fileName]Added);
			  }
		}
	}
}";
        public const string BusinessUpdateCommand = @"
using [projectName].Common.Constants;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using MediatR;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace [projectName].Handlers.[pluralizedFileName].Commands
{
    public class Update[fileName]Command:IRequest<IResult>
    {

        public class Update[fileName]CommandHandler : IRequestHandler<Update[fileName]Command, IResult>
        {
            I[fileName]Repository _[camelCasedFileName]Repository;
            private IMapper _mapper;
            public Update[fileName]CommandHandler(I[fileName]Repository [camelCasedFileName]Repository,IMapper mapper)
            {
                _[camelCasedFileName]Repository = [camelCasedFileName]Repository;
                _mapper = mapper;
            }

            public async Task<IResult> Handle(Update[fileName]Command request, CancellationToken cancellationToken)
            {
                var entityToUpdate = _mapper.Map<DestinationType>(source);
                await _[camelCasedFileName]Repository.UpdateAsync(entityToUpdate);

                return new SuccessResult(Messages.[fileName]Updated);
            }
        }
    }
}
        ";
        public const string BusinessDeleteCommand = @"
using [projectName].Common.Constants;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
namespace [projectName].Handlers.[pluralizedFileName].Commands
{
            public class Delete[fileName]Command:IRequest<IResult>
    {
        public int Id { get; set; }


        public class Delete[fileName]CommandHandler : IRequestHandler<Delete[fileName]Command, IResult>
        {
            I[fileName]Repository _[camelCasedFileName]Repository;

            public Delete[fileName]CommandHandler(I[fileName]Repository [camelCasedFileName]Repository)
            {
               _[camelCasedFileName]Repository = [camelCasedFileName]Repository;
            }

            public async Task<IResult> Handle(Delete[fileName]Command request, CancellationToken cancellationToken)
            {
                var recordToDelete = await _[camelCasedFileName]Repository.GetAsync(x => x.Id == request.Id);

                await _[camelCasedFileName]Repository.DeleteAsync(recordToDelete);

                return new SuccessResult(Messages.[fileName]Deleted);
            }
        }
    }
}
        ";
        public const string BusinessGetQuery = @"
using Core.Entities.Concrete;
using Core.Utilities.Results;
using DataAccess.Abstract;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Entities.Concrete;

        namespace [projectName].Handlers.[pluralizedFileName].Queries
{
    public class Get[fileName]Query:IRequest<IDataResult<[fileName]>>
    {
        public int Id { get; set; }

        public class Get[fileName]QueryHandler : IRequestHandler<Get[fileName]Query, IDataResult<[fileName]>>
        {
            I[fileName]Repository _[camelCasedFileName]Repository;

            public Get[fileName]QueryHandler(I[fileName]Repository [camelCasedFileName]Repository)
            {
                _[camelCasedFileName]Repository = [camelCasedFileName]Repository;
            }

            public async Task<IDataResult<[fileName]>> Handle(Get[fileName]Query request, CancellationToken cancellationToken)
            {
                return new SuccessDataResult<[fileName]>(await _[camelCasedFileName]Repository.GetAsync(x => x.Id == request.Id));
            }
        }
    }
}
        ";
        public const string BusinessGetListQuery = @"
         using Core.Entities.Concrete;
using Core.Utilities.Results;
using DataAccess.Abstract;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Entities.Concrete;

        namespace [projectName].Handlers.[pluralizedFileName].Queries
{
    public class Get[pluralizedFileName]Query:IRequest<IDataResult<IEnumerable<[fileName]>>>
    {
      
        public class Get[pluralizedFileName]QueryHandler : IRequestHandler<Get[pluralizedFileName]Query, IDataResult<IEnumerable<[fileName]>>>
        {
            I[fileName]Repository _[camelCasedFileName]Repository;

            public Get[pluralizedFileName]QueryHandler(I[fileName]Repository [camelCasedFileName]Repository)
            {
                _[camelCasedFileName]Repository = [camelCasedFileName]Repository;
            }

            public async Task<IDataResult<IEnumerable<[fileName]>>> Handle(Get[pluralizedFileName]Query request, CancellationToken cancellationToken)
            {
                return new SuccessDataResult<IEnumerable<[fileName]>>(await _[camelCasedFileName]Repository.GetListAsync());
            }
        }
    }
}
        ";



    }
}

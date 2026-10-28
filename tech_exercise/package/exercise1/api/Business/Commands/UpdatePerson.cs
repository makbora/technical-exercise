using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands
{
    public class UpdatePerson : IRequest<UpdatePersonResult>
    {
        public required string Name;
        public required string NewName { get; set; } = string.Empty;
        public Person? Person;
    }

    public class UpdatePersonPreProcessor : IRequestPreProcessor<UpdatePerson>
    {
        private readonly StargateContext _context;
        public UpdatePersonPreProcessor(StargateContext context)
        {
            _context = context;
        }
        public Task Process(UpdatePerson request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name)||string.IsNullOrWhiteSpace(request.NewName)) throw new BadHttpRequestException("Bad Request: Name and NewName cannot be empty");

            var person = _context.People.AsNoTracking().FirstOrDefault(z => z.Name == request.Name);

            if (person is null) throw new BadHttpRequestException("Bad Request: Person with this name does not exist");

            var newPerson = _context.People.AsNoTracking().FirstOrDefault(z => z.Name == request.NewName);

            if (newPerson is not null) throw new BadHttpRequestException("Bad Request: Person with this name already exists");

            request.Person = person;

            return Task.CompletedTask;
        }
    }

    public class UpdatePersonHandler : IRequestHandler<UpdatePerson, UpdatePersonResult>
    {
        private readonly StargateContext _context;

        public UpdatePersonHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<UpdatePersonResult> Handle(UpdatePerson request, CancellationToken cancellationToken)
        {

                request.Person.Name = request.NewName;

                _context.People.Update(request.Person);

                await _context.SaveChangesAsync();

                return new UpdatePersonResult()
                {
                    Id = request.Person.Id
                };
          
        }
    }

    public class UpdatePersonResult : BaseResponse
    {
        public int Id { get; set; }
    }
}

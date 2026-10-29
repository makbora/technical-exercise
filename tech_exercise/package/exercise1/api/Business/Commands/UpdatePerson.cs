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
        public async Task Process(UpdatePerson request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name)||string.IsNullOrWhiteSpace(request.NewName)) throw new BadHttpRequestException("Bad Request: Name and NewName cannot be empty");

            var person = await _context.People.AsNoTracking().FirstOrDefaultAsync(z => EF.Functions.Collate(z.Name, "NOCASE") == request.Name, cancellationToken);

            if (person is null) throw new BadHttpRequestException("Bad Request: Person with this name does not exist");

            // Names are unique regardless of capitalization; the person themselves is excluded so they can fix their own capitalization.
            var newPerson = await _context.People.AsNoTracking().FirstOrDefaultAsync(z => EF.Functions.Collate(z.Name, "NOCASE") == request.NewName && z.Id != person.Id, cancellationToken);

            if (newPerson is not null) throw new BadHttpRequestException("Bad Request: Person with this name already exists");

            request.Person = person;
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

                await _context.SaveChangesAsync(cancellationToken);

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

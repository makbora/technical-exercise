using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;
using Microsoft.EntityFrameworkCore;

namespace StargateAPI.Business.Queries
{
    public class GetPersonByName : IRequest<GetPersonByNameResult>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public class GetPersonByNameHandler : IRequestHandler<GetPersonByName, GetPersonByNameResult>
    {
        private readonly StargateContext _context;
        public GetPersonByNameHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetPersonByNameResult> Handle(GetPersonByName request, CancellationToken cancellationToken)
        {
            var result = new GetPersonByNameResult();

            var person = await _context.People
            .AsNoTracking()
            .Include(z => z.AstronautDetail)
            .Where(z => z.Name == request.Name)
            .Select(z => new PersonAstronaut()
            {
                PersonId = z.Id,
                Name = z.Name,
                CurrentRank = z.AstronautDetail.CurrentRank,
                CurrentDutyTitle = z.AstronautDetail.CurrentDutyTitle,
                CareerStartDate = z.AstronautDetail.CareerStartDate,
                CareerEndDate = z.AstronautDetail.CareerEndDate
            })
            .FirstOrDefaultAsync(cancellationToken);

            result.Person = person;

            return result;
        }
    }

    public class GetPersonByNameResult : BaseResponse
    {
        public PersonAstronaut? Person { get; set; }
    }
}

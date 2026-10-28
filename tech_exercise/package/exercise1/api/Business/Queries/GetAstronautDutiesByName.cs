using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;
using Microsoft.EntityFrameworkCore;

namespace StargateAPI.Business.Queries
{
    public class GetAstronautDutiesByName : IRequest<GetAstronautDutiesByNameResult>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class GetAstronautDutiesByNameHandler : IRequestHandler<GetAstronautDutiesByName, GetAstronautDutiesByNameResult>
    {
        private readonly StargateContext _context;

        public GetAstronautDutiesByNameHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetAstronautDutiesByNameResult> Handle(GetAstronautDutiesByName request, CancellationToken cancellationToken)
        {

            var result = new GetAstronautDutiesByNameResult();

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

            var duties = await _context.AstronautDuties.Where(z => z.PersonId == person.PersonId).OrderByDescending(z => z.DutyStartDate).ToListAsync(cancellationToken);

            result.AstronautDuties = duties.ToList();

            return result;

        }
    }

    public class GetAstronautDutiesByNameResult : BaseResponse
    {
        public PersonAstronaut Person { get; set; }
        public List<AstronautDuty> AstronautDuties { get; set; } = new List<AstronautDuty>();
    }
}

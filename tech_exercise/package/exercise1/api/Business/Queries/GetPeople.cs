using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;
using Microsoft.EntityFrameworkCore;

namespace StargateAPI.Business.Queries
{
    public class GetPeople : IRequest<GetPeopleResult>
    {

    }

    public class GetPeopleHandler : IRequestHandler<GetPeople, GetPeopleResult>
    {
        public readonly StargateContext _context;
        public GetPeopleHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<GetPeopleResult> Handle(GetPeople request, CancellationToken cancellationToken)
        {
            var result = new GetPeopleResult();

            var people = await _context.People
            .AsNoTracking()
            .Include(z => z.AstronautDetail)
            .Select(z => new PersonAstronaut()
            {
                PersonId = z.Id,
                Name = z.Name,
                CurrentRank = z.AstronautDetail.CurrentRank,
                CurrentDutyTitle = z.AstronautDetail.CurrentDutyTitle,
                CareerStartDate = z.AstronautDetail.CareerStartDate,
                CareerEndDate = z.AstronautDetail.CareerEndDate
            })
            .ToListAsync(cancellationToken);

            result.People = people.ToList();

            return result;
        }
    }

    public class GetPeopleResult : BaseResponse
    {
        public List<PersonAstronaut> People { get; set; } = new List<PersonAstronaut> { };

    }
}

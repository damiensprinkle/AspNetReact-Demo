using Application.Core;
using Application.DTOs;
using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Activities
{
    public class Edit
    {
        public class Command : IRequest<Result<ActivityDto>>
        {
            public Guid Id { get; set; }
            public ActivityFormDto Activity { get; set; } = null!;
        }

        public class Handler : IRequestHandler<Command, Result<ActivityDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<ActivityDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var activity = await _context.Activities.FindAsync(new object[] { request.Id }, cancellationToken);

                if (activity == null)
                    return Result<ActivityDto>.NotFound("Activity not found");

                _mapper.Map(request.Activity, activity);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                    return Result<ActivityDto>.Failure("Failed to update activity");

                return Result<ActivityDto>.Success(_mapper.Map<ActivityDto>(activity));
            }
        }
    }
}

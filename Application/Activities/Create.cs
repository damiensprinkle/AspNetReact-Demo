using Application.Core;
using Application.DTOs;
using AutoMapper;
using Domain;
using MediatR;
using Persistence;

namespace Application.Activities
{
    public class Create
    {
        public class Command : IRequest<Result<ActivityDto>>
        {
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
                var activity = _mapper.Map<Activity>(request.Activity);
                activity.Id = Guid.NewGuid();

                _context.Activities.Add(activity);
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                    return Result<ActivityDto>.Failure("Failed to create activity");

                return Result<ActivityDto>.Success(_mapper.Map<ActivityDto>(activity));
            }
        }
    }
}

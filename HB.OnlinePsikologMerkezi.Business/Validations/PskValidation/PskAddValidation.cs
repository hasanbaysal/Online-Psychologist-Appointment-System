using FluentValidation;
using HB.OnlinePsikologMerkezi.Dto.Dtos;

namespace HB.OnlinePsikologMerkezi.Business.Validations.PskValidation
{
    public class PskAddValidation : AbstractValidator<PsychologistAddDto>
    {
        public PskAddValidation()
        {

            RuleFor(x => x.PsychologistCategories)
               .Must(x => x.Count > 0)
               .WithMessage("en az bir kategoriye sahip olmalı");
            RuleFor(x => x.ConsulationPrice).GreaterThan(10)
                .WithMessage("görüşme ücreti 10 TL'den fazla olmalı");


        }
    }

    public class PskUpdateValidation : AbstractValidator<PsychologistUpdateDto>
    {
        public PskUpdateValidation()
        {

            RuleFor(x => x.PsychologistCategories)
               .Must(x => x.Count > 0)
               .WithMessage("en az bir kategoriye sahip olmalı");
            RuleFor(x => x.ConsulationPrice).GreaterThan(10)
                .WithMessage("görüşme ücreti 10 TL'den fazla olmalı");


        }
    }
}

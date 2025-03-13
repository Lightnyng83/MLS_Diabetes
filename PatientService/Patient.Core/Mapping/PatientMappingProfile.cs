using AutoMapper;
using Patient.Models.Bdd;

public class PatientMappingProfile : Profile
{
    public PatientMappingProfile()
    {
        // Mapping de Patient vers PatientViewModel (pour les réponses au client)
        CreateMap<Patient.Models.Bdd.Patient, PatientViewModel>();

        // Mapping inverse (pour transformer les données reçues du client en entité Patient)
        CreateMap<PatientViewModel, Patient.Models.Bdd.Patient>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); ;
    }
}
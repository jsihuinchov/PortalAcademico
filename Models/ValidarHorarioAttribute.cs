using System.ComponentModel.DataAnnotations;

namespace PortalAcademico.Models
{
    public class ValidarHorarioAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var curso = (Curso)validationContext.ObjectInstance;
            
            if (curso.HorarioInicio >= curso.HorarioFin)
            {
                return new ValidationResult("El horario de inicio debe ser anterior al horario de fin");
            }
            
            return ValidationResult.Success;
        }
    }
}
namespace ApiPetFoundation.Application.DTOs.Pets
{
    /// <summary>Datos detallados de una mascota.</summary>
    public class PetResponseDetails
    {
        /// <summary>Id de la mascota.</summary>
        public int Id { get; init; }
        /// <summary>Nombre de la mascota.</summary>
        public string Name { get; init; } = string.Empty;
        /// <summary>Especie de la mascota.</summary>
        public string Species { get; init; } = string.Empty;
        /// <summary>Raza de la mascota.</summary>
        public string? Breed { get; init; }
        /// <summary>Edad de la mascota.</summary>
        public int Age { get; init; }
        /// <summary>Sexo de la mascota.</summary>
        public string Sex { get; init; } = string.Empty;
        /// <summary>Tamano de la mascota.</summary>
        public string Size { get; init; } = string.Empty;
        /// <summary>Descripcion de la mascota.</summary>
        public string Description { get; init; } = string.Empty;
        /// <summary>Estado de adopcion de la mascota.</summary>
        public string Status { get; init; } = string.Empty;
        /// <summary>Id del usuario que creo la mascota.</summary>
        public int CreatedById { get; init; }
        /// <summary>Url de la imagen principal.</summary>
        public string CoverImageUrl { get; init; } = string.Empty;
        /// <summary>Fecha de creacion.</summary>
        public DateTime CreatedAt { get; init; }
        /// <summary>Fecha de ultima actualizacion.</summary>
        public DateTime UpdatedAt { get; init; }
    }
}

namespace ApiPetFoundation.Application.DTOs.Pets
{
    /// <summary>Datos de una mascota.</summary>
    public class PetResponse
    {
        /// <summary>Id de la mascota.</summary>
        public required int Id { get; init; }
        /// <summary>Nombre de la mascota.</summary>
        public required string Name { get; init; }
        /// <summary>Especie de la mascota.</summary>
        public required string Species { get; init; }
        /// <summary>Raza de la mascota.</summary>
        public string? Breed { get; init; }
        /// <summary>Edad de la mascota.</summary>
        public required int Age { get; init; }
        /// <summary>Sexo de la mascota.</summary>
        public required string Sex { get; init; }
        /// <summary>Tamano de la mascota.</summary>
        public required string Size { get; init; }
        /// <summary>Descripcion de la mascota.</summary>
        public required string Description { get; init; }
        /// <summary>Estado de adopcion de la mascota.</summary>
        public required string Status { get; init; }
        /// <summary>Url de la imagen principal.</summary>
        public string? CoverImageUrl { get; init; }
    }
}

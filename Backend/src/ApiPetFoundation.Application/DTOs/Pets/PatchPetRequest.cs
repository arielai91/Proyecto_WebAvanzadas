namespace ApiPetFoundation.Application.DTOs.Pets
{
    /// <summary>Datos parciales para actualizar una mascota. Ej: solo nombre.</summary>
    public class PatchPetRequest
    {
        /// <summary>Nombre de la mascota. Ej: Serafin</summary>
        public string? Name { get; init; }
        /// <summary>Especie de la mascota. Ej: Perro</summary>
        public string? Species { get; init; }
        /// <summary>Raza de la mascota. Ej: Schnauzer</summary>
        public string? Breed { get; init; }
        /// <summary>Edad de la mascota. Ej: 3</summary>
        public int? Age { get; init; }
        /// <summary>Sexo de la mascota. Ej: Macho</summary>
        public string? Sex { get; init; }
        /// <summary>Tamano de la mascota. Ej: Mediano</summary>
        public string? Size { get; init; }
        /// <summary>Descripcion de la mascota. Ej: Perrito jugueton.</summary>
        public string? Description { get; init; }
    }
}

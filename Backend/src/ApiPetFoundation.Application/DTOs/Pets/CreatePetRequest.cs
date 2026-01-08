namespace ApiPetFoundation.Application.DTOs.Pets
{
    /// <summary>Datos para crear una mascota. Ej: perro con nombre y descripcion.</summary>
    public class CreatePetRequest
    {
        /// <summary>Nombre de la mascota. Ej: Serafin</summary>
        public required string Name { get; init; }
        /// <summary>Especie de la mascota. Ej: Perro</summary>
        public required string Species { get; init; }
        /// <summary>Raza de la mascota. Ej: Schnauzer</summary>
        public required string Breed { get; init; }
        /// <summary>Edad de la mascota. Ej: 3</summary>
        public required int Age { get; init; }
        /// <summary>Sexo de la mascota. Ej: Macho</summary>
        public required string Sex { get; init; }
        /// <summary>Tamano de la mascota. Ej: Mediano</summary>
        public required string Size { get; init; }
        /// <summary>Descripcion de la mascota. Ej: Perrito jugueton.</summary>
        public required string Description { get; init; }
    }
}


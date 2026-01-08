using ApiPetFoundation.Domain.Constants;

namespace ApiPetFoundation.Domain.Entities
{
    public class Pet
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Species { get; private set; } = string.Empty;
        public string? Breed { get; private set; }
        public int Age { get; private set; }
        public string Sex { get; private set; } = string.Empty; // "Macho" / "Hembra"
        public string Size { get; private set; } = string.Empty; // "Peque¤o", "Mediano", "Grande"
        public string Description { get; private set; } = string.Empty;
        public string Status { get; private set; } = PetStatuses.Available;
        public int CreatedById { get; private set; } // FK  User
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        // ?? Relaciones
        public User? CreatedBy { get; private set; }
        public ICollection<PetImage>? Images { get; private set; }
        public ICollection<AdoptionRequest>? AdoptionRequests { get; private set; }

        private Pet() { }

        public static Pet Create(
            string name,
            string species,
            string? breed,
            int age,
            string sex,
            string size,
            string description,
            int createdById)
        {
            return new Pet
            {
                Name = name,
                Species = species,
                Breed = breed,
                Age = age,
                Sex = sex,
                Size = size,
                Description = description,
                Status = PetStatuses.Available,
                CreatedById = createdById,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void UpdateDetails(
            string name,
            string species,
            string? breed,
            int age,
            string sex,
            string size,
            string description)
        {
            Name = name;
            Species = species;
            Breed = breed;
            Age = age;
            Sex = sex;
            Size = size;
            Description = description;
            UpdatedAt = DateTime.UtcNow;
        }

        public void PatchDetails(
            string? name,
            string? species,
            string? breed,
            int? age,
            string? sex,
            string? size,
            string? description)
        {
            if (name != null)
                Name = name;
            if (species != null)
                Species = species;
            if (breed != null)
                Breed = breed;
            if (age.HasValue)
                Age = age.Value;
            if (sex != null)
                Sex = sex;
            if (size != null)
                Size = size;
            if (description != null)
                Description = description;

            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsAvailable()
        {
            return Status == PetStatuses.Available;
        }

        public void MarkPending()
        {
            if (Status != PetStatuses.Available)
                throw new InvalidOperationException("Pet must be available to mark as pending.");

            Status = PetStatuses.Pending;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAdopted()
        {
            if (Status != PetStatuses.Pending)
                throw new InvalidOperationException("Pet must be pending to mark as adopted.");

            Status = PetStatuses.Adopted;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAvailable()
        {
            Status = PetStatuses.Available;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

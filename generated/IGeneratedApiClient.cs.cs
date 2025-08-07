using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GeneratedApiClient.Models;

namespace GeneratedApiClient
{
    public interface IGeneratedApiClient
    {
        /// <summary>
        /// List all pets
        /// </summary>
        Task<List<Pet>> ListPets(CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a pet
        /// </summary>
        Task<Pet> CreatePet(CreatePetRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a pet by ID
        /// </summary>
        Task<Pet> GetPetById(long petId, CancellationToken cancellationToken = default);

    }
}

using ApiPetFoundation.Application.Interfaces.Repositories;
using ApiPetFoundation.Application.Interfaces.Services;
using ApiPetFoundation.Domain.Entities;
using System;

namespace ApiPetFoundation.Application.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserRepository _userRepository;

    public UserProfileService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task CreateProfileAsync(User user)
    {
        await _userRepository.AddAsync(user);
    }

    public async Task<User?> GetByIdentityUserIdAsync(string identityUserId)
    {
        return await _userRepository.GetByIdentityUserIdAsync(identityUserId);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName)
    {
        return await _userRepository.GetUsersByRoleAsync(roleName);
    }
}

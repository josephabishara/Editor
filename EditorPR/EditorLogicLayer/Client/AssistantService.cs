using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using EditorViewModelLayer.ClientViewModel;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.Client
{
    public class AssistantService : IAssistantService
    {
        private readonly IAssistantRepository _assistantRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public AssistantService(
            IAssistantRepository assistantRepo,
            UserManager<ApplicationUser> userManager)
        {
            _assistantRepo = assistantRepo;
            _userManager = userManager;
        }

        public async Task<IEnumerable<AssistantDTO>> GetAssistantsByClientAsync(int clientId)
        {
            var list = await _assistantRepo.GetByClientIdAsync(clientId);
            return list.Select(MapAssistantToDTO);
        }

        public async Task<AssistantDTO?> GetAssistantByIdAsync(int id)
        {
            var a = await _assistantRepo.GetByIdAsync(id);
            return a == null ? null : MapAssistantToDTO(a);
        }

        public async Task<(bool Success, string Message)> CreateAssistantAsync(AssistantDTO model)
        {
            if (await _assistantRepo.EmailExistsAsync(model.Email))
                return (false, "An assistant with this email already exists.");

            if (await _assistantRepo.UsernameExistsAsync(model.Username))
                return (false, "An assistant with this username already exists.");

            if (string.IsNullOrWhiteSpace(model.Password))
                return (false, "Password is required when creating an assistant.");

            var user = new ApplicationUser
            {
                FullName = model.Name,
                UserName = model.Username,
                Email = model.Email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var userResult = await _userManager.CreateAsync(user, model.Password);
            if (!userResult.Succeeded)
            {
                var errors = string.Join(", ", userResult.Errors.Select(e => e.Description));
                return (false, errors);
            }

            await _userManager.AddToRoleAsync(user, "Assistant");

            var entity = new Assistant
            {
                ClientId = model.ClientId,
                Name = model.Name,
                Email = model.Email,
                Username = model.Username,
                Photo = model.Photo,
                Status = model.Status,
                ApplicationUserId = user.Id,
                IsActive = true,           // BUG 6: was missing — Active is from BaseEntity
                Deleted = 0,              // BUG 6: was missing — Deleted is from BaseEntity
                CreatedAt = DateTime.UtcNow // BUG 6: was CreatedAt — BaseEntity uses CreatedDate
            };

            await _assistantRepo.AddAsync(entity);
            return (true, "Assistant created successfully.");
        }

        public async Task<(bool Success, string Message)> UpdateAssistantAsync(AssistantDTO model)
        {
            var existing = await _assistantRepo.GetByIdAsync(model.Id);
            if (existing == null)
                return (false, "Assistant not found.");

            if (await _assistantRepo.EmailExistsAsync(model.Email, model.Id))
                return (false, "Another assistant with this email already exists.");

            if (await _assistantRepo.UsernameExistsAsync(model.Username, model.Id))
                return (false, "Another assistant with this username already exists.");

            if (existing.ApplicationUserId.HasValue)
            {
                var user = await _userManager.FindByIdAsync(existing.ApplicationUserId.Value.ToString());
                if (user != null)
                {
                    user.FullName = model.Name;
                    user.Email = model.Email;
                    user.UserName = model.Username;

                    if (!string.IsNullOrWhiteSpace(model.Password))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        await _userManager.ResetPasswordAsync(user, token, model.Password);
                    }

                    await _userManager.UpdateAsync(user);
                }
            }

            existing.Name = model.Name;
            existing.Email = model.Email;
            existing.Username = model.Username;
            existing.Photo = model.Photo;
            existing.Status = model.Status;
            existing.UpdatedAt = DateTime.UtcNow; // BUG 7: was UpdatedAt — BaseEntity uses UpdatedDate

            await _assistantRepo.UpdateAsync(existing);
            return (true, "Assistant updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeleteAssistantAsync(int id)
        {
            var existing = await _assistantRepo.GetByIdAsync(id);
            if (existing == null)
                return (false, "Assistant not found.");

            if (existing.ApplicationUserId.HasValue)
            {
                var user = await _userManager.FindByIdAsync(existing.ApplicationUserId.Value.ToString());
                if (user != null)
                {
                    user.IsActive = false;
                    await _userManager.UpdateAsync(user);
                }
            }

            existing.Deleted = 1;
            existing.IsActive = false;           // BUG 8: was IsActive — BaseEntity uses Active not IsActive
            existing.DeletedAt = DateTime.UtcNow; // BUG 8: was DeletedAt — BaseEntity uses DeletedDate

            await _assistantRepo.UpdateAsync(existing);
            return (true, "Assistant deleted successfully.");
        }

        private static AssistantDTO MapAssistantToDTO(Assistant a) => new()
        {
            Id = a.Id,
            ClientId = a.ClientId,
            ClientName = a.Client?.Name,
            Name = a.Name,
            Email = a.Email,
            Username = a.Username,
            Photo = a.Photo,
            Status = a.Status,
            ApplicationUserId = a.ApplicationUserId
        };
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services.CharacterService
{
    public class CharacterService : ICharacterService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CharacterService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters()
        {
            var response = new ServiceResponse<List<GetCharacterDto>>();

            List<Character> characters = await _context.Characters
                                                            .Where(c => c.User.Id == GetUserId())
                                                            .ToListAsync();

            response.Data = characters.Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();

            return response;
        }

        public async Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id)
        {
            var response = new ServiceResponse<GetCharacterDto>();
            Character character = await _context.Characters
                                                .Include(c => c.Weapon)
                                                .Include(c => c.CharacterSkills)
                                                .ThenInclude(cs => cs.Skill)
                                                .FirstOrDefaultAsync(c => c.Id == id && c.User.Id == GetUserId());

            response.Data = _mapper.Map<GetCharacterDto>(character);

            return response;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> AddCharacter(AddCharacterDto newCharacter)
        {
            var response = new ServiceResponse<List<GetCharacterDto>>();

            Character character = _mapper.Map<Character>(newCharacter);
            character.User = await _context.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());
            await _context.Characters.AddAsync(character);
            await _context.SaveChangesAsync();

            response.Data = await _context.Characters.Where(c => c.User.Id == GetUserId())
                                                            .Select(c => _mapper.Map<GetCharacterDto>(c))
                                                            .ToListAsync();

            return response;
        }

        public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto updatedCharacter)
        {
            var response = new ServiceResponse<GetCharacterDto>();

            try
            {
                Character character = await _context.Characters.Include(c => c.User)
                                                                .FirstOrDefaultAsync(c => c.Id == updatedCharacter.Id);

                if (character != null && character.User.Id == GetUserId())
                {
                    character.Name = updatedCharacter.Name;
                    character.Class = updatedCharacter.Class;
                    character.Defence = updatedCharacter.Defence;
                    character.HitPoints = updatedCharacter.HitPoints;
                    character.Intelligence = updatedCharacter.Intelligence;
                    character.Strength = updatedCharacter.Strength;

                    _context.Characters.Update(character);
                    await _context.SaveChangesAsync();

                    response.Data = _mapper.Map<GetCharacterDto>(character);
                }
                else
                {
                    response.Success = false;
                    response.Message = "Character not found.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> DeleteCharacter(int id)
        {
            var response = new ServiceResponse<List<GetCharacterDto>>();

            try
            {
                Character character = await _context.Characters.FirstOrDefaultAsync(c => c.Id == id && c.User.Id == GetUserId());

                if (character != null)
                {
                    _context.Characters.Remove(character);
                    await _context.SaveChangesAsync();

                    response.Data = _context.Characters.Where(c => c.User.Id == GetUserId())
                                                                .Select(c => _mapper.Map<GetCharacterDto>(c))
                                                                .ToList();
                }
                else
                {
                    response.Success = false;
                    response.Message = "Character not found.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }
    }
}

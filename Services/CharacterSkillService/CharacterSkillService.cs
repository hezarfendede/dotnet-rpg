﻿using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Dtos.CharacterSkill;
using dotnet_rpg.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace dotnet_rpg.Services.CharacterSkillService
{
    public class CharacterSkillService : ICharacterSkillService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public CharacterSkillService(DataContext context, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        public async Task<ServiceResponse<GetCharacterDto>> AddCharacterSkill(AddCharacterSkillDto newCharacterSkill)
        {
            var response = new ServiceResponse<GetCharacterDto>();

            try
            {
                Character character = await _context.Characters
                                                    .Include(c => c.Weapon)
                                                    .Include(c => c.CharacterSkills)
                                                    .ThenInclude(c => c.Skill)
                                                    .FirstOrDefaultAsync(c => c.Id == newCharacterSkill.CharacterId
                                                                                        && c.User.Id == GetUserId());

                if (character == null)
                {
                    response.Success = false;
                    response.Message = "Character not found.";
                }
                else
                {
                    Skill skill = await _context.Skills.FirstOrDefaultAsync(s => s.Id == newCharacterSkill.SkillId);

                    if (skill == null)
                    {
                        response.Success = false;
                        response.Message = "Skill not found.";
                    }
                    else
                    {
                        CharacterSkill characterSkill = new CharacterSkill
                        {
                            Character = character,
                            Skill = skill
                        };
                        await _context.CharacterSkills.AddAsync(characterSkill);
                        await _context.SaveChangesAsync();

                        response.Data = _mapper.Map<GetCharacterDto>(character);
                    }
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
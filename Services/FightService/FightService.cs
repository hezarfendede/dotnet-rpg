using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Fight;
using dotnet_rpg.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.Services.FightService
{
    public class FightService : IFightService
    {
        private readonly DataContext _context;

        public FightService(DataContext context)
        {
            _context = context;
        }

        // TODO: UserId check
        // TODO: Character null check
        // TODO: Character dead check
        // TODO: Weapon null check
        // TODO: User CharacterService
        public async Task<ServiceResponse<AttackResultDto>> WeaponAttack(WeaponAttackDto request)
        {
            var response = new ServiceResponse<AttackResultDto>();

            try
            {
                Character attacker = await _context.Characters.Include(c => c.Weapon)
                                                                .FirstOrDefaultAsync(c => c.Id == request.AttackerId);
                Character opponent = await _context.Characters.FirstOrDefaultAsync(c => c.Id == request.OpponentId);

                int damage = attacker.Weapon.Damage + (new Random().Next(attacker.Strength));
                damage -= new Random().Next(opponent.Defence);

                if (damage > 0)
                {
                    opponent.HitPoints -= damage;
                }

                if (opponent.HitPoints <= 0)
                {
                    response.Message = $"{opponent.Name} has been defeated!";
                }

                _context.Characters.Update(opponent);
                await _context.SaveChangesAsync();

                response.Data = new AttackResultDto
                {
                    Attacker = attacker.Name,
                    Opponent = opponent.Name,
                    AttackerHP = attacker.HitPoints,
                    OpponentHP = opponent.HitPoints,
                    Damage = damage
                };
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<AttackResultDto>> SkillAttack(SkillAttackDto request)
        {
            var response = new ServiceResponse<AttackResultDto>();

            try
            {
                Character attacker = await _context.Characters.Include(c => c.CharacterSkills)
                                                                .ThenInclude(cs => cs.Skill)
                                                                .FirstOrDefaultAsync(c => c.Id == request.AttackerId);
                Character opponent = await _context.Characters.FirstOrDefaultAsync(c => c.Id == request.OpponentId);

                CharacterSkill characterSkill = attacker.CharacterSkills.FirstOrDefault(cs => cs.SkillId == request.SkillId);
                if (characterSkill == null)
                {
                    response.Success = false;
                    response.Message = $"{attacker.Name} does not have that skill.";
                }
                else
                {
                    int damage = characterSkill.Skill.Damage + (new Random().Next(attacker.Intelligence));
                    damage -= new Random().Next(opponent.Defence);

                    if (damage > 0)
                    {
                        opponent.HitPoints -= damage;
                    }

                    if (opponent.HitPoints <= 0)
                    {
                        response.Message = $"{opponent.Name} has been defeated!";
                    }

                    _context.Characters.Update(opponent);
                    await _context.SaveChangesAsync();

                    response.Data = new AttackResultDto
                    {
                        Attacker = attacker.Name,
                        Opponent = opponent.Name,
                        AttackerHP = attacker.HitPoints,
                        OpponentHP = opponent.HitPoints,
                        Damage = damage
                    };
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

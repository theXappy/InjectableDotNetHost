//
//  SkillSelector.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Data.Abstractions.Enums;
using NosSmooth.Extensions.Combat.Errors;
using NosSmooth.Extensions.Combat.Selectors;
using NosSmooth.Game.Data.Characters;
using Remora.Results;

namespace SimplePiiBot;

/// <summary>
/// Selects skill for the pii bot.
/// </summary>
public class SkillSelector : ISkillSelector
{
    private bool _isPii;

    /// <summary>
    /// Initializes a new instance of the <see cref="SkillSelector"/> class.
    /// </summary>
    /// <param name="isPii">Whether the target entity is a pii (false for pii pod).</param>
    public SkillSelector(bool isPii)
    {
        _isPii = isPii;
    }

    /// <inheritdoc />
    public Result<Skill> GetSelectedSkill(IEnumerable<Skill> usableSkills)
    {
        var skills = usableSkills.ToList();

        var skill = skills.MaxBy(x => x.Info!.Range);
        if (_isPii)
        {
            // try to find skill that does area damage
            skill = skills.MinBy(x => x.Info!.HitType == HitType.EnemiesInZone ? 0 : 1);
        }

        if (skill is null)
        {
            return new SkillNotFoundError();
        }

        return skill;
    }
}
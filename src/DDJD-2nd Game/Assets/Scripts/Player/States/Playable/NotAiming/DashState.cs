using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashState : MeleeAttackableState
{
    private Player _context;
    private DashStats _stats;
    private DashSkill _skill;
    private Rigidbody _rigidbody;
    private Dashable _dashable;
    private CharacterStatus _status;
    private GameUI _gameUI;

    public DashState(
        StateContext context,
        GenericState superState,
        DashStats stats,
        DashSkill skill
    )
        : base(context, superState)
    {
        _context = (Player)context;
        _stats = stats;
        _skill = skill;
        _rigidbody = _context.GetComponent<Rigidbody>();
        _status = _context.GetComponent<CharacterStatus>();
        _gameUI = _context.GetComponent<UIController>().PlayerUI.playingUI.GetComponent<GameUI>();
    }

    public override void Enter()
    {
        base.Enter();
        _dashable = _context.GetComponent<Dashable>();

        bool canUseSkill =
            _skill != null
            && !_context.PlayerSkills.IsSkillOnCooldown(_skill, true)
            && (_skill.CanDashInAir || MovementUtils.IsGrounded(_context.Rigidbody))
            && _status.ConsumeMana(_skill.Element, _skill.SkillStats.ManaCost);
        if (canUseSkill)
        {
            _dashable.DashWithSkill(_skill);
            _gameUI.startChargingIndicator();
            _context.PlayerSkills.StartSkillCooldown(_skill, true);
        }
        else
        {
            _dashable.Dash(_stats);
            _context.SoundEmitter.SetParameterWithLabel(
                "dash",
                _context.SfxDashStateId,
                "Basic",
                true
            );
        }
    }

    public override bool CanChangeState(GenericState state)
    {
        if (!base.CanChangeState(state))
        {
            return false;
        }
        return !_dashable.IsDashing;
    }
}

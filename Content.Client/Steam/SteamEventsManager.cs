using Content.Shared.Mobs;
using Content.Shared.Projectiles;
using Robust.Client.Player;
using Robust.Client.Utility;
using Robust.Shared.Player;

namespace Content.Client.Steam;

public sealed class SteamEventsManager : EntitySystem
{
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly ISteamManager _steam = default!;
    [Dependency] private IPlayerManager _playerManager = default!;

    private ISawmill _logger = default!;

    public override void Initialize()
    {
        base.Initialize();

        _logger = _logManager.GetSawmill("steam.events");
        _logger.Debug("Steam Init");

        SubscribeLocalEvent<LocalPlayerAttachedEvent>(OnAttach);
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnDetach);
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<ProjectileHitEvent>(OnProjectileHit);
    }

    private void OnProjectileHit(ProjectileHitEvent ev)
    {
        // throw new NotImplementedException();
    }

    private void OnMobStateChanged(MobStateChangedEvent ev)
    {
        _logger.Debug($"OnMobStateChanged {ev}");

        var player = _playerManager.LocalSession?.AttachedEntity;
        if (player == null)
            return;

        if (ev.Target == player)
        {
            _logger.Debug($"Player changed: {ev.NewMobState}");
            if (ev.NewMobState == MobState.Dead)
                _steam.AddInstantaneousTimelineEvent("Dead", "You died!", "steam_death", 0, 0);
        }
        else if (ev.Origin == player)
        {
            if (ev.NewMobState == MobState.Dead)
                _steam.AddInstantaneousTimelineEvent("Kill", $"You killed {ToPrettyString(ev.Target)}", "steam_combat", 0, 0);
            else if (ev.NewMobState == MobState.Critical)
                _steam.AddInstantaneousTimelineEvent("Kill", $"You crit {ToPrettyString(ev.Target)}", "steam_combat", 0, 0);
        }
    }

    private void OnAttach(LocalPlayerAttachedEvent ev)
    {
        _logger.Debug($"Steam OnAttach: {ToPrettyString(ev.Entity)}");
        // _steam.SetTimelineTooltip(ToPrettyString(ev.Entity), 0);
        if (TryComp(ev.Entity, out MetaDataComponent? metadata))
            _steam.SetTimelineTooltip(metadata.EntityName, 0);
    }

    private void OnDetach(LocalPlayerDetachedEvent ev)
    {
        _logger.Debug($"Detached: {ev}");
    }
}

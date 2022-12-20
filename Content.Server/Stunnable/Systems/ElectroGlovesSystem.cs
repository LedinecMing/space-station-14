using Content.Server.Power.Components;
using Content.Server.Power.Events;
using Content.Server.Stunnable.Components;
using Content.Shared.Audio;
using Content.Shared.Damage.Events;
using Content.Shared.Examine;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Popups;
using Content.Shared.Toggleable;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Player;

namespace Content.Server.Stunnable.Systems
{
    public sealed class ElectroGlovesSystem : EntitySystem
    {
        [Dependency] private readonly SharedItemSystem _item = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ElectroGlovesComponent, StaminaDamageOnHitAttemptEvent>(OnStaminaHitAttempt);
        }

        private void OnStaminaHitAttempt(EntityUid uid, ElectroGlovesComponent component, ref StaminaDamageOnHitAttemptEvent args)
        {
            if (!TryComp<BatteryComponent>(uid, out var battery) || !battery.TryUseCharge(component.EnergyPerUse))
            {
                args.Cancelled = true;
                return;
            }

            args.HitSoundOverride = component.StunSound;
        }

        private void SendPowerPulse(EntityUid target, EntityUid? user, EntityUid used)
        {
            RaiseLocalEvent(target, new PowerPulseEvent()
            {
                Used = used,
                User = user
            }, false);
        }
    }
}

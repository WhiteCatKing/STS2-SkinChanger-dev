using System;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MeirinWatcherSkin.Characters.Meirin;

internal static class MeirinWatcherAnimator
{
	public static CreatureAnimator Create(MegaSprite controller)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Expected O, but got Unknown
		string idleAnimation = MeirinSkinProfile.Profile.CombatBody.IdleAnimation;
		AnimState val = new AnimState(idleAnimation, true);
		AnimState val2 = new AnimState(idleAnimation, true);
		AnimState val3 = new AnimState("hit", false)
		{
			NextState = val
		};
		AnimState val4 = new AnimState("attack_ready", false)
		{
			NextState = val
		};
		AnimState val5 = new AnimState("defense_ready", false)
		{
			NextState = val
		};
		AnimState val6 = new AnimState("buff_ready", false)
		{
			NextState = val
		};
		AnimState val7 = new AnimState("death_ready", false);
		CreatureAnimator val8 = new CreatureAnimator(val, controller);
		val8.AddAnyState("Idle", val, (Func<bool>)null);
		val8.AddAnyState("Relaxed", val2, (Func<bool>)null);
		val8.AddAnyState("Hit", val3, (Func<bool>)null);
		val8.AddAnyState("Attack", val4, (Func<bool>)null);
		val8.AddAnyState("Cast", val5, (Func<bool>)null);
		val8.AddAnyState("PowerUp", val6, (Func<bool>)null);
		val8.AddAnyState("SignatureMove", val6, (Func<bool>)null);
		val8.AddAnyState("Dead", val7, (Func<bool>)null);
		return val8;
	}
}

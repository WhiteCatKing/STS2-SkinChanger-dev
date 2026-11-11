#nullable disable
using System;
using System.Reflection;
using Godot;
using HarmonyLib;

namespace STS2SkinChanger.Ui.EmperorCrabCompat
{
    /// <summary>
    /// RitsuLib 卡牌异步 Hook 兼容补丁。
    ///
    /// 局内换帝皇蟹皮肤后，RitsuLib 的 HarmonyAsyncTaskBridge 在卡牌异步
    /// continuation 里可能访问到已经释放的对象，抛出 ObjectDisposedException。
    /// 这个异常会一路抛回 PlayCardAction，导致卡牌动作没走完，牌停在半空。
    ///
    /// 这里只拦截 HarmonyAsyncTaskBridge.InvokeContinuation(Action) 抛出的
    /// ObjectDisposedException，其余异常原样放行，不改变 RitsuLib 的其他逻辑。
    /// </summary>
    internal static class RitsuLibCardGuardPatch
    {
        private static bool _installed;

        public static void Install(Harmony harmony)
        {
            if (_installed)
            {
                return;
            }

            try
            {
                Assembly ritsu = FindAssembly("STS2-RitsuLib.Runtime");
                if (ritsu == null)
                {
                    GD.Print("[EmperorCrabSkinFix] 未检测到 RitsuLib，跳过卡牌异步兼容补丁");
                    return;
                }

                Type bridge = ritsu.GetType(
                    "STS2RitsuLib.Utils.HarmonyIl.HarmonyAsyncTaskBridge",
                    false,
                    false);

                MethodInfo target = bridge?.GetMethod(
                    "InvokeContinuation",
                    BindingFlags.Static | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(Action) },
                    null);

                if (target == null)
                {
                    GD.PrintErr("[EmperorCrabSkinFix] 找不到 RitsuLib HarmonyAsyncTaskBridge.InvokeContinuation(Action)");
                    return;
                }

                harmony.Patch(
                    target,
                    finalizer: new HarmonyMethod(
                        typeof(RitsuLibCardGuardPatch).GetMethod(
                            nameof(Finalizer),
                            BindingFlags.Static | BindingFlags.NonPublic)));

                _installed = true;
                GD.Print("[EmperorCrabSkinFix] 已挂上 RitsuLib 卡牌异步兼容补丁");
            }
            catch (Exception ex)
            {
                GD.PrintErr("[EmperorCrabSkinFix] 安装 RitsuLib 兼容补丁失败：" + ex);
            }
        }

        private static Assembly FindAssembly(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == name)
                {
                    return assembly;
                }
            }
            return null;
        }

        private static Exception Finalizer(Exception __exception)
        {
            if (__exception is ObjectDisposedException)
            {
                GD.Print("[EmperorCrabSkinFix] 已忽略 RitsuLib 卡牌异步 continuation 的 ObjectDisposedException");
                return null;
            }

            return __exception;
        }
    }
}



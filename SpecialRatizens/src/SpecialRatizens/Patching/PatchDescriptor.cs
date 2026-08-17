using System;
using System.Reflection;
using HarmonyLib;

namespace SpecialRatizens.Patching
{
    internal enum PatchKind
    {
        Prefix,
        Postfix
    }

    internal sealed class PatchDescriptor
    {
        private readonly Func<MethodBase> _targetFactory;
        private readonly MethodInfo _patchMethod;

        public PatchDescriptor(string name, PatchKind kind, Func<MethodBase> targetFactory, MethodInfo patchMethod)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Kind = kind;
            _targetFactory = targetFactory ?? throw new ArgumentNullException(nameof(targetFactory));
            _patchMethod = patchMethod ?? throw new ArgumentNullException(nameof(patchMethod));
        }

        public string Name { get; }
        public PatchKind Kind { get; }

        public void Apply(Harmony harmony)
        {
            var target = _targetFactory();
            if (target == null)
            {
                throw new MissingMethodException($"找不到 Harmony 目标：{Name}");
            }

            var harmonyMethod = new HarmonyMethod(_patchMethod);
            if (Kind == PatchKind.Prefix)
            {
                harmony.Patch(target, prefix: harmonyMethod);
            }
            else
            {
                harmony.Patch(target, postfix: harmonyMethod);
            }
        }
    }
}

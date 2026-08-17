using System;
using System.Reflection;
using PopulationCustomizer.Core;
using Xunit;

namespace PopulationCustomizer.Tests
{
    public sealed class LimitRuntimeTests
    {
        [Fact]
        public void RuntimeStartsInVanillaModeAndCachesOriginalLimits()
        {
            var runtime = GetRuntimeType();
            Invoke(runtime, "Reset");

            Assert.Equal(123, Invoke<int>(runtime, "ResolveCitizen", 123));
            Assert.Equal(45, Invoke<int>(runtime, "ResolveRatron", 45));
            Assert.Equal(123, GetProperty<int>(runtime, "LastVanillaCitizenLimit"));
            Assert.Equal(45, GetProperty<int>(runtime, "LastVanillaRatronLimit"));
        }

        [Fact]
        public void RuntimeAppliesCitizenAndRatronSettingsIndependently()
        {
            var runtime = GetRuntimeType();
            Invoke(runtime, "Reset");
            Invoke(runtime, "Apply", new LimitSettings(true, 300, false, 100));

            Assert.Equal(300, Invoke<int>(runtime, "ResolveCitizen", 123));
            Assert.Equal(45, Invoke<int>(runtime, "ResolveRatron", 45));
        }

        [Fact]
        public void RuntimeResetRestoresVanillaBehavior()
        {
            var runtime = GetRuntimeType();
            Invoke(runtime, "Apply", new LimitSettings(true, 0, true, 999));
            Invoke(runtime, "Reset");

            Assert.Equal(123, Invoke<int>(runtime, "ResolveCitizen", 123));
            Assert.Equal(45, Invoke<int>(runtime, "ResolveRatron", 45));
        }

        private static Type GetRuntimeType()
        {
            var type = typeof(LimitSettings).Assembly.GetType("PopulationCustomizer.Runtime.LimitRuntime", false);
            Assert.NotNull(type);
            return type;
        }

        private static void Invoke(Type type, string name, params object[] arguments)
        {
            GetMethod(type, name).Invoke(null, arguments);
        }

        private static T Invoke<T>(Type type, string name, params object[] arguments)
        {
            return (T)GetMethod(type, name).Invoke(null, arguments);
        }

        private static T GetProperty<T>(Type type, string name)
        {
            var property = type.GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(property);
            return (T)property.GetValue(null, null);
        }

        private static MethodInfo GetMethod(Type type, string name)
        {
            var method = type.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return method;
        }
    }
}

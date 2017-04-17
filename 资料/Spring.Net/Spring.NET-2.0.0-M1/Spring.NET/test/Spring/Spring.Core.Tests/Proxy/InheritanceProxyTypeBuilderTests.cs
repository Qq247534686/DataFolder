#region License

/*
 * Copyright � 2002-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

#region Imports

using System;
using System.Reflection;

using NUnit.Framework;

using Spring.Util;

#endregion

namespace Spring.Proxy
{
    /// <summary>
    /// Unit tests for the InheritanceProxyTypeBuilder class.
    /// </summary>
    /// <author>Rick Evans</author>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public sealed class InheritanceProxyTypeBuilderTests : AbstractProxyTypeBuilderTests
    {
        [Test]
        public void OnClassThatDoesntImplementAnyInterfaces()
        {
            IProxyTypeBuilder builder = GetProxyBuilder();
            builder.TargetType = typeof(DoesntImplementAnyInterfaces);
            builder.BuildProxyType();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void WithSealedClass()
        {
            InheritanceProxyTypeBuilder builder = (InheritanceProxyTypeBuilder) GetProxyBuilder();
            builder.TargetType = typeof(SealedClass);
            builder.BuildProxyType();
        }

        [Test]
        public void ProxyInnerClass()
        {
            IProxyTypeBuilder builder = GetProxyBuilder();
            builder.TargetType = typeof(InnerClass);
            Type proxy = builder.BuildProxyType();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to BuildProxy() was null.");
            object foo = Activator.CreateInstance(proxy);
            Assert.IsTrue(foo is InnerInterface);
            Assert.IsTrue(foo is InnerClass);
        }

        [Test]
        public void CheckInheritType()
        {
            IProxyTypeBuilder builder = GetProxyBuilder();
            builder.TargetType = typeof(DoesntImplementAnyInterfaces);
            Type proxy = builder.BuildProxyType();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to BuildProxy() was null.");
            object foo = Activator.CreateInstance(proxy);
            Assert.IsTrue(foo is DoesntImplementAnyInterfaces,
                          "Using inheritance proxying, so the proxied object must inherit from the base type.");
        }

        [Test]
        public void OverrideVirtualMethod()
        {
            IProxyTypeBuilder builder = GetProxyBuilder();
            builder.TargetType = typeof(TargetObjectTest);
            Type proxy = builder.BuildProxyType();

            MethodInfo method = ReflectionUtils.GetMethod(proxy, "InterfaceVirtualMethodWithNoArguments", Type.EmptyTypes);
            
            Assert.IsNotNull(method, "Should define this method.");
            Assert.AreEqual(proxy, method.DeclaringType, "Method should be overrided in the proxy class.");

            // call overrided method
            object foo = Activator.CreateInstance(proxy);
            Assert.IsTrue(foo is TargetObjectTest);
            ((TargetObjectTest)foo).InterfaceVirtualMethodWithNoArguments();
        }

        [Test]
        public void ImplementFinalMethodThatImplementsInterface()
        {
            IProxyTypeBuilder builder = GetProxyBuilder();
            builder.TargetType = typeof(TargetObjectTest);
            Type proxy = builder.BuildProxyType();

            MethodInfo method = ReflectionUtils.GetMethod(proxy, "Spring.Proxy.ITargetObjectTest.InterfaceMethodWithArguments", new Type[] { typeof(string) });

            Assert.IsNotNull(method, "Should define this method.");
            Assert.AreEqual(proxy, method.DeclaringType, "Method should be defined in the proxy class.");

            // call final interfaced method
            object foo = Activator.CreateInstance(proxy);
            Assert.IsTrue(foo is ITargetObjectTest);
            ((ITargetObjectTest)foo).InterfaceMethodWithArguments("code");
        }

        [Test]
        public void DoesNotImplementFinalMethodThatDoesNotImplementInterface()
        {
            IProxyTypeBuilder builder = GetProxyBuilder();
            builder.TargetType = typeof(TargetObjectTest);
            Type proxy = builder.BuildProxyType();

            MethodInfo method = ReflectionUtils.GetMethod(proxy, "MethodWithArguments", new Type[] { typeof(string) });

            Assert.IsNotNull(method, "Should define this method.");
            Assert.AreNotEqual(proxy, method.DeclaringType, "Method can't be proxied.");

            // call base method
            object foo = Activator.CreateInstance(proxy);
            Assert.IsTrue(foo is TargetObjectTest);
            ((TargetObjectTest)foo).MethodWithArguments("code");
        }

        [Test]
        public void SetsInterfacesToProxy()
        {
            IProxyTypeBuilder builder = GetProxyBuilder();
            builder.TargetType = typeof(MultipleInterfaces);
            builder.Interfaces = new Type[] { typeof(IAnotherMarkerInterface) };

            Type proxy = builder.BuildProxyType();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to BuildProxy() was null.");

            object foo = Activator.CreateInstance(proxy);
            Assert.IsTrue(foo is IBase);
            Assert.IsTrue(foo is IInherited);
            Assert.IsTrue(foo is IAnotherMarkerInterface);

            // try to call proxied interface methods
            ((IBase)foo).Base();
            ((IInherited)foo).Base();
            ((IInherited)foo).Inherited();
        }


        [Test]
        public void DoesNotProxyNonProxiableInterface()
        {
            IProxyTypeBuilder builder = GetProxyBuilder();
            builder.TargetType = typeof(ApplicationClass);

            Type proxy = builder.BuildProxyType();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to BuildProxy() was null.");

            object foo = Activator.CreateInstance(proxy);
            Assert.IsTrue(foo is IApplicationInterface);
            Assert.IsTrue(foo is IFrameworkInterface);

            // proxy implements IApplicationInterface and proxy methods
            Assert.IsNotNull(foo.GetType().GetMethod("Spring.Proxy.IApplicationInterface.ApplicationMethod", BindingFlags.Instance | BindingFlags.NonPublic));

            // proxy implements IFrameworkInterface but should not proxy methods
            Assert.IsNull(foo.GetType().GetMethod("Spring.Proxy.IFrameworkInterface.FrameworkMethod", BindingFlags.Instance | BindingFlags.NonPublic));
        }

        [Test]
        public void ForcesNonProxiableInterfacesToBeProxied()
        {
            IProxyTypeBuilder builder = GetProxyBuilder();
            builder.TargetType = typeof(ApplicationClass);
            builder.Interfaces = new Type[] { typeof(IFrameworkInterface) };

            Type proxy = builder.BuildProxyType();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to BuildProxy() was null.");

            object foo = Activator.CreateInstance(proxy);
            Assert.IsTrue(foo is IApplicationInterface);
            Assert.IsTrue(foo is IFrameworkInterface);

            // proxy implements IFrameworkInterface and proxy methods
            Assert.IsNotNull(foo.GetType().GetMethod("Spring.Proxy.IFrameworkInterface.FrameworkMethod", BindingFlags.Instance | BindingFlags.NonPublic));

            // proxy implements IApplicationInterface but should not proxy methods
            Assert.IsNull(foo.GetType().GetMethod("Spring.Proxy.IApplicationInterface.ApplicationMethod", BindingFlags.Instance | BindingFlags.NonPublic));
        }

        [Test]
        public void ProxyTargetVirtualMethodAttributes()
        {
            IProxyTypeBuilder builder = GetProxyBuilder();
            builder.TargetType = typeof(AnotherMarkerClass);
            Type proxy = builder.BuildProxyType();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to BuildProxy() was null.");
            MethodInfo method = proxy.GetMethod("MarkerVirtualMethod");
            Assert.IsNotNull(method);
            object[] attrs = method.GetCustomAttributes(false);
            Assert.IsNotNull(attrs, "Should have 1 attribute applied to the target method.");
            Assert.AreEqual(1, attrs.Length, "Should have 1 attribute applied to the target method.");
            Assert.AreEqual(typeof(MarkerAttribute), attrs[0].GetType(), "Wrong System.Type of Attribute applied to the target method.");
        }

        [Test]
        public void DoesNotProxyTargetVirtualMethodAttributesWithProxyTargetAttributesEqualsFalse()
        {
            IProxyTypeBuilder builder = GetProxyBuilder();
            builder.TargetType = typeof(AnotherMarkerClass);
            builder.ProxyTargetAttributes = false;
            Type proxy = builder.BuildProxyType();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to BuildProxy() was null.");
            MethodInfo method = proxy.GetMethod("MarkerVirtualMethod");
            Assert.IsNotNull(method);
            object[] attrs = method.GetCustomAttributes(false);
            Assert.IsNotNull(attrs);
            Assert.AreEqual(0, attrs.Length, "Should not have attribute applied to the target method.");
        }

        [Test]
        public void ProxyTargetVirtualMethodParameterAttributes()
        {
            IProxyTypeBuilder builder = GetProxyBuilder();
            builder.TargetType = typeof(SomeMarkerClass);
            Type proxy = builder.BuildProxyType();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to BuildProxy() was null.");
            MethodInfo method = proxy.GetMethod("MarkerVirtualMethod");
            Assert.IsNotNull(method);
            object[] attrs = method.GetParameters()[1].GetCustomAttributes(false);
            Assert.IsNotNull(attrs, "Should have had 1 attribute applied to the method's parameter.");
            Assert.AreEqual(1, attrs.Length, "Should have had 1 attribute applied to the method's parameter.");
            Assert.AreEqual(typeof(MarkerAttribute), attrs[0].GetType(), "Wrong System.Type of Attribute applied to the method's parameter.");
        }

        [Test]
        [Description("SPRNET-1134")]
        public void ProxyTargetVirtualMethodParameterMultipleAttributes()
        {
            IProxyTypeBuilder builder = GetProxyBuilder();
            builder.TargetType = typeof(MultipleMarkerClass);
            Type proxy = builder.BuildProxyType();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to BuildProxy() was null.");
            MethodInfo method = proxy.GetMethod("MarkerVirtualMethod");
            Assert.IsNotNull(method);
            object[] attrs = method.GetParameters()[1].GetCustomAttributes(false);
            Assert.IsNotNull(attrs, "Should have had 2 attribute applied to the method's parameter.");
            Assert.AreEqual(2, attrs.Length, "Should have had 2 attribute applied to the method's parameter.");
            Assert.AreEqual(typeof(MarkerAttribute), attrs[0].GetType(), "Wrong System.Type of Attribute applied to the method's parameter.");
            Assert.AreEqual(typeof(MarkerAttribute), attrs[1].GetType(), "Wrong System.Type of Attribute applied to the method's parameter.");
        }

        [Test]
        public void DoesNotProxyTargetVirtualMethodParameterAttributesWithProxyTargetAttributesEqualsFalse()
        {
            IProxyTypeBuilder builder = GetProxyBuilder();
            builder.TargetType = typeof(SomeMarkerClass);
            builder.ProxyTargetAttributes = false;
            Type proxy = builder.BuildProxyType();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to BuildProxy() was null.");
            MethodInfo method = proxy.GetMethod("MarkerVirtualMethod");
            Assert.IsNotNull(method);
            object[] attrs = method.GetParameters()[1].GetCustomAttributes(false);
            Assert.IsNotNull(attrs);
            Assert.AreEqual(0, attrs.Length, "Should not have attribute applied to the method's parameter.");
        }

        [Test]
        public void ProxyTargetVirtualMethodReturnValueAttributes()
        {
            IProxyTypeBuilder builder = GetProxyBuilder();
            builder.TargetType = typeof(SomeMarkerClass);
            Type proxy = builder.BuildProxyType();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to BuildProxy() was null.");
            MethodInfo method = proxy.GetMethod("MarkerVirtualMethod");
            Assert.IsNotNull(method);
            object[] attrs = method.ReturnTypeCustomAttributes.GetCustomAttributes(false);
            Assert.IsNotNull(attrs, "Should have had 1 attribute applied to the method's return value.");
            Assert.AreEqual(1, attrs.Length, "Should have had 1 attribute applied to the method's return value.");
            Assert.AreEqual(typeof(MarkerAttribute), attrs[0].GetType(), "Wrong System.Type of Attribute applied to the method's return value.");
        }

        [Test]
        [Description("SPRNET-1134")]
        public void ProxyTargetVirtualMethodReturnValueMultipleAttributes()
        {
            IProxyTypeBuilder builder = GetProxyBuilder();
            builder.TargetType = typeof(MultipleMarkerClass);
            Type proxy = builder.BuildProxyType();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to BuildProxy() was null.");
            MethodInfo method = proxy.GetMethod("MarkerVirtualMethod");
            Assert.IsNotNull(method);
            object[] attrs = method.ReturnTypeCustomAttributes.GetCustomAttributes(false);
            Assert.IsNotNull(attrs, "Should have had 2 attribute applied to the method's return value.");
            Assert.AreEqual(2, attrs.Length, "Should have had 2 attribute applied to the method's return value.");
            Assert.AreEqual(typeof(MarkerAttribute), attrs[0].GetType(), "Wrong System.Type of Attribute applied to the method's return value.");
            Assert.AreEqual(typeof(MarkerAttribute), attrs[1].GetType(), "Wrong System.Type of Attribute applied to the method's return value.");
        }

        [Test]
        public void DoesNotProxyTargetVirtualMethodReturnValueAttributesWithProxyTargetAttributesEqualsFalse()
        {
            IProxyTypeBuilder builder = GetProxyBuilder();
            builder.TargetType = typeof(SomeMarkerClass);
            builder.ProxyTargetAttributes = false;
            Type proxy = builder.BuildProxyType();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to BuildProxy() was null.");
            MethodInfo method = proxy.GetMethod("MarkerVirtualMethod");
            Assert.IsNotNull(method);
            object[] attrs = method.ReturnTypeCustomAttributes.GetCustomAttributes(false);
            Assert.IsNotNull(attrs);
            Assert.AreEqual(0, attrs.Length, "Should not have attribute applied to the method's return value.");
        }

        protected override IProxyTypeBuilder GetProxyBuilder()
        {
            return new InheritanceProxyTypeBuilder();
        }
    }

    #region Helper Classes

    public class NotSealedClass
    {
        public bool IsAnyoneThere()
        {
            return false;
        }
    }

    public sealed class SealedClass
    {}

    #endregion
}
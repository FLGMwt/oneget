// 
//  Copyright (c) Microsoft Corporation. All rights reserved. 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  

namespace Microsoft.OneGet.Utility.Plugin {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Collections;
    using Extensions;

    internal static class DynamicInterfaceExtensions {
        private readonly static Type[] _emptyTypes = { };

        private static readonly Dictionary<Type, MethodInfo[]> _methodCache = new Dictionary<Type, MethodInfo[]>();
        private static readonly Dictionary<Type[], MethodInfo[]> _methodCacheForTypes = new Dictionary<Type[], MethodInfo[]>();
        private static readonly Dictionary<Type, FieldInfo[]> _delegateFieldsCache = new Dictionary<Type, FieldInfo[]>();
        private static readonly Dictionary<Type, PropertyInfo[]> _delegatePropertiesCache = new Dictionary<Type, PropertyInfo[]>();
        private static readonly Dictionary<Type, MethodInfo[]> _requiredMethodsCache = new Dictionary<Type, MethodInfo[]>();
        private static readonly Dictionary<Type, MethodInfo[]> _virtualMethodsCache = new Dictionary<Type, MethodInfo[]>();
        private static readonly Dictionary<Assembly, Type[]> _creatableTypesCache = new Dictionary<Assembly, Type[]>();

        public static MethodInfo FindMethod(this MethodInfo[] methods, MethodInfo methodSignature) {
            return methods.FirstOrDefault(candidate => DoNamesMatchAcceptably(methodSignature.Name, candidate.Name) && DoSignaturesMatchAcceptably(methodSignature, candidate));
        }

        public static MethodInfo FindMethod(this MethodInfo[] methods, Type delegateType ) {
            return methods.FirstOrDefault(candidate => DoNamesMatchAcceptably(delegateType.Name, candidate.Name) && delegateType.IsDelegateAssignableFromMethod(candidate));
        }

        public static Delegate FindDelegate(this FieldInfo[] fields, object actualInstance, MethodInfo signature) {
            return (from field in fields
                let value = field.GetValue(actualInstance) as Delegate
                where DoNamesMatchAcceptably(signature.Name, field.Name) && field.FieldType.IsDelegateAssignableFromMethod(signature) && value != null
                select value).FirstOrDefault();
        }

        public static Delegate FindDelegate(this PropertyInfo[] properties, object actualInstance, MethodInfo signature) {
            return (from property in properties
                let value = property.GetValue(actualInstance,null) as Delegate
                where DoNamesMatchAcceptably(signature.Name, property.Name) && property.PropertyType.IsDelegateAssignableFromMethod(signature) && value != null
                select value).FirstOrDefault();
        }

        public static Delegate FindDelegate(this FieldInfo[] fields, object actualInstance, Type delegateType) {
            return (from candidate in fields
                    let value = candidate.GetValue(actualInstance) as Delegate
                    where value != null && DoNamesMatchAcceptably(delegateType.Name, candidate.Name) && delegateType.IsDelegateAssignableFromDelegate(value.GetType()) 
                    select value).FirstOrDefault();
        }

        public static Delegate FindDelegate(this PropertyInfo[] properties, object actualInstance, Type delegateType) {
            return (from candidate in properties
                    let value = candidate.GetValue(actualInstance, null) as Delegate
                    where value != null && DoNamesMatchAcceptably(delegateType.Name, candidate.Name) && delegateType.IsDelegateAssignableFromDelegate(value.GetType()) 
                    select value).FirstOrDefault();
        }

        private static bool DoNamesMatchAcceptably(string originalName, string candidateName) {
            if (originalName.EqualsIgnoreCase(candidateName)) {
                return true;
            }

            // transform non-leading underscores to nothing.
            if (!candidateName.StartsWith("_",StringComparison.OrdinalIgnoreCase)) {
                candidateName = candidateName.Replace("_", "");
            }

            if (originalName.EqualsIgnoreCase(candidateName)) {
                return true;
            }

            // get_ => get
            if (candidateName.StartsWith("get_", StringComparison.OrdinalIgnoreCase)) {
                if (originalName.EqualsIgnoreCase("get" + candidateName.Substring(4))) {
                    return true;
                }
            }

            return false;
        }

        private static bool DoSignaturesMatchAcceptably(MethodInfo member, MethodInfo candidate) {
            return candidate.GetParameterTypes().SequenceEqual(member.GetParameterTypes(), AssignableTypeComparer.Instance) && (member.ReturnType == candidate.ReturnType || member.ReturnType.IsAssignableFrom(candidate.ReturnType));
        }

        internal static MethodInfo[] GetPublicMethods(this Type type) {
            return _methodCache.GetOrAdd(type, () => {
                if (type != null) {
                    return type.GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
                }
                return new MethodInfo[0];
            });
        }

        internal static MethodInfo[] GetPublicMethods(this Type[] types) {
            return _methodCacheForTypes.GetOrAdd(types, () => types.SelectMany(each => each.GetPublicMethods()).ToArray());
        }

        internal static IEnumerable<FieldInfo> GetPublicFields(this Type type) {
            if (type != null) {
                return type.GetFields(BindingFlags.FlattenHierarchy|BindingFlags.Instance | BindingFlags.Public);
            }
            return Enumerable.Empty<FieldInfo>();
        }

        internal static FieldInfo[] GetPublicDelegateFields(this Type type) {
            return _delegateFieldsCache.GetOrAdd(type, () => type.GetPublicFields().Where(each => each.FieldType.BaseType == typeof (MulticastDelegate)).ToArray());
        }

        internal static PropertyInfo[] GetPublicDelegateProperties(this Type type) {
            return _delegatePropertiesCache.GetOrAdd(type, () => type.GetPublicProperties().Where(each => each.PropertyType.BaseType == typeof (MulticastDelegate)).ToArray());
        }

        internal static IEnumerable<PropertyInfo> GetPublicProperties(this Type type) {
            if (type != null) {
                return type.GetProperties(BindingFlags.FlattenHierarchy|BindingFlags.Instance | BindingFlags.Public);
            }
            return Enumerable.Empty<PropertyInfo>();
        }

        private static IEnumerable<MethodInfo> DisambiguateMethodsBySignature(params IEnumerable<MethodInfo>[] setsOfMethods) {
            var unique = new HashSet<string>();

            return setsOfMethods.SelectMany(methodSet => methodSet.ReEnumerable()).Where(method => {
                var sig = method.ToSignatureString();
                if (!unique.Contains(sig)) {
                    unique.Add(sig);
                    return true;
                }
                return false;
            });
        }

        internal static MethodInfo[] GetVirtualMethods(this Type type) {
            return _virtualMethodsCache.GetOrAdd(type, () => {
                var methods = (type.IsInterface
                    ? (IEnumerable<MethodInfo>)type.GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance)
                    : (IEnumerable<MethodInfo>)type.GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance).Where(each => each.IsAbstract || each.IsVirtual));

                var interfaceMethods = type.GetInterfaces().SelectMany(GetVirtualMethods);

                return DisambiguateMethodsBySignature(methods, interfaceMethods).ToArray();
            });
        }

        internal static MethodInfo[] GetRequiredMethods(this Type type) {
            return _requiredMethodsCache.GetOrAdd(type, () => {
                var i = type.GetVirtualMethods().Where(each => each.GetCustomAttributes(true).Any(attr => attr.GetType().Name.Equals("RequiredAttribute", StringComparison.OrdinalIgnoreCase))).ToArray();
                return i;
            });
        }

        internal static ConstructorInfo GetDefaultConstructor(this Type t) {
            return t.GetConstructor(_emptyTypes);
        }

        internal static string ToSignatureString(this MethodInfo method) {
            return "{0} {1}({2})".format(method.ReturnType.Name, method.Name, method.GetParameters().Select( each => "{0} {1}".format( each.ParameterType.NiceName(), each.Name)).JoinWithComma());
        }

        public static string NiceName(this Type type) {
            if (!type.IsGenericType) {
                return type.Name;
            }
            var typeName = type.GetGenericTypeDefinition().Name;
            typeName = typeName.Substring(0, typeName.IndexOf('`'));
            return typeName + "<" + string.Join(",", type.GetGenericArguments().Select(NiceName).ToArray()) + ">";
        }

        public static string FullNiceName(this Type type) {
            if (!type.IsGenericType) {
                return type.FullName;
            }
            var typeName = type.GetGenericTypeDefinition().FullName;
            typeName = typeName.Substring(0, typeName.IndexOf('`'));
            return typeName + "<" + string.Join(",", type.GetGenericArguments().Select(NiceName).ToArray()) + ">";
        }


        internal static Func<string, bool> GenerateInstancesSupportsMethod(object[] actualInstance) {
            var ism = actualInstance.Select(GenerateInstanceSupportsMethod).ToArray();
            return (s) => ism.Any(each => each(s));
        }

        internal static Func<string, bool> GenerateInstanceSupportsMethod(object actualInstance) {
            // if the object implements an IsMethodImplemented Method, we'll be using that 
            // to see if the method is actually supposed to be used.
            // this enables an implementor to physically implement the function in the class
            // yet treat it as if it didn't. (see the PowerShellPackageProvider)
            var imiMethodInfo = actualInstance.GetType().GetMethod("IsMethodImplemented", new[] {
                typeof (string)
            });
            return imiMethodInfo == null ? (s) => true : actualInstance.CreateProxiedDelegate<Func<string, bool>>(imiMethodInfo);
        }

        
        public static TInterface As<TInterface>(this object instance) {
            if (typeof (TInterface).IsDelegate()) {
                // find a function in this object that matches the delegate that we are given
                // and return that instead.
                if (instance.GetType().IsDelegate()) {
                    if (typeof (TInterface).IsDelegateAssignableFromDelegate(instance.GetType())) {
                        return ((Delegate)instance).CreateProxiedDelegate<TInterface>();
                    }
                    throw new Exception("Delegate '{0}' can not be created from Delegate '{1}'.".format(typeof (TInterface).NiceName(), instance.GetType().NiceName()));
                }

                var instanceSupportsMethod = GenerateInstanceSupportsMethod(instance);
                var instanceType = instance.GetType();

                var instanceMethods = instanceType.GetPublicMethods();
                var instanceFields = instanceType.GetPublicDelegateFields();
                var instanceProperties = instanceType.GetPublicDelegateProperties();

                if (!instanceSupportsMethod(typeof (TInterface).Name)) {
                    throw new Exception("Generation of Delegate '{0}' not supported from object.".format(typeof (TInterface).NiceName()));
                }

                var method = instanceMethods.FindMethod(typeof (TInterface));
                if (method != null) {
                    return instance.CreateProxiedDelegate<TInterface>(method);
                }
                var instanceDelegate = instanceFields.FindDelegate(instance, typeof (TInterface)) ?? instanceProperties.FindDelegate(instance, typeof (TInterface));
                if (instanceDelegate != null) {
                    if (instanceDelegate is TInterface) {
                        return (TInterface)(object)instanceDelegate;
                    }
                    return instanceDelegate.CreateProxiedDelegate<TInterface>();
                }
                return (TInterface)(object) typeof(TInterface).CreateEmptyDelegate();
                // throw new Exception("Delegate '{0}' not matched in object.".format(typeof (TInterface).NiceName()));
            }
            return DynamicInterface.Instance.Create<TInterface>(instance);
        }

        public static TInterface Extend<TInterface>(this object obj, params object[] objects) {
            return DynamicInterface.Instance.Create<TInterface>(objects, obj);
        }

        public static bool IsDelegate(this Type t) {
            return t.BaseType == typeof (MulticastDelegate);
        }

        public static IEnumerable<Type> CreatableTypes(this Assembly assembly) {
#if DEEPDEBUG
            var x = _creatableTypesCache.GetOrAdd(assembly, () => assembly.GetTypes().Where(each => each.IsPublic && !each.IsEnum && !each.IsInterface && !each.IsAbstract && each.GetDefaultConstructor() != null && each.BaseType != typeof(MulticastDelegate)).ToArray());
            foreach (var i in x) {
                Debug.WriteLine("Creatable Type in assembly {0} - {1}", assembly.GetName(), i.Name);
            }
#endif 
            return _creatableTypesCache.GetOrAdd(assembly, () => assembly.GetTypes().Where(each => each.IsPublic && !each.IsEnum && !each.IsInterface && !each.IsAbstract && each.GetDefaultConstructor() != null && each.BaseType != typeof (MulticastDelegate)).ToArray());
        }

    }
}
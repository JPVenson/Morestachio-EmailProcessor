using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity;

namespace Morestachio.MailProcessor.Ui
{
	public static class IoC
	{
		static IoC()
		{
			_creationAwaiter = new ConcurrentDictionary<Type, ManualResetEventSlim>();
		}

		public static IUnityContainer Container { get; private set; }
		private static readonly ConcurrentDictionary<Type, ManualResetEventSlim> _creationAwaiter;

		public static void Init(IUnityContainer container)
		{
			if (Container != null)
			{
				return;
			}
			Container = container;
		}

		public static object RegisterInstance(object instance, Type impl = null)
		{
			if (impl == null)
			{
				impl = instance.GetType();
			}
			Container.RegisterInstance(impl, instance);

			ManualResetEventSlim waiter;
			if (_creationAwaiter.TryRemove(impl, out waiter))
			{
				waiter.Set();
			}

			return instance;
		}

		public static T RegisterInstance<T>(T instance) where T : class
		{
			return RegisterInstance(instance, typeof(T)) as T;
		}


		public static void Register<T, TE>()
		{
			Register(typeof(T), typeof(TE));
		}

		public static void Register<T>()
		{
			Register(typeof(T), typeof(T));
		}

		public static void Register(Type forType, Type impl)
		{
			Container.RegisterType(forType, impl);
			ManualResetEventSlim waiter;
			if (_creationAwaiter.TryRemove(forType, out waiter))
			{
				waiter.Set();
			}
		}

		public static void Register(Type impl)
		{
			Register(impl, impl);
		}

		public static T Resolve<T>() where T : class
		{
			if (Container.IsRegistered(typeof(T)))
			{
				return Container.Resolve(typeof(T)) as T;
			}
			return null;
		}

		//public static async Task<T> ResolveLater<T>() where T : class
		//{
		//	if (Container.IsRegistered(typeof(T)))
		//	{
		//		return Container.Resolve(typeof(T)) as T;
		//	}
		//	var manualResetEventSlim = _creationAwaiter.GetOrAdd(typeof(T), f => new ManualResetEventSlim());
		//	await manualResetEventSlim.WaitHandle.WaitOneAsync();
		//	return Resolve<T>();
		//}

		public static IEnumerable<T> ResolveMany<T>() where T : class
		{
			return Container
				.Registrations
				.Where(e => typeof(T).IsAssignableFrom(e.RegisteredType))
				.Select(e => e.LifetimeManager?.GetValue() as T)
				.Where(e => e != null);
		}
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MorestachioMailProcessor.ViewModels;
using WPFLocalizeExtension.Providers;

namespace MorestachioMailProcessor.Services.TextService
{
	   public class TextServiceUsingLocalizationProvider : ILocalizationProvider
    {
        private readonly ITextService _textService;

        public TextServiceUsingLocalizationProvider(ITextService textService)
        {
            _textService = textService;
            _textService.TranslationLoaded += _textService_TranslationLoaded;
            AvailableCultures = new ObservableCollection<CultureInfo>();

            foreach (var textServiceSupportedCulture in _textService.SupportedCultures)
            {
                AvailableCultures.Add(textServiceSupportedCulture.Value);
            }
        }

        private void _textService_TranslationLoaded(object sender, string e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                OnValueChanged(new ValueChangedEventArgs(e, _textService.Compile(e, CultureInfo.CurrentUICulture, out _), null));
            }), DispatcherPriority.DataBind);
        }

        public FullyQualifiedResourceKeyBase GetFullyQualifiedResourceKey(string key, DependencyObject target)
        {
            return new FQAssemblyDictionaryKey(key);
        }

        public object GetLocalizedObject(string key, DependencyObject target, CultureInfo culture)
        {
            var findTranslation = _textService.Compile(key, CultureInfo.CurrentUICulture, out var found);
            return findTranslation;
        }

        public ObservableCollection<CultureInfo> AvailableCultures { get; }

        /// <summary>An event that is fired when the provider changed.</summary>
        public event ProviderChangedEventHandler ProviderChanged;

        /// <summary>An event that is fired when an error occurred.</summary>
        public event ProviderErrorEventHandler ProviderError;

        /// <summary>An event that is fired when a value changed.</summary>
        public event ValueChangedEventHandler ValueChanged;

        protected virtual void OnValueChanged(ValueChangedEventArgs args)
        {
            ValueChanged?.Invoke(this, args);
        }

        protected virtual void OnProviderError(ProviderErrorEventArgs args)
        {
            ProviderError?.Invoke(this, args);
        }

        protected virtual void OnProviderChanged(ProviderChangedEventArgs args)
        {
            ProviderChanged?.Invoke(this, args);
        }
    }

	public class ResxTranslationProvider : ITranslationProvider
	{
		private readonly ResourceManager _resource;

		public ResxTranslationProvider(ResourceManager resource)
		{
			_resource = resource;
		}

		public Dictionary<string, object> ProvideTranslations(CultureInfo culture)
		{
			var resource = _resource.GetResourceSet(culture, true, true);
			if (resource == null)
			{
				var logError = $"Translations for requested {culture.Name} could not be loaded";
				throw new InvalidOperationException(logError);
			}

			var resourceFile = new Dictionary<string, object>();
			foreach (var item in resource
				.OfType<DictionaryEntry>()
				.ToDictionary(e => e.Key.ToString().ToUpper(), e => e.Value))
			{
				resourceFile.Add(item.Key, item.Value);
			}

			return resourceFile;
		}
	}

	public class TextService : ITextService
	{
		private static readonly Regex RefRegex = new Regex(@"([{][^\d}]+[}]+)");

		private readonly List<Exception> _loaderExceptions = new List<Exception>();

		public TextService()
		{
			SupportedCultures = new KeyedDictionary<string, CultureInfo>(e => e.Name);
			SupportedCultures.Add(CultureInfo.GetCultureInfo("en-us"));
			SupportedCultures.Add(CultureInfo.GetCultureInfo("de-de"));

			TranslationResources = new Collection<ITranslationProvider>();
			TranslationCacheState = new Dictionary<CultureInfo, string>();

			TextCache = new Dictionary<string, TextResourceEntity[]>();
			TextTransformations = new Dictionary<CultureInfo, IDictionary<string, Func<string, string>>>();
		}

		public IDictionary<CultureInfo, string> TranslationCacheState { get; }

		public KeyedDictionary<string, CultureInfo> SupportedCultures { get; set; }

		public ICollection<ITranslationProvider> TranslationResources { get; set; }

		public IDictionary<CultureInfo, IDictionary<string, Func<string, string>>> TextTransformations { get; set; }

		public Dictionary<string, TextResourceEntity[]> TextCache { get; set; }

		IDictionary<string, CultureInfo> ITextService.SupportedCultures
		{
			get { return SupportedCultures.ToDictionary(e => e.Name, e => e); }
		}

		public event EventHandler<string> TranslationLoaded;

		public object Compile(string key, CultureInfo culture, out bool found)
		{
			var translation = TextCache.SelectMany(e => e.Value)
				.FirstOrDefault(e => string.Equals(e.Key, key, StringComparison.OrdinalIgnoreCase) && Equals(e.Lang, culture));
			if (translation.Text == null)
			{
				found = false;
				return "[MISSING: " + key + "]";
			}
			found = true;
			return translation.Text;
		}

		public object Compile(string key, CultureInfo culture, out bool found1, params FormattableArgument[] arguments)
		{
			found1 = true;
			var formatArgs = new List<object>();
			foreach (var formattableErrorArgument in arguments)
			{
				if (formattableErrorArgument.IsKey)
				{
					formatArgs.Add(Compile(formattableErrorArgument.KeyOrValue, CultureInfo.CurrentUICulture, out found1));
				}
				else
				{
					formatArgs.Add(formattableErrorArgument.KeyOrValue);
				}

				if (!found1)
				{
					return $"[MISSING {key}: {formattableErrorArgument.KeyOrValue}]";
				}
			}

			var resultText = Compile(key, CultureInfo.CurrentUICulture, out found1);

			if (resultText is string translationText)
			{
				for (var index = 0; index < formatArgs.Count; index++)
				{
					var formatArg = formatArgs[index];
					translationText = translationText.Replace($"{{{index}}}", formatArg?.ToString());
				}

				return translationText;
			}

			return resultText;
		}

		public string RunTransformation(CultureInfo culture, string transformationKey, string text)
		{
			var transformations = TextTransformations[culture];
			var transformation =
				transformations.FirstOrDefault(e =>
					string.Equals(e.Key, transformationKey, StringComparison.OrdinalIgnoreCase));
			if (transformation.Key == null)
			{
				_loaderExceptions.Add(new InvalidOperationException(
					$"{culture.Name}:Could not find \'{transformationKey}\' transformation"));
				return "";
			}

			return transformation.Value(text);
		}

		public async Task LoadTexts()
		{
			TextCache.Clear();
			TranslationCacheState.Clear();

			var textResources = new Dictionary<string, List<TextResourceEntity>>();
			foreach (var cultureInfo in SupportedCultures)
			{
				var resources = new Dictionary<string, object>();
				var translationCache = new StringBuilder();

				foreach (var translationResource in TranslationResources)
				{
					if (translationResource is IAsyncTranslationProvider asyncTranslationProvider)
					{
						foreach (var provideTranslation in 
							await asyncTranslationProvider.ProvideTranslationsAsync(cultureInfo))
						{
							resources.Add(provideTranslation.Key, provideTranslation.Value);
						}
					}

					foreach (var provideTranslation in 
						translationResource.ProvideTranslations(cultureInfo))
					{
						resources.Add(provideTranslation.Key, provideTranslation.Value);
					}
				}

				var localResources = new List<TextResourceEntity>();
				foreach (var textResource in resources)
				{
					var key = textResource.Key;
					var keyParts = key.Split('/');
					localResources.Add(new TextResourceEntity
					{
						Key = key,
						Page = keyParts[0],
						Text = textResource.Value,
						Lang = cultureInfo
					});
				}

				var transformedResources = localResources
					.Select(localResource => new TextResourceEntity
					{
						Key = localResource.Key,
						Text = TransformText(localResource, localResources),
						Lang = localResource.Lang,
						Page = localResource.Page
					}).ToList();

				//Process References
				foreach (var group in transformedResources.GroupBy(e => e.Page))
				{
					foreach (var textResourceEntity in group)
					{
						translationCache.AppendLine(textResourceEntity.Page + "-" + textResourceEntity.Text);
					}

					List<TextResourceEntity> cache;
					if (!textResources.ContainsKey(group.Key))
					{
						textResources[group.Key] = cache = new List<TextResourceEntity>();
					}
					else
					{
						cache = textResources[group.Key];
					}

					cache.AddRange(group.ToArray());
				}

				var hash = MD5
					.Create()
					.ComputeHash(Encoding.Unicode.GetBytes(translationCache.ToString()))
					.Select(e => e.ToString("X2").ToUpper())
					.Aggregate((e, f) => e + f);
				TranslationCacheState.Add(cultureInfo, hash);
			}

			if (_loaderExceptions.Any())
			{
				throw new AggregateException(_loaderExceptions.GroupBy(e => e.Message).Select(e => e.First()).ToArray())
					.Flatten();
			}

			foreach (var textResource in textResources)
			{
				TextCache[textResource.Key] = textResource.Value.ToArray();
			}
		}

		private object TransformText(TextResourceEntity textResourceEntity,
			IEnumerable<TextResourceEntity> fromResources, Stack<string> transformationChain = null)
		{
			if (!(textResourceEntity.Text is string text))
			{
				return textResourceEntity.Text;
			}

			transformationChain = transformationChain ?? new Stack<string>();

			foreach (Match match in RefRegex.Matches(text))
			{
				var textPart = match.Value.Trim();

				if (textPart.Trim('{', '}').StartsWith("!"))
				{
					text = text.Replace("!", "");
					continue;
				}

				var transformationResource = fromResources.FirstOrDefault(e =>
					e.Lang == textResourceEntity.Lang &&
					Equals(e.Key, textPart.ToUpper().Trim('{', '}')));
				if (transformationResource.Key == null)
				{
					_loaderExceptions.Add(new InvalidOperationException(
						$"{textResourceEntity.Lang.Name}: The requested Transformation in '{textResourceEntity.Key.ToUpper()}' for '{match.Value}' could not found"));
					return "";
				}

				if (transformationChain.Contains(transformationResource.Key))
				{
					_loaderExceptions.Add(new InvalidOperationException(
						$"{textResourceEntity.Lang.Name}: Endless requsion detected for: " +
						transformationChain.Aggregate((e, f) => e + " | " + f)));
					return "";
				}

				transformationChain.Push(transformationResource.Key);

				var textReplacement = TransformText(transformationResource, fromResources, transformationChain);

				transformationChain.Pop();

				foreach (var textTransformationOperator in textPart.Split('|').Skip(1))
				{
					var transformations = TextTransformations[textResourceEntity.Lang];
					var transformation = transformations.First(e => Equals(e.Key, textTransformationOperator));
					textPart = transformation.Value(textPart);
				}

				if (textReplacement is string textReplacementStr)
				{
					text = text.Replace(textPart, textReplacementStr);
				}
			}

			return text;
		}

		public IEnumerable<TextResourceEntity> GetByGroupName(string groupName, string locale)
		{
			if (TextCache.ContainsKey(groupName))
			{
				return TextCache[groupName].Where(e =>
					string.Equals(e.Lang.Name, locale, StringComparison.InvariantCultureIgnoreCase)
					|| string.Equals(e.Lang.TwoLetterISOLanguageName, locale, StringComparison.InvariantCultureIgnoreCase)
					|| string.Equals(e.Lang.ThreeLetterWindowsLanguageName, locale,
						StringComparison.InvariantCultureIgnoreCase));
			}

			return Enumerable.Empty<TextResourceEntity>();
		}

		protected virtual void OnTranslationLoaded(string e)
		{
			TranslationLoaded?.Invoke(this, e);
		}
	}
}
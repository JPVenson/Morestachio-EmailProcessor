using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Morestachio.Newtonsoft.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Morestachio.MailProcessor.Framework.Import.Strategies
{
	public class JsonFileImportStrategy : IMailDataStrategy
	{
		public JsonFileImportStrategy(Stream sourceData)
		{
			Id = IdKey;
			SourceData = sourceData;
		}

		public const string IdKey = "JsonImport";
		public string Id { get; }
		public Stream SourceData { get; }

		public async Task<int> Count()
		{
			var elements = 0;
			SourceData.Seek(0, SeekOrigin.Begin);
			using (var sr = new StreamReader(SourceData, null, true, -1, true))
			{
				using (var reader = new JsonTextReader(sr))
				{
					while (await reader.ReadAsync())
					{
						// deserialize only when there's "{" character in the stream
						if (reader.TokenType == JsonToken.StartObject)
						{
							await reader.SkipAsync();
							elements++;
						}
					}
				}
			}

			return elements;
		}

		private class JsonAsyncEnumerable : IAsyncEnumerable<MailData>
		{
			private readonly Stream _sourceData;
			private readonly JsonLoadSettings _jsonLoadSettings;

			public JsonAsyncEnumerable(Stream sourceData, JsonLoadSettings jsonLoadSettings)
			{
				_sourceData = sourceData;
				_jsonLoadSettings = jsonLoadSettings;
			}

			public IAsyncEnumerator<MailData> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
			{
				return new JsonAsyncEnumerator(new StreamReader(_sourceData, null, true, -1, true),
					_jsonLoadSettings);
			}

			private class JsonAsyncEnumerator : IAsyncEnumerator<MailData>
			{
				private readonly StreamReader _stream;
				private readonly JsonTextReader _reader;
				private readonly JsonLoadSettings _jsonLoadSettings;

				public JsonAsyncEnumerator(StreamReader stream, JsonLoadSettings jsonLoadSettings)
				{
					_stream = stream;
					_jsonLoadSettings = jsonLoadSettings;
					_reader = new JsonTextReader(_stream);
					_reader.Read();
					if (_reader.TokenType != JsonToken.StartArray)
					{
						throw new InvalidOperationException("The start of the json file should be an array");
					}
				}

				public async ValueTask DisposeAsync()
				{
					await ValueTask.CompletedTask;
					_stream.Dispose();
				}

				public async ValueTask<bool> MoveNextAsync()
				{
					if (!(await _reader.ReadAsync()) || _reader.TokenType == JsonToken.EndArray)
					{
						return false;
					}
					
					var jObject = await JObject.LoadAsync(_reader, _jsonLoadSettings);
					Current = new MailData(JsonNetValueResolver.EvalJObject(jObject));
					await _reader.SkipAsync();
					return true;
				}

				public MailData Current { get; private set; }
			}
		}

		public async Task<IAsyncEnumerable<MailData>> GetMails()
		{
			await Task.CompletedTask;
			var settings = new JsonLoadSettings();
			SourceData.Seek(0, SeekOrigin.Begin);
			return new JsonAsyncEnumerable(SourceData, settings);
		}

		public async Task<MailData> GetPreviewData()
		{
			await foreach (var item in await GetMails())
			{
				return item;
			}

			return null;
		}

		public async ValueTask DisposeAsync()
		{
			await SourceData.DisposeAsync();
		}
	}
}

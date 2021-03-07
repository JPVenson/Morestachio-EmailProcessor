using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace Morestachio.MailProcessor.Framework.Import.Strategies
{
	public class CsvImportStrategy : IMailDataStrategy, IAsyncDisposable
	{
		private readonly Stream _csvContents;
		private readonly Configuration _configuration;

		public CsvImportStrategy(Stream csvContents, Configuration configuration)
		{
			_csvContents = csvContents;
			_configuration = configuration;
			Exclude = new List<string>();
			Id = IdKey;
		}

		public IList<string> Exclude { get; }

		public const string IdKey = "CSV_FILE_IMPORTER";
		public string Id { get; }

		private class CsvEnumerable : IAsyncEnumerable<MailData>, IDisposable
		{
			private readonly CsvReader _reader;
			private readonly string[] _headers;

			public CsvEnumerable(CsvReader reader, string[] headers)
			{
				_reader = reader;
				_headers = headers;
			}

			public IAsyncEnumerator<MailData> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
			{
				return new CsvEnumerator(_reader, _headers);
			}

			public void Dispose()
			{
				_reader?.Dispose();
			}
		}

		private class CsvEnumerator : IAsyncEnumerator<MailData>
		{
			private readonly CsvReader _reader;
			private readonly string[] _headers;

			public CsvEnumerator(CsvReader reader, string[] headers)
			{
				_reader = reader;
				_headers = headers;
			}

			public async ValueTask DisposeAsync()
			{
				_reader.Dispose();
				await Task.CompletedTask;
			}

			public async ValueTask<bool> MoveNextAsync()
			{
				if (!await _reader.ReadAsync())
				{
					return false;
				}

				var mailData = new MailData();
				foreach (var header in _headers)
				{
					mailData.Data[header] = _reader.GetField<string>(header);
				}

				Current = mailData;
				return true;
			}

			public MailData Current
			{
				get;
				private set;
			}
		}

		public async Task<int> Count()
		{
			_csvContents.Position = 0;
			var fileReader = new StreamReader(_csvContents, null, true, -1, true);
			var csvReader = new CsvReader(fileReader, _configuration, true);

			await csvReader.ReadAsync();
			csvReader.ReadHeader();

			var count = 0;
			while (await csvReader.ReadAsync())
			{
				count++;
			}

			return count;
		}

		public async Task<IAsyncEnumerable<MailData>> GetMails()
		{
			_csvContents.Position = 0;
			var fileReader = new StreamReader(_csvContents, null, true, -1, true);
			var csvReader = new CsvReader(fileReader, _configuration, true);

			await csvReader.ReadAsync();
			csvReader.ReadHeader();
			return new CsvEnumerable(csvReader, csvReader.Context.HeaderRecord.Except(Exclude).ToArray());
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
			await _csvContents.DisposeAsync();
		}
	}
}

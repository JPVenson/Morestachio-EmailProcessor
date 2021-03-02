using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;

namespace Morestachio.MailProcessor.Framework.Import.Strategies
{
	public class CsvImportStrategy : IMailDataStrategy
	{
		private readonly string _file;

		public CsvImportStrategy(string file)
		{
			_file = file;
			Exclude = new List<string>();
			Id = IdKey;
		}

		public string[] GetMetaData()
		{
			using (var reader = new StreamReader(_file))
			using (var csv = new CsvReader(reader))
			{
				csv.Read();
				csv.ReadHeader();
				return csv.Context.HeaderRecord;
			}
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
			private int _index;

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
				return await _reader.ReadAsync();
			}

			public MailData Current
			{
				get
				{
					var mailData = new MailData();
					foreach (var header in _headers)
					{
						mailData.Data[header] = _reader.GetField<string>(header);
					}

					return mailData;
				}
			}
		}

		public async Task<int> Count()
		{
			var fileReader = new StreamReader(_file);
			var csvReader = new CsvReader(fileReader);
			
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
			var fileReader = new StreamReader(_file);
			var csvReader = new CsvReader(fileReader);
			
			await csvReader.ReadAsync();
			csvReader.ReadHeader();
			return new CsvEnumerable(csvReader, csvReader.Context.HeaderRecord.Except(Exclude).ToArray());
		}

		public async Task<MailData> GetPreviewData()
		{
			var fileReader = new StreamReader(_file);
			var csvReader = new CsvReader(fileReader);
			
			await csvReader.ReadAsync();
			csvReader.ReadHeader();

			var csvEnumerable = new CsvEnumerable(csvReader, csvReader.Context.HeaderRecord.Except(Exclude).ToArray());
			await foreach (var item in csvEnumerable)
			{
				return item;
			}

			return null;
		}
	}
}

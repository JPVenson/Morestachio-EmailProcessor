using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

		public async Task<ICollection<MailData>> GetMails(int? skip, int? take)
		{
			var result = new List<MailData>();
			using (var reader = new StreamReader(_file))
			using (var csv = new CsvReader(reader))
			{
				await csv.ReadAsync();
				csv.ReadHeader();
				var headers = csv.Context.HeaderRecord.Except(Exclude);

				var indexer = 0;
				while (await csv.ReadAsync())
				{
					if (skip.HasValue && indexer < skip.Value)
					{
						indexer++;
						continue;
					}

					var mailData = new MailData();
					foreach (var header in headers)
					{
						mailData.Data[header] = csv.GetField<string>(header);
					}
					result.Add(mailData);
					indexer++;
					
					if (take.HasValue && indexer > take.Value)
					{
						break;
					}
				}
			}

			return result;
		}
	}
}

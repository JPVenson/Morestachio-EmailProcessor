using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.AdoWrapper.MsSqlProvider;
using JPB.DataAccess.Manager;

namespace Morestachio.MailProcessor.Framework.Import.Strategies
{
	public class SqlImportStrategy : IMailDataStrategy
	{
		public SqlImportStrategy(string query, string connectionString)
		{
			Query = query;
			ConnectionString = connectionString;
		}

		public string Query { get; set; }
		public string ConnectionString { get; set; }

		public const string IdKey = "SQL_IMPORTER";
		public string Id { get; } = IdKey;

		public class AsyncDbAccessLayerEnumerable : IAsyncEnumerable<MailData>
		{
			private readonly DbAccessLayer _dbAccessLayer;
			private readonly string _query;

			public AsyncDbAccessLayerEnumerable(DbAccessLayer dbAccessLayer, string query)
			{
				_dbAccessLayer = dbAccessLayer;
				_query = query;
			}

			public IAsyncEnumerator<MailData> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
			{
				_dbAccessLayer.Database.Connect();
				var command = _dbAccessLayer.Database.CreateCommand(_query);
				var reader = command.ExecuteReader();
				return new AsyncDbAccessLayerEnumerator(_dbAccessLayer, reader);
			}

			private class AsyncDbAccessLayerEnumerator : IAsyncEnumerator<MailData>
			{
				private readonly DbAccessLayer _dbAccessLayer;
				private readonly IDataReader _reader;

				public AsyncDbAccessLayerEnumerator(DbAccessLayer dbAccessLayer, IDataReader reader)
				{
					_dbAccessLayer = dbAccessLayer;
					_reader = reader;
				}

				public async ValueTask DisposeAsync()
				{
					await ValueTask.CompletedTask;
					_reader.Dispose();
					_dbAccessLayer.Database.CloseConnection();
				}

				public async ValueTask<bool> MoveNextAsync()
				{
					await ValueTask.CompletedTask;
					if (!_reader.Read())
					{
						return false;
					}

					Current = new MailData();
					for (int i = 0; i < _reader.FieldCount; i++)
					{
						Current.Data[_reader.GetName(i)] = _reader.GetValue(i);
					}

					return true;
				}

				public MailData Current { get; private set; }
			}
		}

		public async Task<IAsyncEnumerable<MailData>> GetMails()
		{
			var accessLayer = GenerateAccessLayer();
			await ValueTask.CompletedTask;
			return new AsyncDbAccessLayerEnumerable(accessLayer, Query);
		}

		private DbAccessLayer GenerateAccessLayer()
		{
			var accessLayer = new DbAccessLayer(new MsSql(ConnectionString));
			accessLayer.Async = true;
			accessLayer.LoadCompleteResultBeforeMapping = false;
			accessLayer.ThreadSave = true;
			return accessLayer;
		}

		public async Task<MailData> GetPreviewData()
		{
			var accessLayer = GenerateAccessLayer();
			var asyncDbAccessLayerEnumerable = new AsyncDbAccessLayerEnumerable(accessLayer, Query);
			try
			{
				await foreach (var item in asyncDbAccessLayerEnumerable)
				{
					return item;
				}
			}
			finally
			{
				accessLayer.Database.Dispose();
				//Server=.\V17;Database=JPB.MyWorksheet.Database;Trusted_Connection=True;
			}
			

			return null;
		}

		public async Task<int> Count()
		{
			return -1;
		}

		public async ValueTask DisposeAsync()
		{
			await Task.CompletedTask;
		}
	}
}
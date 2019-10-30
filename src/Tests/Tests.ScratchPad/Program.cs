using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Elasticsearch.Net;
using Elasticsearch.Net.Diagnostics;
using Nest;
using Tests.Core.Client;
using Tests.Domain;
using Xunit.Sdk;

namespace Tests.ScratchPad
{
	public class Program
	{
		private class ListenerObserver : IObserver<DiagnosticListener>
		{
			public void OnCompleted() => Console.WriteLine("Completed");

			public void OnError(Exception error) => Console.Error.WriteLine(error.Message);

			public void OnNext(DiagnosticListener value)
			{
				void WriteToConsole<T>(string eventName, T data)
				{
					var a = Activity.Current;
					Console.WriteLine($"{eventName?.PadRight(30)} {a.Id?.PadRight(32)} {a.ParentId?.PadRight(32)} {data?.ToString().PadRight(10)}");
				}
				if (value.Name == DiagnosticSources.AuditTrailEvents.SourceName)
					value.Subscribe(new AuditDiagnosticObserver(v => WriteToConsole(v.Key, v.Value)));

				if (value.Name == DiagnosticSources.RequestPipeline.SourceName)
					value.Subscribe(new RequestPipelineDiagnosticObserver(
						v => WriteToConsole(v.Key, v.Value),
						v => WriteToConsole(v.Key, v.Value))
					);

				if (value.Name == DiagnosticSources.HttpConnection.SourceName)
					value.Subscribe(new HttpConnectionDiagnosticObserver(
						v => WriteToConsole(v.Key, v.Value),
						v => WriteToConsole(v.Key, v.Value)
					));

				if (value.Name == DiagnosticSources.Serializer.SourceName)
					value.Subscribe(new SerializerDiagnosticObserver(v => WriteToConsole(v.Key, v.Value)));
			}
		}

		private static readonly IList<Project> Projects = Project.Generator.Clone().Generate(10000);
		private static byte[] Response = TestClient.DefaultInMemoryClient.ConnectionSettings.RequestResponseSerializer.SerializeToBytes(ReturnBulkResponse(Projects));
		private static readonly IElasticClient Client = TestClient.FixedInMemoryClient(Response);


		private static async Task Main(string[] args)
		{
			var response = Client.Bulk(b => b.IndexMany(Projects));


			await Task.Delay(TimeSpan.FromSeconds(6));
			Console.WriteLine($"Kicking off");

			for (var i = 0; i < 10_000; i++)
			{
				var r = Client.Bulk(b => b.IndexMany(Projects));
				Console.Write($"\r{i}: {r.IsValid} {r.Items.Count}");
			}
		}


		private static object BulkItemResponse(Project project) => new
		{
			index = new
			{
				_index = "nest-52cfd7aa",
				_type = "_doc",
				_id = project.Name,
				_version = 1,
				_shards = new
				{
					total = 2,
					successful = 1,
					failed = 0
				},
				created = true,
				status = 201
			}
		};

		private static object ReturnBulkResponse(IList<Project> projects) => new
		{
			took = 276,
			errors = false,
			items = projects
				.Select(p => BulkItemResponse(p))
				.ToArray()
		};

		private static void Bench<TBenchmark>() where TBenchmark : RunBase => BenchmarkRunner.Run<TBenchmark>();

		private static void Run<TRun>() where TRun : RunBase, new()
		{
			var runner = new TRun { IsNotBenchmark = true };
			runner.GlobalSetup();
			runner.Run();
		}

		private static void RunCreateOnce<TRun>() where TRun : RunBase, new()
		{
			var runner = new TRun { IsNotBenchmark = true };
			runner.GlobalSetup();
			runner.RunCreateOnce();
		}
	}
}

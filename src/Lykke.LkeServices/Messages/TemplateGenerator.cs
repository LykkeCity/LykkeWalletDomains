using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Core.Messages;
using Microsoft.Extensions.PlatformAbstractions;
using RazorLight;

namespace LkeServices.Messages
{
	public class TemplateGenerator : ITemplateGenerator
	{
		private readonly string[] _resources;

		public TemplateGenerator()
		{
			_resources = typeof(TemplateGenerator).GetTypeInfo().Assembly.GetManifestResourceNames();
		}

		public Task<string> GenerateAsync<T>(string templateName, T templateModel, TemplateType type)
		{
			string template = "." + templateName + ".cshtml";
			template = string.Join(".", _resources.FirstOrDefault(o => o.EndsWith(template)).Split('.').Skip(1));
			var engine = EngineFactory.CreateEmbedded(typeof(SrvBinder));

			try
			{
				return Task.FromResult(engine.Parse(Path.GetFileNameWithoutExtension(template), templateModel));
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Fail template \"{template}\" compilation: {ex.Message}");
				throw;
			}
		}
	}
}

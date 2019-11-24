using System.Diagnostics.CodeAnalysis;
using Markdig;
using Microsoft.AspNetCore.Html;

namespace StyleEl
{
	public class MarkdownProvider
	{
		readonly MarkdownPipeline _pipeline;

		public MarkdownProvider()
		{
			_pipeline = new MarkdownPipelineBuilder()
				.UseSoftlineBreakAsHardlineBreak()
				.UseEmphasisExtras()
				.UseAutoLinks()
				.UsePipeTables()
				.UseAbbreviations()
				.Build();
		}

		[return: NotNullIfNotNull("source")]
		public IHtmlContent? ToHtml(string? source)
		{
			if (source == null)
				return null;
			var html = new HtmlContentBuilder();
			html.AppendHtml(Markdown.ToHtml(source, _pipeline));
			return html;
		}

		[return: NotNullIfNotNull("source")]
		public string? ToPlainText(string? source)
		{
			if (source == null)
				return null;
			return Markdown.ToPlainText(source, _pipeline);
		}
	}
}
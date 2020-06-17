using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace StyleEl.Models
{
	[Serializable]
	[XmlRoot("urlset", Namespace = NS, IsNullable = false)]
	public class Sitemap : List<SitemapUrl>
	{
		const string NS = "http://www.sitemaps.org/schemas/sitemap/0.9";

		public string ToXml()
		{
			var encoding = new UTF8Encoding(false);
			using var ms = new MemoryStream();
			ToStream(ms, encoding);
			return encoding.GetString(ms.ToArray());
		}

		public void ToStream(Stream output, Encoding? encoding = null)
		{
			var serializer = new XmlSerializer(typeof(Sitemap));
			var ns = new XmlSerializerNamespaces();
			ns.Add(string.Empty, NS);

			var settings = new XmlWriterSettings { Encoding = encoding ?? new UTF8Encoding(false) };
			using var writer = XmlWriter.Create(output, settings);
			serializer.Serialize(writer, this, ns);
		}
	}

	[Serializable]
	[XmlRoot("url"), XmlType("url")]
	public class SitemapUrl
	{
		[XmlElement("loc")]
		public string Location { get; set; }

		[XmlIgnore]
		public DateTime? LastMod { get; set; }

		[XmlElement("lastmod")]
		public string? LastModString
		{
			get => LastMod?.ToString("yyyy-MM-dd");
			set => LastMod = value == null ? null : (DateTime?)DateTime.Parse(value);
		}

		[XmlElement("changefreq")]
		public ChangeFrequency? ChangeFrequency { get; set; }

		[XmlElement("priority")]
		public double? Priority { get; set; }

		public SitemapUrl(string location)
			=> Location = location;

		public SitemapUrl(IUrlHelper url, string? action, string? controller, object? values = null)
			=> Location = url.Action(action, controller, values, url.ActionContext.HttpContext.Request.Scheme);

		public SitemapUrl(IUrlHelper url, string pageName, object? values = null)
			=> Location = url.Page(pageName, null, values, url.ActionContext.HttpContext.Request.Scheme);

		public bool ShouldSerializeChangeFrequency() => ChangeFrequency.HasValue;
		public bool ShouldSerializePriority() => Priority.HasValue;
	}

	[Serializable]
	public enum ChangeFrequency
	{
		[XmlEnum(Name = "always")]
		Always,

		[XmlEnum(Name = "hourly")]
		Hourly,

		[XmlEnum(Name = "daily")]
		Daily,

		[XmlEnum(Name = "weekly")]
		Weekly,

		[XmlEnum(Name = "monthly")]
		Monthly,

		[XmlEnum(Name = "yearly")]
		Yearly,

		[XmlEnum(Name = "never")]
		Never
	}
}
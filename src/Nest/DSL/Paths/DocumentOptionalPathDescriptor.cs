﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Elasticsearch.Net;
using Nest.Resolvers;

namespace Nest
{
	public interface IDocumentOptionalPath<TParameters, T> : IRequest<TParameters>
		where TParameters : IRequestParameters, new()
		where T : class
	{
		IndexNameMarker Index { get; set; }
		TypeNameMarker Type { get; set; }
		string Id { get; set; }
		T IdFrom { get; set; }
	}

	internal static class DocumentOptionalPathRouteParamaters
	{
		public static void SetRouteParameters<TParameters, T>(
			IDocumentOptionalPath<TParameters, T> path,
			IConnectionSettingsValues settings,
			ElasticsearchPathInfo<TParameters> pathInfo)
			where TParameters : IRequestParameters, new()
			where T : class
		{
			var inferrer = new ElasticInferrer(settings);
			var index = path.Index != null ? inferrer.IndexName(path.Index) : inferrer.IndexName<T>();
			var type = path.Type != null ? inferrer.TypeName(path.Type) : inferrer.TypeName<T>();
			var id = path.Id ?? inferrer.Id(path.IdFrom);

			pathInfo.Index = index;
			pathInfo.Type = type;
			pathInfo.Id = id;
		}

	}
	public abstract class DocumentOptionalPathBase<TParameters, T> : BasePathRequest<TParameters>, IDocumentOptionalPath<TParameters, T>
		where TParameters : FluentRequestParameters<TParameters>, new()
		where T : class
	{
		public IndexNameMarker Index { get; set; }
		public TypeNameMarker Type { get; set; }
		public string Id { get; set; }
		public T IdFrom { get; set; }

		protected override void SetRouteParameters(
			IConnectionSettingsValues settings, ElasticsearchPathInfo<TParameters> pathInfo)
		{
			DocumentOptionalPathRouteParamaters.SetRouteParameters(this, settings, pathInfo);
		}
	}

	/// <summary>
	/// Provides a base for descriptors that need to describe a path in the form of 
	/// <pre>
	///	/{index}/{type}/{id}
	/// </pre>
	/// if one of the parameters is not explicitly specified this will fall back to the defaults for type 
	/// this version won't throw if any of the parts are inferred to be empty<para>T</para>
	/// </summary>
	public abstract class DocumentOptionalPathDescriptorBase<TDescriptor, T, TParameters>
		: BasePathDescriptor<TDescriptor, TParameters>, IDocumentOptionalPath<TParameters, T>
		where TDescriptor : DocumentOptionalPathDescriptorBase<TDescriptor, T, TParameters>, new()
		where T : class
		where TParameters : FluentRequestParameters<TParameters>, new()
	{

		private IDocumentOptionalPath<TParameters, T> Self { get { return this; } }

		IndexNameMarker IDocumentOptionalPath<TParameters, T>.Index { get; set; }
		TypeNameMarker IDocumentOptionalPath<TParameters, T>.Type { get; set; }
		string IDocumentOptionalPath<TParameters, T>.Id { get; set; }
		T IDocumentOptionalPath<TParameters, T>.IdFrom { get; set; }

		public TDescriptor Index(string index)
		{
			Self.Index = index;
			return (TDescriptor)this;
		}

		public TDescriptor Index(Type index)
		{
			Self.Index = index;
			return (TDescriptor)this;
		}

		public TDescriptor Index<TAlternative>() where TAlternative : class
		{
			Self.Index = typeof(TAlternative);
			return (TDescriptor)this;
		}

		public TDescriptor Type(string type)
		{
			Self.Type = type;
			return (TDescriptor)this;
		}
		public TDescriptor Type(Type type)
		{
			Self.Type = type;
			return (TDescriptor)this;
		}
		public TDescriptor Type<TAlternative>() where TAlternative : class
		{
			Self.Type = typeof(TAlternative);
			return (TDescriptor)this;
		}
		public TDescriptor Id(long id)
		{
			return this.Id(id.ToString());
		}
		public TDescriptor Id(string id)
		{
			Self.Id = id;
			return (TDescriptor)this;
		}
		public TDescriptor Object(T @object)
		{
			Self.IdFrom = @object;
			return (TDescriptor)this;
		}

		protected override void SetRouteParameters(
			IConnectionSettingsValues settings, ElasticsearchPathInfo<TParameters> pathInfo)
		{
			DocumentOptionalPathRouteParamaters.SetRouteParameters(this, settings, pathInfo);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Net;
using AxosoftAPI.NET.Helpers;
using AxosoftAPI.NET.Interfaces;
using AxosoftAPI.NET.Models;

namespace AxosoftAPI.NET.Core
{
	public delegate void TraceHandler(string verb, string resource, object contentOrId, IDictionary<string, object> parameters);

	public class BaseRequest
	{
		protected IProxy client;

		public TraceHandler TraceCallback;

		public BaseRequest(IProxy client)
		{
			this.client = client;
		}

		public Result<T> GetInvalidResponse<T>(Exception ex)
		{
			// Create default instance
			return new Result<T>
			{
				IsSuccessful = false,
				ErrorMessage = ex.Message
			};
		}

		public virtual string GetVersionedUri()
		{
			return string.Format(@"{0}/api/{1}", client.Url, client.Version.GetDescription());
		}

		public virtual string GetVersionedResourceUri(string resource)
		{
			return string.Format(@"{0}/{1}", GetVersionedUri(), resource);
		}

		public virtual R Get<R>(string resource, IDictionary<string, object> parameters = null)
		{
			var request = BuildRequest(resource, parameters);

			var result = request.Get<R, ErrorResponse>();
			InvokeTrace("get", resource, null, parameters);
			return result;
		}

		public virtual R Post<R>(string resource, object content, IDictionary<string, object> parameters = null)
		{
			var request = BuildRequest(resource, parameters);

			var result = request.Post<R, ErrorResponse>(content);
			InvokeTrace("post", resource, content, parameters);
			return result;
		}

		public virtual object Delete(string resource, int id, IDictionary<string, object> parameters = null)
		{
			var request = BuildRequest(string.Format("{0}/{1}", resource, id), parameters);

			var result = request.Delete<object, ErrorResponse>();
			InvokeTrace("delete", resource, id, parameters);
			return result;
		}

		public virtual object Delete(string resource, IDictionary<string, object> parameters = null)
		{
			var request = BuildRequest(string.Format("{0}", resource), parameters);

			var result = request.Delete<object, ErrorResponse>();
			InvokeTrace("delete", resource, null, parameters);
			return result;
		}

		public virtual HttpWebRequest BuildRequest(string resource, IDictionary<string, object> parameters = null)
		{
			// Build URI
			var uri = new UriBuilder(GetVersionedResourceUri(resource));

			// Add all parameters to URI
			uri.AddParameters(parameters);

			// Create new http web request
			var request = WebRequest.CreateHttp(uri.ToString());

			// Add OAuth header (X-Authorization)
			if (!string.IsNullOrWhiteSpace(client.AccessToken))
			{
				request.Headers.Add("X-Authorization", string.Format(@"OAuth {0}", client.AccessToken));
			}

			// Return http request 
			return request;
		}

		private void InvokeTrace(string verb, string resource, object contentOrId, IDictionary<string, object> parameters)
		{
			TraceCallback?.Invoke(verb, resource, contentOrId, parameters);
		}
	}
}

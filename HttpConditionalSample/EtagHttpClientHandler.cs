using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace HttpConditionalSample
{
	public class EtagHttpClientHandler : HttpClientHandler
	{
		private readonly Dictionary<string, string> _etagDictionary = new Dictionary<string, string>();

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			// See if we have a ETag for this endpoint and add it to the request
			_etagDictionary.TryGetValue(request.RequestUri.ToString(), out var etag);

			if (!string.IsNullOrWhiteSpace(etag))
			{
				request.Headers.IfNoneMatch.Clear();
				request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(etag));
			}

			// Everything before the base call is the request
			var responseMessage = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

			// Here we can handle the response
			if (responseMessage.IsSuccessStatusCode)
			{
				// If a ETag is provided with the response, cache it for future requests
				if (!string.IsNullOrWhiteSpace(responseMessage.Headers.ETag?.Tag))
					_etagDictionary.Add(responseMessage.RequestMessage.RequestUri.ToString(), responseMessage.Headers.ETag?.Tag);
			}

			return responseMessage;
		}
	}
}
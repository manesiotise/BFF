﻿using Duende.Bff.Tests.TestFramework;
using FluentAssertions;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Duende.Bff.Tests.Endpoints
{
    public class LocalEndpointTests : BffIntegrationTestBase
    {
        [Fact]
        public async Task calls_to_remote_endpoint_should_forward_to_api()
        {
            await _bffHost.BffLoginAsync("alice");

            var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/api/test"));
            req.Headers.Add("x-csrf", "1");
            var response = await _bffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.Should().BeTrue();
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
            apiResult.method.Should().Be("GET");
            apiResult.path.Should().Be("/test");
            apiResult.sub.Should().Be("alice");
        }

        [Fact]
        public async Task put_to_remote_endpoint_should_forward_to_api()
        {
            await _bffHost.BffLoginAsync("alice");

            var req = new HttpRequestMessage(HttpMethod.Put, _bffHost.Url("/api/test"));
            req.Headers.Add("x-csrf", "1");
            req.Content = new StringContent(JsonSerializer.Serialize(new TestPayload("hello test api")), Encoding.UTF8, "application/json");
            var response = await _bffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.Should().BeTrue();
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
            apiResult.method.Should().Be("PUT");
            apiResult.path.Should().Be("/test");
            apiResult.sub.Should().Be("alice");
            var body = JsonSerializer.Deserialize<TestPayload>(apiResult.body);
            body.message.Should().Be("hello test api");
        }
    }
}
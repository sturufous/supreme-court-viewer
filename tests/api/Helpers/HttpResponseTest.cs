using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace tests.api.Helpers
{
    public class HttpResponseTest
    {
        public static T CheckForValidHttpResponseAndReturnValue<T>(ActionResult<T> actionResult)
        {
            Assert.NotNull(actionResult);
            Assert.NotNull(actionResult.Result);
            var okObjectResult = actionResult.Result as OkObjectResult;
            Assert.NotNull(okObjectResult);
            var result = (T)okObjectResult.Value;
            Assert.NotNull(result);
            return result;
        }

        public static ControllerContext SetupMockControllerContext()
        {
            var headerDictionary = new HeaderDictionary();
            var response = new Mock<HttpResponse>();
            response.SetupGet(r => r.Headers).Returns(headerDictionary);

            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(a => a.Response).Returns(response.Object);

            return new ControllerContext()
            {
                HttpContext = httpContext.Object
            };
        }
    }
}
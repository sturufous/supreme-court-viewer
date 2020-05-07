using Microsoft.AspNetCore.Mvc;
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
    }
}

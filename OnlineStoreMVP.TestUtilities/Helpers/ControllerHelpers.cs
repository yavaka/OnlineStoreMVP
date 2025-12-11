using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace OnlineStoreMVP.TestUtilities.Helpers
{
    public static class ControllerHelpers
    {
        /// <summary>
        /// Sets up a mock HttpContext with a TraceIdentifier for the controller.
        /// </summary>
        public static void SetupHttpContext(ControllerBase controller)
        {
            var services = new ServiceCollection()
            .AddSingleton<IWebHostEnvironment>(Mock.Of<IWebHostEnvironment>())
            .BuildServiceProvider();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    TraceIdentifier = Guid.NewGuid().ToString(),
                    RequestServices = services
                }
            };
        }
    }
}

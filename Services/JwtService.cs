using System.Linq;

using System.Security.Claims;

using Microsoft.AspNetCore.Http;


namespace QAssessment_project.Services

{

    public class JwtService

    {

        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtService(IHttpContextAccessor httpContextAccessor)

        {

            _httpContextAccessor = httpContextAccessor;

        }

        public int GetEmployeeIdFromToken()

        {

            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null || httpContext.User == null)

            {

                throw new UnauthorizedAccessException("No active HTTP context found.");

            }

            var user = httpContext.User;

            if (!user.Identity.IsAuthenticated)

            {

                throw new UnauthorizedAccessException("User is not authenticated.");

            }

            var employeeIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (employeeIdClaim == null)

            {

                throw new UnauthorizedAccessException("Employee ID not found in token.");

            }

            return int.Parse(employeeIdClaim.Value);

        }

    }

}



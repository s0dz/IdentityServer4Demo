using System;
using System.Linq;
using System.Threading.Tasks;
using ImageGallery.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ImageGallery.API.Authorization
{
    public class MustOwnImageHandler : AuthorizationHandler<MustOwnImageRequirement>
    {
        private readonly IGalleryRepository _galleryRepository;

        public MustOwnImageHandler(IGalleryRepository galleryRepository)
        {
            _galleryRepository = galleryRepository;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MustOwnImageRequirement requirement)
        {
            if (!(context.Resource is AuthorizationFilterContext filterContext))
            {
                context.Fail();
                return Task.FromResult(0);
            }

            var imageId = filterContext.RouteData.Values["id"].ToString();

            if (!Guid.TryParse(imageId, out var imageGuid))
            {
                context.Fail();
                return Task.FromResult(0);
            }

            var ownerId = context.User.Claims.FirstOrDefault(c => c.Type == "sub").Value;

            if (!_galleryRepository.IsImageOwner(imageGuid, ownerId))
            {
                context.Fail();
                return Task.FromResult(0);
            }

            context.Succeed(requirement);
            return Task.FromResult(0);
        }
    }
}

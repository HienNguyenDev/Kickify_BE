using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.Features.VenuePhotos.Commands.DeleteVenuePhoto;
using Kickify.Application.Features.VenuePhotos.Commands.UpdateVenuePhoto;
using Kickify.Application.Features.VenuePhotos.Commands.UploadVenuePhotos;
using Kickify.Application.Features.VenuePhotos.Queries.GetVenuePhotoById;
using Kickify.Application.Features.VenuePhotos.Queries.GetVenuePhotos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers
{
    [ApiController]
    [Route("api/venue-photos")]
    public class VenuePhotoController : ControllerBase
    {
        private readonly ISender _sender;

        public VenuePhotoController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Get all photos of a venue
        /// </summary>
        [HttpGet("venue/{venueId:guid}")]
        public async Task<IResult> GetVenuePhotos(
            Guid venueId,
            CancellationToken cancellationToken)
        {
            var query = new GetVenuePhotosQuery(venueId);
            var result = await _sender.Send(query, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Get a specific photo by ID
        /// </summary>
        [HttpGet("{photoId:guid}")]
        public async Task<IResult> GetVenuePhotoById(
            Guid photoId,
            CancellationToken cancellationToken)
        {
            var query = new GetVenuePhotoByIdQuery(photoId);
            var result = await _sender.Send(query, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Upload photos for a venue
        /// </summary>
        [Authorize]
        [HttpPost("venue/{venueId:guid}")]
        [Consumes("multipart/form-data")]
        public async Task<IResult> UploadVenuePhotos(
            Guid venueId,
            [FromForm] List<IFormFile> photos,
            CancellationToken cancellationToken)
        {
            var files = new List<FileUploadRequest>();
            foreach (var photo in photos)
            {
                files.Add(new FileUploadRequest(
                    photo.OpenReadStream(),
                    photo.FileName,
                    photo.ContentType,
                    photo.Length));
            }

            var command = new UploadVenuePhotosCommand(venueId, files);
            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Update a venue photo (display order)
        /// </summary>
        [Authorize]
        [HttpPut("{photoId:guid}")]
        public async Task<IResult> UpdateVenuePhoto(
            Guid photoId,
            [FromBody] UpdateVenuePhotoRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateVenuePhotoCommand(photoId, request.DisplayOrder);
            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Delete a venue photo
        /// </summary>
        [Authorize]
        [HttpDelete("{photoId:guid}")]
        public async Task<IResult> DeleteVenuePhoto(
            Guid photoId,
            CancellationToken cancellationToken)
        {
            var command = new DeleteVenuePhotoCommand(photoId);
            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }
    }

}

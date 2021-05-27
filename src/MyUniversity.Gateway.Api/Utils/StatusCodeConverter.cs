using Grpc.Core;

namespace MyUniversity.Gateway.Api.Utils
{
    public static class StatusCodeConverter
    {
        public static int FromGrpcToHttp(StatusCode grpsStatusCode)
        {
            return grpsStatusCode switch
            {
                StatusCode.OK => 200,
                StatusCode.InvalidArgument => 400,
                StatusCode.AlreadyExists => 400,
                StatusCode.PermissionDenied => 403,
                StatusCode.NotFound => 404,
                _ => 500
            };
        }
    }
}

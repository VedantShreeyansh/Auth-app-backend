
namespace auth_app_backend.Controllers
{
    [Serializable]
    internal class CouchDbConflictException : Exception
    {
        public CouchDbConflictException()
        {
        }

        public CouchDbConflictException(string? message) : base(message)
        {
        }

        public CouchDbConflictException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
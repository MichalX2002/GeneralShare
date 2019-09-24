using System;

namespace GeneralShare
{
    public class ResourceNotFoundException : Exception
    {
        public string ResourceName { get; }

        public ResourceNotFoundException()
        {
        }

        public ResourceNotFoundException(string message) : base(message)
        {
        }

        public ResourceNotFoundException(string message, string resourceName) : this(message)
        {
            ResourceName = resourceName;
        }

        public ResourceNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        public ResourceNotFoundException(
            string message, string resourceName, Exception inner) : this(message, inner)
        {
            ResourceName = resourceName;
        }
    }
}

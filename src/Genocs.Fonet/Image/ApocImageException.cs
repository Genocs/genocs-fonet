namespace Fonet.Image
{
    using System;

    internal class FonetImageException : Exception
    {
        public FonetImageException()
        {
        }

        public FonetImageException(string message) : base(message)
        {
        }

        public FonetImageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
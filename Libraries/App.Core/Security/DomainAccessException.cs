using System;

namespace App.Core.Security
{
    public class DomainAccessException : Exception
    {
        public DomainAccessException(string message) : base(message) { }
    }
}
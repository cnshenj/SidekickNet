// <copyright file="UnprocessableEntityException.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;

namespace SidekickNet.Utilities
{
    /// <summary>
    /// The exception that is thrown when a request contains instructions that cannot be processed,
    /// similar to HTTP 422 Unprocessable Entity.
    /// </summary>
    public class UnprocessableEntityException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnprocessableEntityException"/> class.
        /// </summary>
        public UnprocessableEntityException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnprocessableEntityException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public UnprocessableEntityException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnprocessableEntityException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public UnprocessableEntityException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

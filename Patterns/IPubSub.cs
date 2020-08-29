// <copyright file="IPubSub.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

namespace SidekickNet.Patterns
{
    /// <summary>
    /// Represents a publisher/subscriber pattern.
    /// </summary>
    public interface IPubSub
    {
        /// <summary>
        /// Sends a message to a channel.
        /// </summary>
        /// <param name="channelName">The name of the channel to publish to.</param>
        /// <param name="message">The message to publish.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SendAsync(string channelName, string message);

        /// <summary>
        /// Subscribes to a channel.
        /// </summary>
        /// <param name="channelName">The name of the channel to subscribe to.</param>
        /// <param name="subscriptionName">The name of the subscription.</param>
        /// <param name="handler">The message handler.</param>
        /// <param name="createChannel">Whether to create the channel if it doesn't exist.</param>
        /// <param name="createSubscription">Whether to create the subscription if it doesn't exist.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SubscribeAsync(
            string channelName,
            string subscriptionName,
            Func<string, Task> handler,
            bool createChannel = false,
            bool createSubscription = false);

        /// <summary>
        /// Unsubscribes from a channel.
        /// </summary>
        /// <param name="channelName">The name of the channel to unsubscribe from.</param>
        /// <param name="subscriptionName">The name of the subscription.</param>
        /// <param name="delete">Whether to delete the subscription.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UnsubscribeAsync(string channelName, string subscriptionName, bool delete = false);
    }
}

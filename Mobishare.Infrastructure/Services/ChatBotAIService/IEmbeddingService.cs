using System;

namespace Mobishare.Infrastructure.Services.ChatBotAIService;

public interface IEmbeddingService
{
    /// <summary>
    /// Creates an embedding for the given input string.
    /// <para>This method is asynchronous and returns a task that resolves to an array of floats representing the embedding.</para>
    /// <para>The embedding is a numerical representation of the input string, which can be used for various natural language processing tasks.</para>
    /// <para>The input string should be a valid text that the embedding model can process.</para>
    /// <para>The returned float array will typically have a fixed size, depending on the embedding model used.</para>
    /// <para>The size of the array corresponds to the dimensionality of the embedding space.</para>
    /// <para>The method may throw exceptions if the input is invalid or if there are issues with the embedding model.</para>
    /// </summary>
    /// <param name="input">String input for which to create an embedding.</param>
    /// <returns>A task that resolves to an array of floats representing the embedding vector.</returns>
    Task<float[]> CreateEmbeddingAsync(string input);
}

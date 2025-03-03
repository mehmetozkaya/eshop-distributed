using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace Catalog.Services;

public class ProductAIService(
    ProductDbContext dbContext,
    IChatClient chatClient,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    IVectorStoreRecordCollection<int, ProductVector> productVectorCollection)
{
    public async Task<string> SupportAsync(string query)
    {
        var systemPrompt = """
        You are a useful assistant. 
        You always reply with a short and funny message. 
        If you do not know an answer, you say 'I don't know that.' 
        You only answer questions related to outdoor camping products. 
        For any other type of questions, explain to the user that you only answer outdoor camping products questions.
        At the end, Offer one of our products: Hiking Poles-$24, Outdoor Rain Jacket-$12, Outdoor Backpack-$32, Camping Tent-$22
        Do not store memory of the chat conversation.
        """;

        var chatHistory = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, systemPrompt),
            new ChatMessage(ChatRole.User, query)
        };

        var resultPrompt = await chatClient.GetResponseAsync(chatHistory);
        return resultPrompt.Message.Contents[0].ToString()!;        
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string query)
    {
        if (!await productVectorCollection.CollectionExistsAsync())
        {
            await InitEmbeddingsAsync();
        }

        var queryEmbedding = await embeddingGenerator.GenerateEmbeddingVectorAsync(query);

        var vectorSearchOptions = new VectorSearchOptions
        {
            Top = 1,
            VectorPropertyName = "Vector"
        };

        var results =
            await productVectorCollection.VectorizedSearchAsync(queryEmbedding, vectorSearchOptions);

        List<Product> products = [];
        await foreach (var resultItem in results.Results)
        {
            products.Add(new Product
            {
                Id = resultItem.Record.Id,
                Name = resultItem.Record.Name,
                Description = resultItem.Record.Description,
                Price = resultItem.Record.Price,
                ImageUrl = resultItem.Record.ImageUrl
            });
        }        

        return products;
    }

    private async Task InitEmbeddingsAsync()
    {
        await productVectorCollection.CreateCollectionIfNotExistsAsync();

        var products = await dbContext.Products.ToListAsync();
        foreach (var product in products)
        {
            var productInfo = $"[{product.Name}] is a product that costs [{product.Price}] and is described as [{product.Description}]";

            var productVector = new ProductVector
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                Vector = await embeddingGenerator.GenerateEmbeddingVectorAsync(productInfo)
            };

            await productVectorCollection.UpsertAsync(productVector);
        }
    }
}

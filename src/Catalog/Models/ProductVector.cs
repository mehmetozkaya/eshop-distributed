using Microsoft.Extensions.VectorData;
using System.ComponentModel.DataAnnotations.Schema;

namespace Catalog.Models;

public class ProductVector
{
    [VectorStoreRecordKey]
    public int Id { get; set; }
    [VectorStoreRecordData]
    public string Name { get; set; } = default!;
    [VectorStoreRecordData]
    public string Description { get; set; } = default!;
    [VectorStoreRecordData]
    public decimal Price { get; set; }
    [VectorStoreRecordData]
    public string ImageUrl { get; set; } = default!;

    [NotMapped]
    [VectorStoreRecordVector(384, DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; set; }
}
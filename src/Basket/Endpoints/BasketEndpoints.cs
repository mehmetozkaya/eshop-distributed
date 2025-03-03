namespace Basket.Endpoints;

public static class BasketEndpoints
{
    public static void MapBasketEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("basket");

        // GET by userName
        group.MapGet("/{userName}", async (string userName, BasketService service) =>
        {
            var shoppingCart = await service.GetBasket(userName);

            if (shoppingCart is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(shoppingCart);
        })
        .WithName("GetBasket")
        .Produces<ShoppingCart>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        // POST (Upsert)
        group.MapPost("/", async (ShoppingCart shoppingCart, BasketService service) =>
        {
            await service.UpdateBasket(shoppingCart);
            return Results.Created("GetBasket", shoppingCart);
        })
        .WithName("UpdateBasket")
        .Produces<ShoppingCart>(StatusCodes.Status201Created)
        .RequireAuthorization();

        // DELETE
        group.MapDelete("/{userName}", async (string userName, BasketService service) =>
        {
            await service.DeleteBasket(userName);
            return Results.NoContent();
        })
        .WithName("DeleteBasket")
        .Produces(StatusCodes.Status204NoContent)
        .RequireAuthorization();
    }
}

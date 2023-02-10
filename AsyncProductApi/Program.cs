using AsyncProductApi.Data;
using AsyncProductApi.Dtos;
using AsyncProductApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite("Data Source=RequestDb.db"));

var app = builder.Build();

app.UseHttpsRedirection();

// Get All Listing Requests
app.MapGet("api/v1/productstatus", async (AppDbContext context) =>
{
    var listingRequests = await context.ListingRequests.ToListAsync();
    return Results.Ok(listingRequests);
});

// Delete All Listing Requests
app.MapDelete("api/v1/productstatus", async (AppDbContext context) =>
{
    await context.ListingRequests.ExecuteDeleteAsync();
    return Results.Ok();
});

// Change Status Endpoint
app.MapPut("api/v1/productstatus/{requestId}", async (AppDbContext context, string requestId) =>
{
    var listingRequest = context.ListingRequests.FirstOrDefault(lr => lr.RequestId == requestId);
    if (listingRequest == null)
    {
        return Results.NotFound();
    }

    listingRequest.RequestStatus = RequestStatus.Completed;
    context.ListingRequests.Update(listingRequest);
    await context.SaveChangesAsync();
    return Results.Ok(listingRequest);
});

// Start Endpoint
app.MapPost("api/v1/products", async (AppDbContext context, ListingRequest listingRequest) => 
{
    if (listingRequest == null)
    {
        return Results.BadRequest();
    }

    listingRequest.RequestStatus = RequestStatus.Accepted;
    listingRequest.EstimatedCompletionTime = DateTime.UtcNow.AddSeconds(20);

    await context.ListingRequests.AddAsync(listingRequest);
    await context.SaveChangesAsync();

    return Results.Accepted($"api/v1/productstatus/{listingRequest.RequestId}", listingRequest);
});

// Status Endpoint
app.MapGet("api/v1/productstatus/{requestId}", (AppDbContext context, string requestId) =>
{
    var listingRequest = context.ListingRequests.FirstOrDefault(lr => lr.RequestId == requestId);
    if (listingRequest == null)
    {
        return Results.NotFound();
    }

    var listingStatus = new ListingStatus
    {
        RequestStatus = listingRequest.RequestStatus,
        ResourceUrl = string.Empty,
    };

    if (DateTime.UtcNow > listingRequest.EstimatedCompletionTime)
    {
        listingStatus.RequestStatus = RequestStatus.Completed;
        listingStatus.ResourceUrl = $"api/v1/products/{Guid.NewGuid()}";
        return Results.Redirect($"https://localhost:7241/" + listingStatus.ResourceUrl);
    }

    listingStatus.EstimatedCompletionTime = listingRequest.EstimatedCompletionTime;
    return Results.Ok(listingStatus);
});

app.MapGet("api/v1/products/{requestId}", (string requestId) =>
{
    var response = "This is where you would pass back the final result.";
    return Results.Ok(response);
});

app.Run();
var builder=WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
var app=builder.Build();
app.MapHealthChecks("/health");
app.MapGet("/",()=>Results.Ok(new { Product="EDAN Tensor", Service="ApiHost", Status="Foundation" }));
app.Run();
public partial class Program;

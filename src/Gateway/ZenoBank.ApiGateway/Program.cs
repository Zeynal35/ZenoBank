using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicy = "Frontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3001",
                "https://localhost:3001",
                "http://localhost:5173",
                "https://localhost:5173",
                "http://127.0.0.1:3001",
                "https://127.0.0.1:3001",
                "http://127.0.0.1:5173",
                "https://127.0.0.1:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
        // ✅ AllowCredentials() silindi — axios withCredentials:false olduğu üçün
        // AllowCredentials + withCredentials:false birlikdə CORS xətasına səbəb olur
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// ✅ HTTPS redirect silindi — development-də SSL sertifikat xətasının qarşısını alır
// app.UseHttpsRedirection();

app.UseCors(CorsPolicy);

app.MapGet("/", () => Results.Ok(new
{
    Service = "ZenoBank API Gateway",
    Status = "Running",
    TimestampUtc = DateTime.UtcNow
}));

app.MapReverseProxy();

app.Run();
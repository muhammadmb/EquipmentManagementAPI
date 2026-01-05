using API.GraphQL.RentalContract.Mutations;
using API.GraphQL.RentalContract.Queries;
using API.GraphQL.RentalContract.Types;
using Application.Interface;
using Application.Interface.Repositories;
using Application.Interface.Services;
using Infrastructure.BackgroundJobs;
using Infrastructure.Contexts;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        configuration.GetConnectionString("ApplicationContext"),
        b => b.MigrationsAssembly("Infrastructure"));
});

builder.Services.AddGraphQLServer()
        .AddQueryType(d => d.Name("Query"))
        .AddMutationType(d => d.Name("Mutation"))
        .AddTypeExtension<RentalContractQueries>()
        .AddTypeExtension<RentalContractAnalyticsQueries>()
        .AddTypeExtension<RentalContractChecks>()
        .AddTypeExtension<RentalContractMutations>()
        .AddType<RentalContractType>();

builder.Services.AddTransient<IPropertyCheckerService, PropertyCheckerService>();

builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IRentalContractRepository, RentalContractRepository>();
builder.Services.AddScoped<ISellingContractRepository, SellingContractRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRentalContractService, RentalContractService>();
builder.Services.AddScoped<IRentalContractAnalyticsService, RentalContractAnalyticsService>();
builder.Services.AddScoped<ICacheVersionProvider, CacheVersionProvider>();

builder.Services.AddHostedService<ContractExpirationWorker>();

builder.Services.AddMemoryCache();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration =
        builder.Configuration.GetSection("Redis")["ConnectionString"];
    options.InstanceName = "RentalApp:";
});

builder.Services.AddControllers()
    .AddNewtonsoftJson(
        x =>
        {
            x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            x.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            x.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        }
    );
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();
app.MapGraphQL("/graphql");

app.Run();

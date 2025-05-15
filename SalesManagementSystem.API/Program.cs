using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Murur.Core.Domain.Identity;
using SalesManagementSystem.API.SeriLog;
using SalesManagementSystem.API.Serivces;
using SalesManagementSystem.Core;
using SalesManagementSystem.Core.Settings;
using SalesManagementSystem.EF;
using SalesManagementSystem.EF.DataContext;
using Serilog;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//SeriLog
builder.Host.UseSerilog((context, loggerConfig) =>
                loggerConfig.ReadFrom.Configuration(context.Configuration)
                );

//Controllers 
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

//============================ Add DbContext and SQl client also Add Db interfaces to use Dapper =================
builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        sqlOptions => sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
    )
);
//=================================================================================================================


//================================== Registration Of Services ===================================================

builder.Services.Scan(s =>
s.FromAssemblies([typeof(AssmblyRefrence).Assembly, typeof(DAAssemblyReference).Assembly])
     .AddClasses(c => c.AssignableTo<IScopedInterface>())
     .AsImplementedInterfaces()
     .WithScopedLifetime()
);

//================================== Adding Identity ===================================================

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

//===================================================================================================================

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
#region Swagger 
builder.Services.AddEndpointsApiExplorer();

// Define the names of the programmers
var programmerNames = new List<string> { "Prog 1", "Prog2", "Prog3" };

//Swagger
builder.Services.AddSwaggerGen(c =>
{
    var programmers = string.Join("\n", programmerNames.Select(name => $"- {name}"));
    var provider = builder.Services.BuildServiceProvider().GetRequiredService<IApiDescriptionGroupCollectionProvider>();
    var endpointCount = provider.ApiDescriptionGroups.Items.SelectMany(group => group.Items).Count();
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TransoBus APi Documenation",
        Description = $"Here Are {endpointCount} Endpoints\nBy Warriors:\n{programmers}",
        Version = "v1 Last Update 2024 -10-14 02:16 PM "

    });

    // Optional: Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    //c.IncludeXmlComments(xmlPath);

    // Add security definition for Bearer token
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Add security requirement for Bearer token
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
#endregion


#region  Authencation && Authrization ===================================================
builder.Services.Configure<JWT_Options>(builder.Configuration.GetSection("JWT"));
var JwtOptions = builder.Configuration.GetSection("JWT").Get<JWT_Options>();

builder.Services.AddSingleton(JwtOptions);

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.SaveToken = true;
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = JwtOptions.Issuer,
        ValidateAudience = true,
        ValidAudience = JwtOptions.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtOptions.SecretKey)),
        ClockSkew = TimeSpan.Zero

    };

}

);
builder.Services.AddAuthorization();

// ------ Configure Passowrd ------------
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 0; // Allow repeated characters
    options.Password.RequireUppercase = false;

});

#endregion ===============================================================================================================
var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await _context.Database.MigrateAsync(); // Applies any pending migrations

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await DataSeeder.SeedRoles(roleManager);
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });
}

app.UseHttpsRedirection();


// Global Exception Handler  
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {


        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Instance = context.Request.Path,
            Extensions = { ["correlationId"] = context.TraceIdentifier }
        });
    });
});


app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();


app.UseAuthorization();

app.MapControllers();

app.Run();

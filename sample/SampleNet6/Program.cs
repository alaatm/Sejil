using Sejil;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Configure Sejil
builder.Host.UseSejil(
    minLogLevel: LogLevel.Information,
    writeToProviders: true);

builder.Services.ConfigureSejil(cfg => cfg.Title = "My App Logs");
//////////////////

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

// Add sejil to request pipeline
app.UseSejil();
//////////////////

app.Run();

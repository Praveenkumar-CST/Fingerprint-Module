using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Frontend;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register a single HttpClient with the backend base address
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5234/") });

await builder.Build().RunAsync();
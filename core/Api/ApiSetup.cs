namespace DividendsHelper.Core; 

public class ApiSetup {
    public void ConfigureServices(IServiceCollection services) {
        services.AddControllers();
    }
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
        app.UseRouting();
        app.UseEndpoints(endpoints => {
            endpoints.MapControllers();
        });
    }
}
namespace RemoteSolver.Api.Main;

public static class ConfigureApp
{
    public static void Setup(WebApplication app, bool isDevelopment)
    {
        if (isDevelopment)
            app.UseDeveloperExceptionPage();

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        app.UseHttpsRedirection();
        app.UseRouting();
    }
}

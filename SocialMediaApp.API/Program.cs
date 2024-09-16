using SocialMediaApp.API.StartupExtensions;
using SocialMediaApp.Core.Hubs;

namespace SocialMediaApp.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.ServiceConfiguration(builder.Configuration);

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors();
            app.UseStaticFiles();
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<CommentHub>("/commentHub"); 
                endpoints.MapHub<TotalFollowingUnfollowingHub>("/TotalFollowingUnfollowingHub");
            });
            app.Run();
        }
    }
}

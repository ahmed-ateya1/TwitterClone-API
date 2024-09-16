
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

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors();
            app.UseStaticFiles();
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<CommentHub>("/commentHub");
            });
            app.Run();
        }
    }
}

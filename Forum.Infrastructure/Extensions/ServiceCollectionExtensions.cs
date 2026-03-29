using ForumBL.Core.Abstractions.Auth;
using ForumBL.Core.Abstractions.Events;
using ForumBL.Core.Abstractions.Repositories;
using ForumBL.Core.Services;
using Forum.Data;
using Forum.Infrastructure.Auth;
using Forum.Infrastructure.Eventing;
using Forum.Infrastructure.Handlers;
using Forum.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Forum.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddForumCore(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<ICommentService, CommentService>();

        return services;
    }

    public static IServiceCollection AddForumInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IPostMemberRepository, PostMemberRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<ICommentReactionRepository, CommentReactionRepository>();
        services.AddScoped<IPostEventRepository, PostEventRepository>();
        services.AddScoped<IForumUnitOfWork, ForumUnitOfWork>();

        services.AddSingleton<IEventDispatcher, InMemoryEventDispatcher>();
        services.AddScoped<PostEventHandler>();
        services.AddScoped<NotificationHandler>();

        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }

    public static void UseForumEventSubscriptions(this IServiceProvider serviceProvider)
    {
        var dispatcher = serviceProvider.GetRequiredService<IEventDispatcher>();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        foreach (var eventType in new[]
                 {
                     "PostCreated",
                     "UserInvited",
                     "PostClosed",
                     "CommentAdded",
                     "UserAcceptedInvite",
                     "UserRejectedInvite",
                     "CommentReplied",
                     "CommentReacted",
                     "CommentDeleted",
                     "UserRemoved"
                 })
        {
            dispatcher.Subscribe(eventType, async (domainEvent, cancellationToken) =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<PostEventHandler>();
                await handler.HandleAsync(domainEvent, cancellationToken);
            });

            dispatcher.Subscribe(eventType, async (domainEvent, cancellationToken) =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<NotificationHandler>();
                await handler.HandleAsync(domainEvent, cancellationToken);
            });
        }
    }
}

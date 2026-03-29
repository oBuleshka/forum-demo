using Forum.Domain.Enums;

namespace Forum.Domain.Entities;

public class PostMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public PostRole Role { get; set; }
    public MembershipStatus Status { get; set; }
    public DateTime? JoinedAt { get; set; }

    public Post? Post { get; set; }
    public User? User { get; set; }
}

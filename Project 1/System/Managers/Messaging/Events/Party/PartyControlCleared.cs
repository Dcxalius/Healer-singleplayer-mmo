using System;

namespace Project_1.Messaging.Events
{
    internal readonly struct PartyControlCleared
    {
        public const int MaxMembers = 4;

        public PartyControlCleared(int memberCount, int memberRenderId0, int memberRenderId1, int memberRenderId2, int memberRenderId3)
        {
            if (memberCount < 0 || memberCount > MaxMembers)
            {
                throw new ArgumentOutOfRangeException(nameof(memberCount), $"PartyControlCleared supports 0..{MaxMembers} members.");
            }

            MemberCount = memberCount;
            MemberRenderId0 = memberRenderId0;
            MemberRenderId1 = memberRenderId1;
            MemberRenderId2 = memberRenderId2;
            MemberRenderId3 = memberRenderId3;
        }

        public int MemberCount { get; }
        public int MemberRenderId0 { get; }
        public int MemberRenderId1 { get; }
        public int MemberRenderId2 { get; }
        public int MemberRenderId3 { get; }

        public int GetMemberRenderId(int index)
        {
            return index switch
            {
                0 => MemberRenderId0,
                1 => MemberRenderId1,
                2 => MemberRenderId2,
                3 => MemberRenderId3,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        }
    }
}

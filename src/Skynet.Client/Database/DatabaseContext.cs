using Microsoft.EntityFrameworkCore;
using Skynet.Client.Database.Entities;
using Skynet.Client.Database.Entities.Messages;
using System;
using System.Collections.Generic;

namespace Skynet.Client.Database
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<ChannelMember> ChannelMembers { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageDependency> MessageDependencies { get; set; }
        public DbSet<PasswordUpdate> PasswordUpdates { get; set; }
        public DbSet<DirectChannelCustomization> DirectChannelCustomizations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var account = modelBuilder.Entity<Account>();
            account.HasKey(a => a.AccountId);
            account.Property(a => a.AccountId).ValueGeneratedNever();

            var channel = modelBuilder.Entity<Channel>();
            channel.HasKey(c => c.ChannelId);
            channel.Property(c => c.ChannelId).ValueGeneratedNever();
            channel.Property(c => c.ChannelType).HasConversion<byte>();

            var channelMember = modelBuilder.Entity<ChannelMember>();
            channelMember.HasKey(m => new { m.ChannelId, m.AccountId });
            channelMember.HasOne(m => m.Channel).WithMany(c => c.ChannelMembers).HasForeignKey(m => m.ChannelId);
            channelMember.HasOne(m => m.Account).WithMany(a => a.ChannelMemberships).HasForeignKey(m => m.AccountId);

            var message = modelBuilder.Entity<Message>();
            message.HasKey(m => m.MessageId);
            message.HasOne(m => m.Channel).WithMany(c => c.Messages).HasForeignKey(m => m.ChannelId);
            message.HasOne(m => m.Sender).WithMany(a => a.SentMessages).HasForeignKey(m => m.SenderId).IsRequired(false);
            message.Property(m => m.MessageId).ValueGeneratedNever();

            var messageDependency = modelBuilder.Entity<MessageDependency>();
            messageDependency.HasKey(d => d.AutoId);
            messageDependency.HasOne(d => d.OwningMessage).WithMany(m => m.Dependencies).HasForeignKey(d => d.OwningMessageId);
            messageDependency.HasOne(d => d.Message).WithMany(m => m.Dependants).HasForeignKey(d => d.MessageId);
            messageDependency.Property(d => d.AutoId).ValueGeneratedOnAdd();

            var passwordUpdate = modelBuilder.Entity<PasswordUpdate>();
            passwordUpdate.HasKey(p => p.MessageId);
            passwordUpdate.HasOne(p => p.Message).WithOne(m => m.PasswordUpdate).HasForeignKey<PasswordUpdate>(p => p.MessageId);

            var directChannelCustomization = modelBuilder.Entity<DirectChannelCustomization>();
            directChannelCustomization.HasKey(c => c.MessageId);
            directChannelCustomization.HasOne(c => c.Message).WithOne(m => m.DirectChannelCustomization)
                .HasForeignKey<DirectChannelCustomization>(c => c.MessageId);
            directChannelCustomization.Property(c => c.ProfileImageShape).HasConversion<byte>();

            var chatMessage = modelBuilder.Entity<ChatMessage>();
            chatMessage.HasKey(cm => cm.MessageId);
            chatMessage.HasOne(cm => cm.Message).WithOne(m => m.ChatMessage).HasForeignKey<ChatMessage>(cm => cm.MessageId);
            chatMessage.HasOne(cm => cm.QuotedMessage).WithMany(m => m.QuotingMessages).HasForeignKey(cm => cm.MessageId).IsRequired(false);
        }
    }
}
